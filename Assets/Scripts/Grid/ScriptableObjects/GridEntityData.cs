using UnityEngine;

namespace Grid {
    public enum WeaponType {
        SWORD,
        SPEAR,
        AXE,
        BOW,
        STAFF
    }


    [CreateAssetMenu(fileName = "New Grid Entity Data", menuName = "Grid Entity Data")]
    public class GridEntityData : ScriptableObject {
        public GameObject SpritePrefab;
        public int Health;
        public int MaxHealth;
        public WeaponType WeaponType;
    }

}

