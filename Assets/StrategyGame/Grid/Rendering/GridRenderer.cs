using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid.GridData;
using StrategyGame.UI.World;
using UnityEngine;

namespace StrategyGame.Grid {
    public class GridRenderer : MonoBehaviour {
        [SerializeField] private GameObject[] tilePrefabs;
        
        private GameObject[,] _tileVisuals;
        
        
        
        [SerializeField] private GridManager grid;
        public GridManager Grid {
            get {
                return grid;
            }
            set {
                OnGridRedraw();
                grid = value;
            }
        }
        
        
        private void OnEnable() {
            _tileVisuals = new GameObject[grid.GetSize().x, grid.GetSize().y];
            OnGridRedraw();
        }

      

        private void OnGridRedraw() {
            Vector2Int dimensions = grid.GetSize();
            for (int y = 0; y < dimensions.y; y++) {
                for (int x = 0; x < dimensions.x; x++) {
                    Vector3 position = new Vector3(x, 0f, y);
                    GameObject newTile = Instantiate((x + y) % 2 == 0 ? tilePrefabs[0] : tilePrefabs[1], transform);
                    newTile.transform.position = position;
                    _tileVisuals[x, y] = newTile;
                }
            }
        }
    
    }
}

