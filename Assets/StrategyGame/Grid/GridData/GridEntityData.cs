using StrategyGame.Factions;
using UnityEngine;

namespace StrategyGame.Grid.GridData {
    public class GridEntityData : ScriptableObject {
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private int health;
        [SerializeField] private int maxHealth;
        [SerializeField] private FactionData factionData;
        [SerializeField] private int movementRange = 5;
        [SerializeField] private int visionRange = 5;

        public GameObject VisualPrefab { get => visualPrefab; }
        public int Health { get => health; }
        public int MaxHealth { get => maxHealth; }
        public FactionData FactionData { get => factionData; }
        public int MovementRange { get => movementRange; }
        public int VisionRange { get => visionRange; }
    }
}
