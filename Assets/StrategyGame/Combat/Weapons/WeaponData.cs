using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Combat.Weapons {
    
    public enum DamageType {
        Physical,
        Magic
    }
    
    [CreateAssetMenu(menuName = "Strategy Game/Weapon")]
    public class WeaponData : ScriptableObject {
        [SerializeField] private WeaponType weaponType = WeaponType.Sword;
        [SerializeField] private int baseAttack = 5;
        [SerializeField] private int maxAttackRange = 1;
        [SerializeField] private int minAttackRange = 1;
        [SerializeField] private DamageType damageType = DamageType.Physical;

        public WeaponType WeaponType { get => weaponType; }
        public int BaseAttack { get => baseAttack; }
        public int MaxAttackRange { get => maxAttackRange; }
        public int MinAttackRange { get => minAttackRange; }
        public DamageType DamageType { get => damageType; }
    }
}
