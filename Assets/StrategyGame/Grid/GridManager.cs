using System;
using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid.GridData;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StrategyGame.Grid {
    public class GridManager : MonoBehaviour {
        public Tile[,] Tiles { get; private set; }
        [SerializeField] private Vector2Int size;
        private void OnEnable() {
            GridDelegates.GetTileFromPosition = GetTileFromPosition;
            GridDelegates.AddEntityToGridFirstTime = AddEntityToGridFirstTime;
        }
        private void OnDisable() {
            GridDelegates.GetTileFromPosition = null;
            GridDelegates.AddEntityToGridFirstTime = null;
        }
        public Vector2Int GetSize() {
            return size;
        }
        public Tile GetTile(Vector2Int position) {
            return Tiles[position.x, position.y];
        }
        private void Start() {
            TileData defaultTileData = Resources.Load<TileData>("ScriptableObjects/Tiles/DefaultTile");
            TileData mountainTileData = Resources.Load<TileData>("ScriptableObjects/Tiles/MountainTile");
            Tiles = new Tile[size.x, size.y];
            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    Tiles[x, y] = new Tile(defaultTileData, new Vector2Int(x, y));
                }
            }

            // Go through grid again to initialize neighbors
            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    Dictionary<Direction, Tile> neighbors = new Dictionary<Direction, Tile>();
                    Vector2Int currentPosition = new Vector2Int(x, y);
                    neighbors[Direction.North] = IsValidPosition(currentPosition + Vector2Int.up) ? Tiles[x, y + 1] : null;
                    neighbors[Direction.East] = IsValidPosition(currentPosition + Vector2Int.right) ? Tiles[x + 1, y] : null;
                    neighbors[Direction.South] = IsValidPosition(currentPosition + Vector2Int.down) ? Tiles[x, y - 1] : null;
                    neighbors[Direction.West] = IsValidPosition(currentPosition + Vector2Int.left) ? Tiles[x - 1, y] : null;
                    Tiles[x, y].SetNeighbors(neighbors);
                }
            }
            GetComponent<GridRenderer>().OnGridRedraw();
            GameStateDelegates.InvokeOnGameStarted();
        }
        private bool IsValidPosition(Vector2Int position) {
            return position.x >= 0 && position.x < size.x && position.y >= 0 && position.y < size.y;
        }
        private Tile GetTileFromPosition(Vector2Int position) {
            return Tiles[position.x, position.y];
        }

        // This function should only be called when adding an entity to the grid for the first time.
        private bool AddEntityToGridFirstTime(GridEntity entity, Vector2Int position) {
            Tile tileToAddTo = GetTile(position);
            Debug.Log($"{position} | {tileToAddTo}");
            if (tileToAddTo == null) {
                return false;
            }
            return tileToAddTo.AddOccupant(entity);
        }
    }
}
