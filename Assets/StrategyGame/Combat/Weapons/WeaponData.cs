using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Combat.Weapons {
    
    public enum DamageType {
        Physical,
        Magic
    }
    
    [CreateAssetMenu(menuName = "Strategy Game/Weapon")]
    public class WeaponData : ScriptableObject {
        [SerializeField] private WeaponType weaponType;
        [SerializeField] private int baseAttack;
        [SerializeField] private int attackRange;
        [SerializeField] private DamageType damageType;

        public WeaponType WeaponType { get => weaponType; }
        public int BaseAttack { get => baseAttack; }
        public int AttackRange { get => attackRange; }
        public DamageType DamageType { get => damageType; }
    }
}
