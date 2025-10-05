using System;
using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid.GridData;
using StrategyGame.UI.World;
using UnityEngine;

namespace StrategyGame.Grid {
    public class GridRenderer : MonoBehaviour {
        private GameObject[,] _tileVisuals;
        [SerializeField] private GridManager grid;
        
        private void OnEnable() {
            _tileVisuals = new GameObject[grid.GetSize().x, grid.GetSize().y];

            GridDelegates.OnSetSelectedTile += UpdateSelectedTileVisuals;
        }
        
        public void OnGridRedraw() {
            Vector2Int dimensions = grid.GetSize();
            for (int y = 0; y < dimensions.y; y++) {
                for (int x = 0; x < dimensions.x; x++) {
                    Vector3 position = new Vector3(x, 0f, y);
                    GameObject tilePrefab = grid.Tiles[x, y].InitData.TilePrefab;
                    GameObject newTile = Instantiate(tilePrefab, transform);
                    newTile.transform.position = position;
                    _tileVisuals[x, y] = newTile;
                    if (newTile.TryGetComponent(out TileSelectable selectable)) {
                        selectable.Initialize(new Vector2Int(x,y));
                    }
                }
            }
        }

        private void UpdateSelectedTileVisuals(Tile oldTile, Tile newTile) {
            if (oldTile != null) {
                // Hide old tile selection visual
                GameObject oldTileVisual = _tileVisuals[oldTile.Position.x, oldTile.Position.y];
                if (oldTileVisual == null) { throw new Exception("Old tile visual not found!");}
                if (oldTileVisual.TryGetComponent(out TileSelectable oldSelectable)) {
                    oldSelectable.SetSelectionVisualVisibility(false);
                }
            }
            
            // Show new tile selection visual
            GameObject newTileVisual = _tileVisuals[newTile.Position.x, newTile.Position.y];
            if (newTileVisual == null) { throw new Exception("New tile visual not found!"); }
            if (newTileVisual.TryGetComponent(out TileSelectable newSelectable)) {
                newSelectable.SetSelectionVisualVisibility(true);
            }

        }
        
        private void ClearActiveSelectionVisuals() {
            
        }
        

    }
}

