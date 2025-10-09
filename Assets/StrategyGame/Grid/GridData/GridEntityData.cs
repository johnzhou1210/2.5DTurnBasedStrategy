using StrategyGame.Factions;
using UnityEngine;

namespace StrategyGame.Grid.GridData {
    public class GridEntityData : ScriptableObject {
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private int baseHealth = 5;
        [SerializeField] private FactionData factionData;
        [SerializeField] private int movementRange = 5;
        [SerializeField] private int visionRange = 5;
        [SerializeField] private int baseAttack = 5;
        [SerializeField] private int baseAccuracy = 5;
        [SerializeField] private int baseDefense = 5;
        [SerializeField] private int baseResistance = 5;
        [SerializeField] private int baseAgility = 5;
        [SerializeField] private int baseEvasion = 5;

        public GameObject VisualPrefab { get => visualPrefab; }
        public int BaseHealth { get => baseHealth; }
        public FactionData FactionData { get => factionData; }
        public int MovementRange { get => movementRange; }
        public int VisionRange { get => visionRange; }

        public int BaseAttack { get => baseAttack; }
        public int BaseAccuracy { get => baseAccuracy; }
        public int BaseDefense { get => baseDefense; }
        public int BaseResistance { get => baseResistance; }
        public int BaseAgility { get => baseAgility; }
        public int BaseEvasion { get => baseEvasion; }
    }
}
