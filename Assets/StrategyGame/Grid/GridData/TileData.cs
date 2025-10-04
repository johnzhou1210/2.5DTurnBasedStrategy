using UnityEngine;

namespace StrategyGame.Grid.GridData {
    
    [CreateAssetMenu(menuName = "Strategy Game/Grid Tile")]
    public class TileData : ScriptableObject {
        [SerializeField] private int movementCost;
        [SerializeField] private GameObject tilePrefab;
        
        public int MovementCost { get => movementCost; set => movementCost = value; }
        public GameObject TilePrefab { get => tilePrefab; set => tilePrefab = value; }
    }
}
