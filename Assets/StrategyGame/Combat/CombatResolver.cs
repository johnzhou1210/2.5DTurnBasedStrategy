using System;
using System.Collections.Generic;
using System.Linq;
using StrategyGame.Combat.Weapons;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid;
using StrategyGame.Utils;
using UnityEngine;

namespace StrategyGame.Combat {
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

            int attackerAccuracy = (int)((attacker.Accuracy * attackerAccuracyModifier.Percent) + attackerAccuracyModifier.Flat);
            int attackerAgility = (int)((attacker.Agility * attackerAgilityModifier.Percent) + attackerAgilityModifier.Flat);
            int attackerAttack = (int)(((attacker.Attack + attacker.Weapon.BaseAttack) * attackerAttackModifier.Percent) + attackerAttackModifier.Flat);
            Debug.Log($"CombatResolver.SimulateAttack: {ability}, {ability?.OverrideDamageType}, {ability?.DamageTypeOverride}, {attacker}, {attacker?.Weapon}, {attacker?.Weapon?.DamageType}");
            
            DamageType attackerDamageType = ability && ability.OverrideDamageType ? ability.DamageTypeOverride : attacker.Weapon != null ? attacker.Weapon.DamageType : DamageType.Physical;
            int defenderEvasion = (int)(defender.Evasion * defenderEvasionModifier.Percent + defenderEvasionModifier.Flat);
            int defenderAgility = (int)(defender.Agility * defenderAgilityModifier.Percent + defenderAgilityModifier.Flat);

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
                int defense = attackerDamageType == DamageType.Physical ? defender.Defense : defender.Resistance;
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

    }



}
