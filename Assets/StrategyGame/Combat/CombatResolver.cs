using System;
using System.Collections.Generic;
using System.Linq;
using StrategyGame.Combat.Cinematics;
using StrategyGame.Combat.Weapons;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid.GridData;
using StrategyGame.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

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
        public int MaxAttackerNumAttacks;
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
        public int MaxDefenderNumCounters;
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
                  Can break target: {CombatResolver.HasWeaponAdvantage(AttackerWeapon.WeaponType, DefenderWeapon.WeaponType)}

                DEFENDER: {DefenderDisplayName} ({DefenderWeapon} ({DefenderWeapon.WeaponType})) (Counter) →
                  Counters: {DefenderNumCounters}
                  Damage/Counter: {DefenderNonCritDamagePerHit}
                  Hit%: {DefenderHitChance:P0}
                  Crit%: {DefenderCritChance:P0}
                  Kill if all hit: {WillKillAttackerIfCounterLands}
                  Kill if all crit: {WillKillAttackerIfCounterCrits}
                  KO Chance: {DefenderChanceToKillAttacker:P1}
                  Can break target: {CombatResolver.HasWeaponAdvantage(DefenderWeapon.WeaponType, AttackerWeapon.WeaponType)}
                ==========================";
        }
    }

    public class CombatOutcome {
        public List<CombatDirector.CombatTimeline> OrderOfEvents;
        public int AttackerSkillID = -1;
        public bool[] AttackHits = new[] { false, false };
        public bool[] AttackHitCrits = new[] { false, false };
        public bool[] DefendCounterHits = new[] { false, false };
        public bool[] DefendCounterCrits = new[] { false, false };
        public bool[] AttackBreakHits = new[] { false, false };
        public bool[] CounterBreakHits = new[] { false, false };
        public int[] AttackDamageInstances = new[] { 0, 0 };
        public int[] CounterDamageInstances = new[] { 0, 0 };

        public bool AttackerBrokenThisSimulation = false;
        public bool DefenderBrokenThisSimulation = false;
        
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
        public bool IsBrokenToBeginWith;
    }

    public enum ModifierStat {
        Attack,
        Defense,
        Agility,
        Accuracy,
        Resistance,
        Evasion,
        CritRate
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
        private static readonly Dictionary<WeaponType, WeaponType> AdvantageTable =
            new Dictionary<WeaponType, WeaponType>
            {
                { WeaponType.Sword, WeaponType.Axe },
                { WeaponType.Axe, WeaponType.Spear },
                { WeaponType.Spear, WeaponType.Sword },
            };

        
        private static float _baseHitChance = .85f;
        private static float _baseCritChance = .05f;
        private static DeterministicRNG _rng = new DeterministicRNG(Random.Range(0, 2048));
        
        public static bool HasWeaponAdvantage(WeaponType attacker, WeaponType defender) {
            if (!AdvantageTable.TryGetValue(attacker, out WeaponType value)) return false;
            return value == defender;
        }
        
        public static CombatOutcome ResolveCombatFromPreview(CombatPreview preview) {
            CombatOutcome outcome = new CombatOutcome();
            outcome.OrderOfEvents = new List<CombatDirector.CombatTimeline>();
            outcome.AttackerID = preview.AttackerID;
            outcome.DefenderID = preview.DefenderID;
            int defenderHP = preview.DefenderCurrentHP;
            int attackerHP = preview.AttackerCurrentHP;
            
            
            int attacksLeft = preview.MaxAttackerNumAttacks;
            int countersLeft = preview.MaxDefenderNumCounters;
            int currAttackIndex = 0;
            int currCounterIndex = 0;
            
            while (attacksLeft > 0 || countersLeft > 0) {
                /* Attacker does their thing */
                if (attacksLeft > 0 && defenderHP > 0) {
                    attacksLeft--;
                    outcome.NumAttacks++;
                    bool crit = _rng.Chance(preview.AttackerCritChance);
                    bool hit = _rng.Chance(preview.AttackerHitChance);
                    outcome.OrderOfEvents.Add(
                            crit ? (preview.AttackerWeapon.MinAttackRange > 1 ? CombatDirector.CombatTimeline.AttackerRangedCrit : CombatDirector.CombatTimeline.AttackerMeleeCrit)
                                : (preview.AttackerWeapon.MinAttackRange > 1 ? CombatDirector.CombatTimeline.AttackerRangedNormal : CombatDirector.CombatTimeline.AttackerMeleeNormal)
                        );
                    if (hit) {
                        int damage = crit ? preview.AttackerCritDamagePerHit : preview.AttackerNonCritDamagePerHit;
                        defenderHP -= damage;
                        outcome.AttackHits[currAttackIndex] = true;
                        outcome.AttackHitCrits[currAttackIndex] = crit;
                        outcome.AttackDamageInstances[currAttackIndex] = damage;
                        outcome.DamageDealt += damage;
                        
                        // Set countersLeft to 0 if attacker weapon is strong against defender weapon
                        if (HasWeaponAdvantage(preview.AttackerWeapon.WeaponType, preview.DefenderWeapon.WeaponType)) {
                            outcome.AttackBreakHits[currAttackIndex] = true;
                            outcome.DefenderBrokenThisSimulation = true;
                            countersLeft = 0;
                        }
                        
                        if (defenderHP <= 0) {
                            outcome.OrderOfEvents.Add(CombatDirector.CombatTimeline.DefenderDies);
                            outcome.DefenderDied = true;
                            countersLeft = 0;
                            attacksLeft = 0;
                        }
                    }
                    currAttackIndex++;
                }
                
                /* Defender does their thing */
                if (countersLeft > 0 && attackerHP > 0 && defenderHP > 0) {
                    countersLeft--;
                    outcome.NumCounters++;
                    bool crit = _rng.Chance(preview.DefenderCritChance);
                    bool hit = _rng.Chance(preview.DefenderHitChance);
                    outcome.OrderOfEvents.Add(
                        crit ? (preview.DefenderWeapon.MinAttackRange > 1 ? CombatDirector.CombatTimeline.DefenderRangedCrit : CombatDirector.CombatTimeline.DefenderMeleeCrit)
                            : (preview.DefenderWeapon.MinAttackRange > 1 ? CombatDirector.CombatTimeline.DefenderRangedNormal : CombatDirector.CombatTimeline.DefenderMeleeNormal)
                    );
                    if (hit) {
                        int damage = crit ? preview.DefenderCritDamagePerHit : preview.DefenderNonCritDamagePerHit;
                        attackerHP -= damage;
                        outcome.DefendCounterHits[currCounterIndex] = true;
                        outcome.DefendCounterCrits[currCounterIndex] = crit;
                        outcome.CounterDamageInstances[currCounterIndex] = damage;
                        outcome.CounterDamageDealt += damage;
                        
                        // Set attacksLeft to 0 if defender weapon is strong against attacker weapon
                        if (HasWeaponAdvantage(preview.DefenderWeapon.WeaponType, preview.AttackerWeapon.WeaponType)) {
                            outcome.CounterBreakHits[currCounterIndex] = true;
                            outcome.AttackerBrokenThisSimulation = true;
                            attacksLeft = 0;
                        }
                        
                        if (attackerHP <= 0) {
                            outcome.OrderOfEvents.Add(CombatDirector.CombatTimeline.AttackerDies);
                            outcome.AttackerDied = true;
                            attacksLeft = 0;
                            countersLeft = 0;
                        }
                    }
                    currCounterIndex++;
                }
                
            }
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
            float atkCrit = GetEffectiveStat(_baseCritChance, GetCombinedModifier(ModifierStat.CritRate, ability, attacker.Weapon));
            DamageType atkDamageType = ability && ability.OverrideDamageType ? ability.DamageTypeOverride : attacker.Weapon != null ? attacker.Weapon.DamageType : DamageType.Physical;

            int defAcc = GetEffectiveStat(defender.Accuracy, GetModifierOfWeapon(defender.Weapon, ModifierStat.Accuracy) ?? new StatModifier());
            int defAgi = GetEffectiveStat(defender.Agility, GetModifierOfWeapon(defender.Weapon, ModifierStat.Agility) ?? new StatModifier());
            int defAtk = GetEffectiveStat(defender.Attack + defender.Weapon.BaseAttack, GetModifierOfWeapon(defender.Weapon, ModifierStat.Attack) ?? new StatModifier());
            int defEvasion = GetEffectiveStat(defender.Evasion, GetModifierOfWeapon(defender.Weapon, ModifierStat.Evasion) ?? new StatModifier());
            int defDef = GetEffectiveStat(defender.Defense, GetModifierOfWeapon(defender.Weapon, ModifierStat.Defense) ?? new StatModifier());
            int defRes = GetEffectiveStat(defender.Resistance, GetModifierOfWeapon(defender.Weapon, ModifierStat.Resistance) ?? new StatModifier());
            float defCrit = GetEffectiveStat(_baseCritChance, GetModifierOfWeapon(defender.Weapon, ModifierStat.CritRate) ?? new StatModifier());
            DamageType defDamageType = defender.Weapon.DamageType;

            // Hit & Crit chances
            float attackerHitChance = GetHitChance(atkAcc, atkAgi, defEvasion, defAgi);
            float attackerCritChance = GetCritChance(atkAcc, atkAgi, defEvasion, defAgi, atkCrit);
            float defenderHitChance = GetHitChance(defAcc, defAgi, atkEvasion, atkAgi);
            float defenderCritChance = GetCritChance(defAcc, defAgi, atkEvasion, atkAgi, defCrit);

            // Hits & counters
            float speedRatio = (float)atkAgi / Mathf.Max(defAgi, 1);
            GetAttackAndCounterCount(speedRatio, out int attackerHits, out int defenderCounters);
            if (!attackerInDefenderRange || defender.IsBrokenToBeginWith) defenderCounters = 0;
            int maxAttackerHits = attackerHits;
            int maxDefenderCounters = defenderCounters;
            
            // Damage per hit
            int defenderEffectiveDefense = atkDamageType == DamageType.Physical ? defDef : defRes;
            int attackerEffectiveDefense = defDamageType == DamageType.Physical ? atkDef : atkRes;
            int attackerNonCritDamage = GetDamage(atkAtk, defenderEffectiveDefense, false);
            int attackerCritDamage = GetDamage(atkAtk, defenderEffectiveDefense, true);
            int defenderNonCritDamage = GetDamage(defAtk, attackerEffectiveDefense, false);
            int defenderCritDamage = GetDamage(defAtk, attackerEffectiveDefense, true);

            // KO chances
            float chanceToKillDefender = 0f;
            float chanceToKillAttacker = 0f;
            bool defenderAlwaysDiesBeforeCounter = Mathf.Approximately(attackerHitChance, 1) && attackerNonCritDamage >= defender.HP;

            if (defender.IsBrokenToBeginWith) maxDefenderCounters = 0;
            
            bool defenderBroken = defender.IsBrokenToBeginWith;
            bool attackerBroken = attacker.IsBrokenToBeginWith;
            
            SimulateCombatBranches(attackerHits, defenderCounters, attacker.HP, defender.HP, attackerBroken, defenderBroken, attacker.Weapon.WeaponType, defender.Weapon.WeaponType,
                attackerNonCritDamage, attackerCritDamage, attackerHitChance, attackerCritChance,
                defenderNonCritDamage, defenderCritDamage, defenderHitChance, defenderCritChance,
                1f, ref chanceToKillDefender, ref chanceToKillAttacker);
            
            if (defenderAlwaysDiesBeforeCounter) {
                defenderCounters = 0;
                chanceToKillAttacker = 0f;
            }
            
            // Since preview shows what happens if all attacks land as non crit, if attacks land, and attacker is super effective against defender, disable defender counter.
            if (HasWeaponAdvantage(attacker.Weapon.WeaponType, defender.Weapon.WeaponType)) {
                defenderCounters = 0;
            }
            if (defenderCounters > 0) {
                if (HasWeaponAdvantage(defender.Weapon.WeaponType, attacker.Weapon.WeaponType)) {
                    attackerHits = 1;
                }
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
                MaxAttackerNumAttacks = maxAttackerHits,
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
                MaxDefenderNumCounters = maxDefenderCounters,
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
            bool attackerBroken,
            bool defenderBroken,
            WeaponType attackerWeaponType,
            WeaponType defenderWeaponType,
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
            if (probability <= 0.000001f) return;
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
            if (atkHitsLeft > 0 && !attackerBroken) {
                // Miss
                SimulateCombatBranches(
                    atkHitsLeft - 1, defHitsLeft,
                    attackerHP, defenderHP, false, defenderBroken, attackerWeaponType, defenderWeaponType,
                    atkNormal, atkCrit, atkHitChance, atkCritChance,
                    defNormal, defCrit, defHitChance, defCritChance,
                    probability * (1 - atkHitChance),
                    ref attackerKills, ref defenderKills);

                
                bool newDefenderBroken = defenderBroken || HasWeaponAdvantage(attackerWeaponType, defenderWeaponType);

                // Normal hit
                SimulateCombatBranches(
                    atkHitsLeft - 1, defHitsLeft,
                    attackerHP, defenderHP - atkNormal, false, newDefenderBroken, attackerWeaponType, defenderWeaponType,
                    atkNormal, atkCrit, atkHitChance, atkCritChance,
                    defNormal, defCrit, defHitChance, defCritChance,
                    probability * atkHitChance * (1 - atkCritChance),
                    ref attackerKills, ref defenderKills);

                // Crit
                SimulateCombatBranches(
                    atkHitsLeft - 1, defHitsLeft,
                    attackerHP, defenderHP - atkCrit, false, newDefenderBroken, attackerWeaponType, defenderWeaponType,
                    atkNormal, atkCrit, atkHitChance, atkCritChance,
                    defNormal, defCrit, defHitChance, defCritChance,
                    probability * atkHitChance * atkCritChance,
                    ref attackerKills, ref defenderKills);

                return;
            }

            // Defender counter phase
            if (defHitsLeft > 0 && !defenderBroken) {
                if (HasWeaponAdvantage(defenderWeaponType, attackerWeaponType)) {
                    attackerBroken = true;
                }
                // Miss
                SimulateCombatBranches(
                    atkHitsLeft, defHitsLeft - 1,
                    attackerHP, defenderHP, attackerBroken, false, attackerWeaponType, defenderWeaponType,
                    atkNormal, atkCrit, atkHitChance, atkCritChance,
                    defNormal, defCrit, defHitChance, defCritChance,
                    probability * (1 - defHitChance),
                    ref attackerKills, ref defenderKills);

                bool newAttackerBroken = attackerBroken || HasWeaponAdvantage(defenderWeaponType, attackerWeaponType);
                
                // Normal
                SimulateCombatBranches(
                    atkHitsLeft, defHitsLeft - 1,
                    attackerHP - defNormal, defenderHP,  newAttackerBroken, false, attackerWeaponType, defenderWeaponType,
                    atkNormal, atkCrit, atkHitChance, atkCritChance,
                    defNormal, defCrit, defHitChance, defCritChance,
                    probability * defHitChance * (1 - defCritChance),
                    ref attackerKills, ref defenderKills);

                // Crit
                SimulateCombatBranches(
                    atkHitsLeft, defHitsLeft - 1,
                    attackerHP - defCrit, defenderHP,  newAttackerBroken, false, attackerWeaponType, defenderWeaponType,
                    atkNormal, atkCrit, atkHitChance, atkCritChance,
                    defNormal, defCrit, defHitChance, defCritChance,
                    probability * defHitChance * defCritChance,
                    ref attackerKills, ref defenderKills);
            }
        }



        private static int GetEffectiveStat(int baseStat, StatModifier modifier) => Mathf.RoundToInt(baseStat * modifier.Percent + modifier.Flat);

        private static float GetEffectiveStat(float baseStat, StatModifier modifier) => baseStat * modifier.Percent + modifier.Flat;

        private static float GetHitChance(int attackerAccuracy, int attackerAgility, int defenderEvasion, int defenderAgility) =>
            Mathf.Clamp01(_baseHitChance + ((((4f * attackerAccuracy + attackerAgility) / 2f) - ((4 * defenderEvasion + defenderAgility) / 2f)) / 100f));

        private static float GetCritChance(int attackerAccuracy, int attackerAgility, int defenderEvasion, int defenderAgility, float attackerBaseCrit) {
            float atkTerm = Mathf.Max(1f, 4f * attackerAccuracy + 2f * attackerAgility + 1f);
            float defTerm = Mathf.Max(1f, 4f * defenderEvasion + 2f * defenderAgility + 1f);
            float crit = attackerBaseCrit + (Mathf.Log(atkTerm) - Mathf.Log(defTerm)) / 11f;
            return Mathf.Clamp(crit, 0f, 1f);
        }

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
