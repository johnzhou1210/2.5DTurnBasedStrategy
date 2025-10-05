using UnityEngine;

namespace StrategyGame.Grid {
    public class TileSelectable : MonoBehaviour {
        [SerializeField] private GameObject selectionVisual;
        [SerializeField] private GameObject routeTipVisual;
        [SerializeField] private GameObject routeStraightVisual;
        [SerializeField] private GameObject routeTurnVisual;

        [SerializeField] private Renderer renderer;
        
        public Vector2Int GridCoordinates { get; private set; }
        
        public void Initialize(Vector2Int position) {
            GridCoordinates = position;
            gameObject.name = $"Tile {GridCoordinates}";
        }

        public void SetSelectionVisualVisibility(bool val) {
            selectionVisual.SetActive(val);
        }
        
        // testing
        public void SetNeighborMarkVisualVisibility(bool val) {
            renderer.material.color = val ? Color.blue : Color.white;
        }
    }
}
