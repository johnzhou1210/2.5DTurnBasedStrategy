using UnityEngine;

namespace StrategyGame.Grid.GridData {

    public struct TerrainStatModifier {
        public float FlatAttackModifier;
        public float PercentAttackModifier;
        public float FlatDefenseModifier;
        public float PercentDefenseModifier;
        public float FlatAgilityModifier;
        public float PercentAgilityModifier;
        public float FlatAccuracyModifier;
        public float PercentAccuracyModifier;
        public float FlatResistanceModifier;
        public float PercentResistanceModifier;
        public float FlatEvasionModifier;
        public float PercentEvasionModifier;
    }
    
    
    [CreateAssetMenu(menuName = "Strategy Game/Grid Tile")]
    public class TileData : ScriptableObject {
        [SerializeField] private int movementCost;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private TerrainStatModifier terrainStatModifier;
        
        public int MovementCost { get => movementCost; }
        public GameObject TilePrefab { get => tilePrefab; }
        public TerrainStatModifier TerrainStatModifier { get => terrainStatModifier; }
    }
}
