using System;
using System.Collections.Generic;
using StrategyGame.Combat.Targeting;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Combat.Weapons {
    
    public enum DamageType {
        Physical,
        Magic
    }

    public enum WeaponMatchupResult {
        Neutral,
        Advantage,
        Disadvantage
    }
    
    [CreateAssetMenu(menuName = "Strategy Game/Weapon")]
    public class WeaponData : ScriptableObject {
        [SerializeField] private WeaponType weaponType = WeaponType.Sword;
        [SerializeField] private int baseAttack = 5;
        [SerializeField] private AttackRange attackRange;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField] private List<StatModifier> statModifiers = new();

        public WeaponType WeaponType { get => weaponType; }
        public int BaseAttack { get => baseAttack; }
        public AttackRange AttackRange { get => attackRange; }
        public DamageType DamageType { get => damageType; }
        public List<StatModifier> StatModifiers { get => statModifiers; }

        public static Sprite GetSpriteFromWeaponType(WeaponType type) {
            switch (type) {
                case WeaponType.Sword:
                    return Resources.Load<Sprite>("WeaponTypeIcons/Sword");
                case WeaponType.Spear:
                    return Resources.Load<Sprite>("WeaponTypeIcons/Spear");
                case WeaponType.Axe:
                    return Resources.Load<Sprite>("WeaponTypeIcons/Axe");
                case WeaponType.Bow:
                    return Resources.Load<Sprite>("WeaponTypeIcons/Bow");
                case WeaponType.Staff:
                    return Resources.Load<Sprite>("WeaponTypeIcons/Staff");
                default:
                    throw new Exception("WeaponData.GetSpriteFromWeaponType: Invalid WeaponType!");
            }
        }

        /// <summary>
        /// Checks the matchup of weapon a against weapon b.
        /// </summary>
        public static WeaponMatchupResult EvaluateWeaponMatchup(WeaponType a, WeaponType b) {
            if (a == WeaponType.Sword) {
                if (b == WeaponType.Axe) return WeaponMatchupResult.Advantage;
                if (b == WeaponType.Spear) return WeaponMatchupResult.Disadvantage;
                return WeaponMatchupResult.Neutral;
            }
            if (a == WeaponType.Spear) {
                if (b == WeaponType.Sword) return WeaponMatchupResult.Advantage;
                if (b == WeaponType.Axe) return WeaponMatchupResult.Disadvantage;
                return WeaponMatchupResult.Neutral;
            }
            if (a == WeaponType.Axe) {
                if (b == WeaponType.Sword) return WeaponMatchupResult.Disadvantage;
                if (b == WeaponType.Spear) return WeaponMatchupResult.Advantage;
                return WeaponMatchupResult.Neutral;
            }
            return WeaponMatchupResult.Neutral;
        }
    }
}
