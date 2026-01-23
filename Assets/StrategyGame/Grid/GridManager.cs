using System;
using System.Collections;
using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid.GridData;
using StrategyGame.Grid.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StrategyGame.Grid {
    public class GridManager : MonoBehaviour {
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        public Tile[,] Tiles { get; private set; }
        [SerializeField] private Vector2Int size;
        
        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        private void OnEnable() {
            GridDelegates.OnSetTileTerrainType += SetTileTerrainType;
            GridDelegates.GetTileFromPosition = GetTileFromPosition;
            GridDelegates.AddEntityToGridFirstTime = AddEntityToGridFirstTime;
            GridDelegates.GetGridDimensions = GetSize;
            GridDelegates.GetTilesInRadius = GetTilesInRadius;
        }
        private void OnDisable() {
            GridDelegates.OnSetTileTerrainType -= SetTileTerrainType;
            GridDelegates.GetTileFromPosition = null;
            GridDelegates.AddEntityToGridFirstTime = null;
            GridDelegates.GetGridDimensions = null;
            GridDelegates.GetTilesInRadius = null;
        }
        private void Start() {
            TileData defaultTileData = Resources.Load<TileData>("ScriptableObjects/Tiles/Grasslands");
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
        
        
        
        // ==============================
        // CORE METHODS
        // ==============================
        public Vector2Int GetSize() {
            return size;
        }
        
        private void SetTileTerrainType(Vector2Int position, TileData tileData) {
            Tile tileToSetTerrain = GetTileFromPosition(position);
            if (tileToSetTerrain == null) throw new Exception("Tile to set terrain is null");
            tileToSetTerrain.SetInitData(tileData);
            GetComponent<GridRenderer>().OnTileRedraw(tileToSetTerrain);
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
        
       
        
        
        // ==============================
        // HELPERS
        // ==============================
        private Tile GetTile(Vector2Int position) {
            return Tiles[position.x, position.y];
        }
       
        private bool IsValidPosition(Vector2Int position) {
            return position.x >= 0 && position.x < size.x && position.y >= 0 && position.y < size.y;
        }
        
        private List<Tile> GetTilesInRadius(Vector2Int center, int radius) {
            List <Tile> result =  new List<Tile>();
            for (int dx = -radius; dx <= radius; dx++) {
                int remaining = radius - Mathf.Abs(dx);
                for (int dy = -remaining; dy <= remaining; dy++) {
                    Vector2Int pos = center + new Vector2Int(dx, dy);
                    if (!IsValidPosition(pos)) continue;
                    // if impassible terrain, exclude from results
                    if (GetTile(pos).MovementCost > 99) continue;
                    result.Add(Tiles[pos.x, pos.y]);
                }
            }
            return result;
        }

        

       
    }
}
