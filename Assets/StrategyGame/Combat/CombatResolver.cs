using System;
using System.Collections.Generic;
using System.Linq;
using StrategyGame.Combat.Weapons;
using StrategyGame.Utils;
using UnityEngine;

namespace StrategyGame.Combat {
    public struct CombatPreview {
        public float AttackerHitChance;
        public float AttackerCritChance;
        public bool WillKillDefenderIfAllHitsLand;
        public bool WillKillDefenderIfAllHitsCrit;
        public int AttackerNumAttacks;
        public int AttackerNonCritDamagePerHit;
        public float AttackerChanceToKillDefender;

        public float DefenderHitChance;
        public float DefenderCritChance;
        public bool WillKillAttackerIfCounterLands;
        public bool WillKillAttackerIfCounterCrits;
        public int DefenderNumCounters;
        public int DefenderNonCritDamagePerHit;
        public float DefenderChanceToKillAttacker;

        public override string ToString() {
            return
                $@"===== COMBAT PREVIEW =====
                ATTACKER →
                  Hits: {AttackerNumAttacks}
                  Damage/Hit: {AttackerNonCritDamagePerHit}
                  Hit%: {AttackerHitChance:P0}
                  Crit%: {AttackerCritChance:P0}
                  Kill if all hit: {WillKillDefenderIfAllHitsLand}
                  Kill if all crit: {WillKillDefenderIfAllHitsCrit}
                  KO Chance: {AttackerChanceToKillDefender:P1}

                DEFENDER (Counter) →
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
        public bool Hit = false;
        public bool Crit = false;
        public int DamageDealt = 0;
        public bool DefenderDied = false;

        public bool CounterOccurs = false;
        public bool CounterHit = false;
        public bool CounterCrit = false;
        public int CounterDamageDealt = 0;
        public bool AttackerDied = false;

        public override string ToString() {
            return $"Hit: {Hit}, Crit: {Crit}, DamageDealt: {DamageDealt}, DefenderDied: {DefenderDied}, CounterOccurs: {CounterOccurs}";
        }
    }

    public class CombatStats {
        public int HP;
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
        public static CombatOutcome SimulateAttack(CombatStats attacker, CombatStats defender, AbilityData ability, bool attackerInDefenderRange) {
            CombatOutcome result = new CombatOutcome();

            // Get total flat and percentage boosts from ability and weapon
            StatModifier attackerAccuracyModifier = GetCombinedModifier(ModifierStat.Accuracy, ability, attacker.Weapon);
            StatModifier attackerAgilityModifier = GetCombinedModifier(ModifierStat.Agility, ability, attacker.Weapon);
            StatModifier attackerAttackModifier = GetCombinedModifier(ModifierStat.Attack, ability, attacker.Weapon);

            StatModifier defenderEvasionModifier = GetModifierOfWeapon(defender.Weapon, ModifierStat.Evasion) ?? new StatModifier();
            StatModifier defenderAgilityModifier = GetModifierOfWeapon(defender.Weapon, ModifierStat.Agility) ?? new StatModifier();
            StatModifier defenderDefenseModifier = GetModifierOfWeapon(defender.Weapon, ModifierStat.Defense) ?? new StatModifier();
            StatModifier defenderResistanceModifier = GetModifierOfWeapon(defender.Weapon, ModifierStat.Resistance) ?? new StatModifier();

            int attackerAccuracy = (int)((attacker.Accuracy * attackerAccuracyModifier.Percent) + attackerAccuracyModifier.Flat);
            int attackerAgility = (int)((attacker.Agility * attackerAgilityModifier.Percent) + attackerAgilityModifier.Flat);
            int attackerAttack = (int)(((attacker.Attack + attacker.Weapon.BaseAttack) * attackerAttackModifier.Percent) + attackerAttackModifier.Flat);
            Debug.Log($"CombatResolver.SimulateAttack: {ability}, {ability?.OverrideDamageType}, {ability?.DamageTypeOverride}, {attacker}, {attacker?.Weapon}, {attacker?.Weapon?.DamageType}");

            DamageType attackerDamageType = ability && ability.OverrideDamageType ? ability.DamageTypeOverride : attacker.Weapon != null ? attacker.Weapon.DamageType : DamageType.Physical;
            int defenderEvasion = (int)(defender.Evasion * defenderEvasionModifier.Percent + defenderEvasionModifier.Flat);
            int defenderAgility = (int)(defender.Agility * defenderAgilityModifier.Percent + defenderAgilityModifier.Flat);
            int defenderDefense = (int)(defender.Defense * defenderDefenseModifier.Percent + defenderDefenseModifier.Flat);
            int defenderResistance = (int)(defender.Resistance * defenderResistanceModifier.Percent + defenderResistanceModifier.Flat);

            /* HIT CHECK */
            float hitChance = _baseHitChance + ((((4f * attackerAccuracy + attackerAgility) / 2f) - ((4 * defenderEvasion + defenderAgility) / 2f)) / 100f);
            hitChance = Mathf.Clamp01(hitChance);
            result.Hit = _rng.Chance(hitChance);

            if (result.Hit) {
                /* CRIT CHECK */
                float critChance = _baseCritChance + (Mathf.Log(4 * attackerAccuracy + 2 * attackerAgility + 1) - Mathf.Log(4 * defenderEvasion + 2 * defenderAgility + 1)) / 11f;
                critChance = Mathf.Clamp(critChance, .01f, 1f);
                result.Crit = _rng.Chance(critChance);

                /* DAMAGE CALCULATION */
                int defense = attackerDamageType == DamageType.Physical ? defenderDefense : defenderResistance;
                float critMultiplier = result.Crit ? 1.5f : 1f;
                float defenseMultiplier = result.Crit ? 0.5f : 1f;

                Debug.Log($"Attacker attack: {attackerAttack}, Defender defense stat: {defense}");

                float damage = attackerAttack * critMultiplier - defense * defenseMultiplier;
                result.DamageDealt = Mathf.Max(1, Mathf.RoundToInt(damage));

                /* DEFENDER DEATH CHECK */
                int defenderHPAfter = Math.Max(0, defender.HP - result.DamageDealt);
                result.DefenderDied = defenderHPAfter == 0;
            }

            /* COUNTER CHECK */
            // Check from defender if attacker is within their attack range
            result.CounterOccurs = attackerInDefenderRange;
            return result;
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
            float chanceToKillDefender = CalculateKOChance(attackerHits, defender.HP, attackerNonCritDamage, attackerCritDamage, attackerHitChance, attackerCritChance);
            float chanceToKillAttacker = CalculateKOChance(defenderCounters, attacker.HP, defenderNonCritDamage, defenderCritDamage, defenderHitChance, defenderCritChance);
            if (defenderAlwaysDiesBeforeCounter) {
                defenderCounters = 0;
                chanceToKillAttacker = 0f;
            }
            
            return new CombatPreview {
                AttackerHitChance = attackerHitChance,
                AttackerCritChance = attackerCritChance,
                WillKillDefenderIfAllHitsLand = attackerNonCritDamage * attackerHits >= defender.HP,
                WillKillDefenderIfAllHitsCrit = attackerCritDamage * attackerHits >= defender.HP,
                AttackerNumAttacks = attackerHits,
                AttackerNonCritDamagePerHit = attackerNonCritDamage,
                AttackerChanceToKillDefender = chanceToKillDefender,

                DefenderHitChance = defenderHitChance,
                DefenderCritChance = defenderCritChance,
                WillKillAttackerIfCounterLands = defenderNonCritDamage * defenderCounters >= attacker.HP,
                WillKillAttackerIfCounterCrits = defenderCritDamage * defenderCounters >= attacker.HP,
                DefenderNumCounters = defenderCounters,
                DefenderNonCritDamagePerHit = defenderNonCritDamage,
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

        private static float CalculateKOChance(int numHits, int HP, int normalDamage, int critDamage, float hitChance, float critChance) {
            return KOChanceRecursive(numHits, HP, normalDamage, critDamage, hitChance, critChance, 1f);
        }

        private static float KOChanceRecursive(int hitsLeft, int remainingHP, int normalDamage, int critDamage, float hitChance, float critChance, float probability) {
            if (hitsLeft == 0) return remainingHP <= 0 ? probability : 0f;

            float chance = 0f;

            // Normal hit
            chance += KOChanceRecursive(hitsLeft - 1, remainingHP - normalDamage, normalDamage, critDamage, hitChance, critChance, probability * hitChance * (1 - critChance));

            // Crit hit
            chance += KOChanceRecursive(hitsLeft - 1, remainingHP - critDamage, normalDamage, critDamage, hitChance, critChance, probability * hitChance * critChance);

            // Miss
            chance += KOChanceRecursive(hitsLeft - 1, remainingHP, normalDamage, critDamage, hitChance, critChance, probability * (1 - hitChance));

            return chance;
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
            if (speedRatio <= 0.5f) {
                attackerHits = 1;
                defenderCounters = 2;
            }
            else if (speedRatio < 1.5f) {
                attackerHits = 1;
                defenderCounters = 1;
            }
            else if (speedRatio <= 2f) {
                attackerHits = 2;
                defenderCounters = 1;
            }
            else {
                attackerHits = 2;
                defenderCounters = 0;
            }
        }




    }


}
