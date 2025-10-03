using UnityEngine;

namespace StrategyGame.Grid.GridData {
    public enum WeaponType {
        Sword,
        Spear,
        Axe,
        Bow,
        Staff
    }


    [CreateAssetMenu(menuName = "Strategy Game/Grid Unit")]
    public class GridUnitData : GridEntityData {
        [SerializeField] private WeaponType weaponType;
        
        public WeaponType WeaponType { get => weaponType; }
    }

}

