using StrategyGame.Core.Delegates;
using UnityEngine;

namespace StrategyGame.Grid {
    public class TileSelectable : MonoBehaviour {
        [SerializeField] private GameObject selectionVisual;
        [SerializeField] private GameObject routeTipVisual;
        [SerializeField] private GameObject routeStraightVisual;
        [SerializeField] private GameObject routeTurnVisual;

        [SerializeField] private new Renderer renderer;

        private Color _originalColor;
        
        public Vector2Int GridCoordinates { get; private set; }
        
        public void Initialize(Vector2Int position) {
            GridCoordinates = position;
            gameObject.name = $"Tile {GridCoordinates}";
            _originalColor = GridDelegates.GetTileFromPosition(position).MovementCost > 100 ? Color.black : Color.white;
            renderer.material.color = _originalColor;
        }

        public void SetSelectionVisualVisibility(bool val) {
            selectionVisual.SetActive(val);
        }
        
        // testing
        public void SetNeighborMarkVisualVisibility(bool val) {
            renderer.material.color = val ? (_originalColor == Color.black ? Color.red : Color.blue) : _originalColor;
        }

        public void SetWalkableMarkVisualVisibility(bool val) {
            renderer.material.color = val ? Color.green : Color.gray;
        }
    }
}
