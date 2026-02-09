using System;
using System.Collections.Generic;
using System.Linq;
using StrategyGame.Combat.Cinematics;
using StrategyGame.Combat.Weapons;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid.GridData;
using StrategyGame.Utils;
using UnityEngine;

namespace StrategyGame.Combat {
    public struct CombatPreview {
        public int AttackerID;
        public int AttackerCurrentHP;
        public int AttackerMaxHP;
        public string AttackerDisplayName;
        public WeaponData AttackerWeapon;
        public float AttackerHitChance;
        public float AttackerCritChance;
        public bool WillKillDefenderIfAllHitsLand;
        public bool WillKillDefenderIfAllHitsCrit;
        public int AttackerNumAttacks;
        public int AttackerNonCritDamagePerHit;
        public int AttackerCritDamagePerHit;
        public float AttackerChanceToKillDefender;

        public int DefenderID;
        public int DefenderCurrentHP;
        public int DefenderMaxHP;
        public string DefenderDisplayName;
        public WeaponData DefenderWeapon;
        public float DefenderHitChance;
        public float DefenderCritChance;
        public bool WillKillAttackerIfCounterLands;
        public bool WillKillAttackerIfCounterCrits;
        public int DefenderNumCounters;
        public int DefenderNonCritDamagePerHit;
        public int DefenderCritDamagePerHit;
        public float DefenderChanceToKillAttacker;

        public override string ToString() {
            return
                $@"===== COMBAT PREVIEW =====
                ATTACKER: {AttackerDisplayName} ({AttackerWeapon} ({AttackerWeapon.WeaponType})) →
                  Hits: {AttackerNumAttacks}
                  Damage/Hit: {AttackerNonCritDamagePerHit}
                  Hit%: {AttackerHitChance:P0}
                  Crit%: {AttackerCritChance:P0}
                  Kill if all hit: {WillKillDefenderIfAllHitsLand}
                  Kill if all crit: {WillKillDefenderIfAllHitsCrit}
                  KO Chance: {AttackerChanceToKillDefender:P1}

                DEFENDER: {DefenderDisplayName} ({DefenderWeapon} ({DefenderWeapon.WeaponType})) (Counter) →
                  Counters: {DefenderNumCounters}
                  Damage/Counter: {DefenderNonCritDamagePerHit}
                  Hit%: {DefenderHitChance:P0}
                  Crit%: {DefenderCritChance:P0}
                  Kill if all hit: {WillKillAttackerIfCounterLands}
                  Kill if all crit: {WillKillAttackerIfCounterCrits}
                  KO Chance: {DefenderChanceToKillAttacker:P1}
                ==========================";
        }
    }

    public class CombatOutcome {
        public List<CombatDirector.CombatTimeline> OrderOfEvents;
        
        public bool[] AttackHits = new[] { false, false };
        public bool[] AttackHitCrits = new[] { false, false };
        public bool[] DefendCounterHits = new[] { false, false };
        public bool[] DefendCounterCrits = new[] { false, false };
        public int[] AttackDamageInstances = new[] { 0, 0 };
        public int[] CounterDamageInstances = new[] { 0, 0 };
        
        public int DamageDealt = 0;
        public bool DefenderDied = false;

        public int NumCounters;
        public int NumAttacks;

        public int CounterDamageDealt = 0;
        public bool AttackerDied = false;

        public int AttackerID;
        public int DefenderID;

        public override string ToString() {
            return $"AttackHits: {string.Join(",", AttackHits)}, AttackHitCrits: {string.Join(",", AttackHitCrits)}, DefendCounterHits: {string.Join(",", DefendCounterHits)}, DefendCounterCrits: {string.Join(",", DefendCounterCrits)}\n" +
                $"  DamageDealt: {DamageDealt}, DefenderDied: {DefenderDied}, NumAttacks: {NumAttacks}, NumCounters: {NumCounters}, CounterDamageDealt: {CounterDamageDealt}, AttackerDied: {AttackerDied}";
        }
    }

    public class CombatStats {
        public int HP;
        public int MaxHP;
        public int Attack;
        public int Defense;
        public int Agility;
        public int Accuracy;
        public int Resistance;
        public int Evasion;
        public WeaponData Weapon;
        public int EntityID = -1;
    }

    public enum ModifierStat {
        Attack,
        Defense,
        Agility,
        Accuracy,
        Resistance,
        Evasion
    }

    [System.Serializable]
    public class StatModifier {
        public ModifierStat ModifierStat;
        public float Flat = 0;
        public float Percent = 1; // Where 1 is 100%
    }

    [CreateAssetMenu(menuName = "Strategy Game/Ability")]
    public class AbilityData : ScriptableObject {
        public List<StatModifier> StatModifiers;
        public bool OverrideDamageType = false;
        public DamageType DamageTypeOverride;
    }

    public static class CombatResolver {
        private static float _baseHitChance = .85f;
        private static float _baseCritChance = .05f;
        private static DeterministicRNG _rng = new DeterministicRNG(0);
        public static CombatOutcome ResolveCombatFromPreview(CombatPreview preview) {
            CombatOutcome outcome = new CombatOutcome();
            outcome.OrderOfEvents = new  List<CombatDirector.CombatTimeline>();
            outcome.AttackerID = preview.AttackerID;
            outcome.DefenderID = preview.DefenderID;
            int defenderHP = preview.DefenderCurrentHP;
            int attackerHP = preview.AttackerCurrentHP;
            int attackerActualNumAttacks = 0;
            int defenderActualNumCounters = 0;

            /* Attacker does their thing */
            for (int i = 0; i < preview.AttackerNumAttacks; i++) {
                if (defenderHP <= 0) break;
                attackerActualNumAttacks += 1;
                bool crit = _rng.Chance(preview.AttackerCritChance);
                bool hit = _rng.Chance(preview.AttackerHitChance);
                if (crit) {
                    outcome.OrderOfEvents.Add(preview.AttackerWeapon.MinAttackRange > 1 ? CombatDirector.CombatTimeline.AttackerRangedCrit : CombatDirector.CombatTimeline.AttackerMeleeCrit);
                } else {
                    outcome.OrderOfEvents.Add(preview.AttackerWeapon.MinAttackRange > 1 ? CombatDirector.CombatTimeline.AttackerRangedNormal : CombatDirector.CombatTimeline.AttackerMeleeNormal);
                }
                
                if (!hit) continue;
                int damage = crit
                    ? preview.AttackerCritDamagePerHit
                    : preview.AttackerNonCritDamagePerHit;

                defenderHP -= damage;

                if (defenderHP <= 0) {
                    outcome.OrderOfEvents.Add(CombatDirector.CombatTimeline.DefenderDies);
                }
                
                outcome.AttackHits[i] = true;
                outcome.AttackHitCrits[i] = crit;
                outcome.DamageDealt += damage;
                outcome.AttackDamageInstances[i] = damage;
            }

            outcome.DefenderDied = defenderHP <= 0;
            outcome.NumAttacks = attackerActualNumAttacks;

            /* Defender does their thing */
            if (!outcome.DefenderDied && preview.DefenderNumCounters > 0) {
                for (int i = 0; i < preview.DefenderNumCounters; i++) {
                    if (attackerHP <= 0) break;
                    defenderActualNumCounters += 1;

                    bool crit = _rng.Chance(preview.DefenderCritChance);
                    bool hit = _rng.Chance(preview.DefenderHitChance);
                    if (crit) {
                        outcome.OrderOfEvents.Add(preview.DefenderWeapon.MinAttackRange > 1 ? CombatDirector.CombatTimeline.DefenderRangedCrit : CombatDirector.CombatTimeline.DefenderMeleeCrit);
                    } else {
                        outcome.OrderOfEvents.Add(preview.DefenderWeapon.MinAttackRange > 1 ? CombatDirector.CombatTimeline.DefenderRangedNormal : CombatDirector.CombatTimeline.DefenderMeleeNormal);
                    }
                    
                    if (!hit) continue;
                    int damage = crit
                        ? preview.DefenderCritDamagePerHit
                        : preview.DefenderNonCritDamagePerHit;

                    attackerHP -= damage;
                    
                    if (attackerHP <= 0) {
                        outcome.OrderOfEvents.Add(CombatDirector.CombatTimeline.AttackerDies);
                    }
                    
                    outcome.DefendCounterHits[i] = true;
                    outcome.DefendCounterCrits[i] = crit;
                    outcome.CounterDamageDealt += damage;
                    outcome.CounterDamageInstances[i] = damage;
                }
            }

            outcome.AttackerDied = attackerHP <= 0;
            outcome.NumCounters = defenderActualNumCounters;
            return outcome;
        }


        public static CombatPreview SimulateAttackPreview(CombatStats attacker, CombatStats defender, AbilityData ability, bool attackerInDefenderRange) {
            // Compute effective stats
            int atkAcc = GetEffectiveStat(attacker.Accuracy, GetCombinedModifier(ModifierStat.Accuracy, ability, attacker.Weapon));
            int atkAgi = GetEffectiveStat(attacker.Agility, GetCombinedModifier(ModifierStat.Agility, ability, attacker.Weapon));
            int atkAtk = GetEffectiveStat(attacker.Attack + attacker.Weapon.BaseAttack, GetCombinedModifier(ModifierStat.Attack, ability, attacker.Weapon));
            int atkEvasion = GetEffectiveStat(attacker.Evasion, GetCombinedModifier(ModifierStat.Evasion, ability, attacker.Weapon));
            int atkDef = GetEffectiveStat(attacker.Defense, GetCombinedModifier(ModifierStat.Defense, ability, attacker.Weapon));
            int atkRes = GetEffectiveStat(attacker.Resistance, GetCombinedModifier(ModifierStat.Resistance, ability, attacker.Weapon));
            DamageType atkDamageType = ability && ability.OverrideDamageType ? ability.DamageTypeOverride : attacker.Weapon != null ? attacker.Weapon.DamageType : DamageType.Physical;

            int defAcc = GetEffectiveStat(defender.Accuracy, GetModifierOfWeapon(defender.Weapon, ModifierStat.Accuracy) ?? new StatModifier());
            int defAgi = GetEffectiveStat(defender.Agility, GetModifierOfWeapon(defender.Weapon, ModifierStat.Agility) ?? new StatModifier());
            int defAtk = GetEffectiveStat(defender.Attack + defender.Weapon.BaseAttack, GetModifierOfWeapon(defender.Weapon, ModifierStat.Attack) ?? new StatModifier());
            int defEvasion = GetEffectiveStat(defender.Evasion, GetModifierOfWeapon(defender.Weapon, ModifierStat.Evasion) ?? new StatModifier());
            int defDef = GetEffectiveStat(defender.Defense, GetModifierOfWeapon(defender.Weapon, ModifierStat.Defense) ?? new StatModifier());
            int defRes = GetEffectiveStat(defender.Resistance, GetModifierOfWeapon(defender.Weapon, ModifierStat.Resistance) ?? new StatModifier());
            DamageType defDamageType = defender.Weapon.DamageType;

            // Hit & Crit chances
            float attackerHitChance = GetHitChance(atkAcc, atkAgi, defEvasion, defAgi);
            float attackerCritChance = GetCritChance(atkAcc, atkAgi, defEvasion, defAgi);
            float defenderHitChance = GetHitChance(defAcc, defAgi, atkEvasion, atkAgi);
            float defenderCritChance = GetCritChance(defAcc, defAgi, atkEvasion, atkAgi);

            // Hits & counters
            float speedRatio = (float)atkAgi / Mathf.Max(defAgi, 1);
            GetAttackAndCounterCount(speedRatio, out int attackerHits, out int defenderCounters);
            if (!attackerInDefenderRange) defenderCounters = 0;

            // Damage per hit
            int defenderEffectiveDefense = atkDamageType == DamageType.Physical ? defDef : defRes;
            int attackerEffectiveDefense = defDamageType == DamageType.Physical ? atkDef : atkRes;
            int attackerNonCritDamage = GetDamage(atkAtk, defenderEffectiveDefense, false);
            int attackerCritDamage = GetDamage(atkAtk, defenderEffectiveDefense, true);
            int defenderNonCritDamage = GetDamage(defAtk, attackerEffectiveDefense, false);
            int defenderCritDamage = GetDamage(defAtk, attackerEffectiveDefense, true);

            // KO chances
            bool defenderAlwaysDiesBeforeCounter = attackerNonCritDamage * attackerHits >= defender.HP;

            float chanceToKillDefender = 0f;
            float chanceToKillAttacker = 0f;
            
            SimulateCombatBranches(attackerHits, defenderCounters, attacker.HP, defender.HP,
                attackerNonCritDamage, attackerCritDamage, attackerHitChance, attackerCritChance,
                defenderNonCritDamage, defenderCritDamage, defenderHitChance, defenderCritChance,
                1f, ref chanceToKillDefender, ref chanceToKillAttacker);
            
            if (defenderAlwaysDiesBeforeCounter) {
                defenderCounters = 0;
                chanceToKillAttacker = 0f;
            }
            return new CombatPreview {
                AttackerID = attacker.EntityID,
                AttackerCurrentHP = attacker.HP,
                AttackerMaxHP = attacker.MaxHP,
                AttackerDisplayName = EntityDelegates.GetGridEntityByID(attacker.EntityID).DisplayName,
                AttackerWeapon = attacker.Weapon,
                AttackerHitChance = attackerHitChance,
                AttackerCritChance = attackerCritChance,
                WillKillDefenderIfAllHitsLand = attackerNonCritDamage * attackerHits >= defender.HP,
                WillKillDefenderIfAllHitsCrit = attackerCritDamage * attackerHits >= defender.HP,
                AttackerNumAttacks = attackerHits,
                AttackerNonCritDamagePerHit = attackerNonCritDamage,
                AttackerCritDamagePerHit = attackerCritDamage,
                AttackerChanceToKillDefender = chanceToKillDefender,

                DefenderID = defender.EntityID,
                DefenderCurrentHP = defender.HP,
                DefenderMaxHP = defender.MaxHP,
                DefenderDisplayName = EntityDelegates.GetGridEntityByID(defender.EntityID).DisplayName,
                DefenderWeapon = defender.Weapon,
                DefenderHitChance = defenderHitChance,
                DefenderCritChance = defenderCritChance,
                WillKillAttackerIfCounterLands = defenderNonCritDamage * defenderCounters >= attacker.HP,
                WillKillAttackerIfCounterCrits = defenderCritDamage * defenderCounters >= attacker.HP,
                DefenderNumCounters = defenderCounters,
                DefenderNonCritDamagePerHit = defenderNonCritDamage,
                DefenderCritDamagePerHit = defenderCritDamage,
                DefenderChanceToKillAttacker = chanceToKillAttacker
            };
        }



        private static StatModifier GetCombinedModifier(ModifierStat stat, AbilityData ability, WeaponData weapon) {
            float flat = 0;
            float percent = 1f;
            if (ability != null) {
                foreach (var mod in ability.StatModifiers.Where(m => m.ModifierStat == stat)) {
                    flat += mod.Flat;
                    percent *= mod.Percent;
                }
            }
            if (weapon != null) {
                foreach (var mod in weapon.StatModifiers.Where(m => m.ModifierStat == stat)) {
                    flat += mod.Flat;
                    percent *= mod.Percent;
                }
            }
            return new StatModifier { Flat = flat, Percent = percent };
        }

        private static StatModifier GetModifierOfWeapon(WeaponData weaponData, ModifierStat stat) {
            if (weaponData?.StatModifiers == null) return null;
            return weaponData.StatModifiers.FirstOrDefault(m => m.ModifierStat == stat);
        }

        private static void SimulateCombatBranches(
            int atkHitsLeft,
            int defHitsLeft,
            int attackerHP,
            int defenderHP,
            int atkNormal,
            int atkCrit,
            float atkHitChance,
            float atkCritChance,
            int defNormal,
            int defCrit,
            float defHitChance,
            float defCritChance,
            float probability,
            ref float attackerKills,
            ref float defenderKills
        ) {
            // Stop if someone already dead
            if (attackerHP <= 0) {
                defenderKills += probability;
                return;
            }
            if (defenderHP <= 0) {
                attackerKills += probability;
                return;
            }

            // Attacker phase first
            if (atkHitsLeft > 0) {
                // Miss
                SimulateCombatBranches(
                    atkHitsLeft - 1, defHitsLeft,
                    attackerHP, defenderHP,
                    atkNormal, atkCrit, atkHitChance, atkCritChance,
                    defNormal, defCrit, defHitChance, defCritChance,
                    probability * (1 - atkHitChance),
                    ref attackerKills, ref defenderKills);

                // Normal hit
                SimulateCombatBranches(
                    atkHitsLeft - 1, defHitsLeft,
                    attackerHP, defenderHP - atkNormal,
                    atkNormal, atkCrit, atkHitChance, atkCritChance,
                    defNormal, defCrit, defHitChance, defCritChance,
                    probability * atkHitChance * (1 - atkCritChance),
                    ref attackerKills, ref defenderKills);

                // Crit
                SimulateCombatBranches(
                    atkHitsLeft - 1, defHitsLeft,
                    attackerHP, defenderHP - atkCrit,
                    atkNormal, atkCrit, atkHitChance, atkCritChance,
                    defNormal, defCrit, defHitChance, defCritChance,
                    probability * atkHitChance * atkCritChance,
                    ref attackerKills, ref defenderKills);

                return;
            }

            // Defender counter phase
            if (defHitsLeft > 0) {
                // Miss
                SimulateCombatBranches(
                    atkHitsLeft, defHitsLeft - 1,
                    attackerHP, defenderHP,
                    atkNormal, atkCrit, atkHitChance, atkCritChance,
                    defNormal, defCrit, defHitChance, defCritChance,
                    probability * (1 - defHitChance),
                    ref attackerKills, ref defenderKills);

                // Normal
                SimulateCombatBranches(
                    atkHitsLeft, defHitsLeft - 1,
                    attackerHP - defNormal, defenderHP,
                    atkNormal, atkCrit, atkHitChance, atkCritChance,
                    defNormal, defCrit, defHitChance, defCritChance,
                    probability * defHitChance * (1 - defCritChance),
                    ref attackerKills, ref defenderKills);

                // Crit
                SimulateCombatBranches(
                    atkHitsLeft, defHitsLeft - 1,
                    attackerHP - defCrit, defenderHP,
                    atkNormal, atkCrit, atkHitChance, atkCritChance,
                    defNormal, defCrit, defHitChance, defCritChance,
                    probability * defHitChance * defCritChance,
                    ref attackerKills, ref defenderKills);
            }
        }



        private static int GetEffectiveStat(int baseStat, StatModifier modifier) => Mathf.RoundToInt(baseStat * modifier.Percent + modifier.Flat);

        private static float GetHitChance(int attackerAccuracy, int attackerAgility, int defenderEvasion, int defenderAgility) =>
            Mathf.Clamp01(_baseHitChance + ((((4f * attackerAccuracy + attackerAgility) / 2f) - ((4 * defenderEvasion + defenderAgility) / 2f)) / 100f));

        private static float GetCritChance(int attackerAccuracy, int attackerAgility, int defenderEvasion, int defenderAgility) =>
            Mathf.Clamp(_baseCritChance + (Mathf.Log(4 * attackerAccuracy + 2 * attackerAgility + 1) - Mathf.Log(4 * defenderEvasion + 2 * defenderAgility + 1)) / 11f, .01f, 1f);

        private static int GetDamage(int attack, int defense, bool crit, float critMultiplier = 1.5f, float critDefenseMultiplier = 0.5f) {
            float damage = crit ? attack * critMultiplier - defense * critDefenseMultiplier : attack - defense;
            return Mathf.Max(1, Mathf.RoundToInt(damage));
        }

        private static void GetAttackAndCounterCount(float speedRatio, out int attackerHits, out int defenderCounters) {
            switch (speedRatio) {
                case <= 0.5f:
                    attackerHits = 1;
                    defenderCounters = 2;
                    break;
                case < 1.5f:
                    attackerHits = 1;
                    defenderCounters = 1;
                    break;
                case <= 2f:
                    attackerHits = 2;
                    defenderCounters = 1;
                    break;
                default:
                    attackerHits = 2;
                    defenderCounters = 0;
                    break;
            }
        }




    }


}
