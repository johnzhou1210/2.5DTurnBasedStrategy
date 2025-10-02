using UnityEngine;

namespace StrategyGame.Grid.GridData {
    public enum WeaponType {
        Sword,
        Spear,
        Axe,
        Bow,
        Staff
    }


    [CreateAssetMenu(fileName = "New Grid Unit Data", menuName = "Grid Unit Data")]
    public class GridUnitData : GridEntityData {
        [SerializeField] private WeaponType weaponType;
        
        public WeaponType WeaponType { get => weaponType; }
    }

}

