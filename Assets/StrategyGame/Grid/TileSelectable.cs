using System;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Grid {
    public class TileSelectable : MonoBehaviour {
        [SerializeField] private GameObject selectionVisual;
        [SerializeField] private GameObject routeTipVisual;
        [SerializeField] private GameObject routeStraightVisual;
        [SerializeField] private GameObject routeTurnVisual;

        [SerializeField] private new Renderer renderer;
        [SerializeField] private Material glowMaterial;

        private Color _originalColor;
        private Material _originalMaterial;
        
        public Vector2Int GridCoordinates { get; private set; }
        
        public void Initialize(Vector2Int position) {
            GridCoordinates = position;
            gameObject.name = $"Tile {GridCoordinates}";
            _originalColor = GridDelegates.GetTileFromPosition(position).MovementCost > 100 ? Color.black : Color.gray;
            _originalMaterial = renderer.material;
            renderer.material.color = _originalColor;
        }

        public void SetSelectionVisualVisibility(bool val) {
            selectionVisual.SetActive(val);
        }
        
        // testing
        public void LightUp(bool val) {
            renderer.material = val ? glowMaterial : _originalMaterial;
        }

        // For when unit is selected
        public void SetWalkableMarkVisualVisibility(bool val) {
            if (_originalColor == Color.black) return;
            renderer.material.color = val ? Color.green : Color.gray;
        }

        public void Redraw() {
            TileData tileInitData = GridDelegates.GetTileFromPosition(GridCoordinates).InitData;
            if (tileInitData == null) throw new Exception("Redraw: Tile init data is null");
            _originalColor = tileInitData.MovementCost >=  100 ? Color.black : Color.white;
            renderer.material.color = _originalColor;
        }
        
        
        
    }
}
