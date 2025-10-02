using UnityEngine;

namespace StrategyGame.Grid.GridData {
    [CreateAssetMenu(fileName = "New Grid Unit Data", menuName = "Grid Unit Data")]
    public class GridEntityData : ScriptableObject {
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private int health;
        [SerializeField] private int maxHealth;

        public GameObject VisualPrefab { get => visualPrefab; }
        public int Health { get => health; }
        public int MaxHealth { get => maxHealth; }
    }
}
