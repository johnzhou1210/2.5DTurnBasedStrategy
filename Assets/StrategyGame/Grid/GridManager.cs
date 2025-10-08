using System;
using System.Collections;
using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid.GridData;
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
            GridDelegates.GetTileFromPosition = GetTileFromPosition;
            GridDelegates.AddEntityToGridFirstTime = AddEntityToGridFirstTime;
            GridDelegates.GetGridDimensions = GetSize;
            GridDelegates.OnMountainifyTile += MountainifyTile;
        }
        private void OnDisable() {
            GridDelegates.GetTileFromPosition = null;
            GridDelegates.AddEntityToGridFirstTime = null;
            GridDelegates.GetGridDimensions = null;
            GridDelegates.OnMountainifyTile -= MountainifyTile;
            
        }
        private void Start() {
            TileData defaultTileData = Resources.Load<TileData>("ScriptableObjects/Tiles/DefaultTile");
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
        
        private void MountainifyTile(Vector2Int position) {
            TileData mountainTileData = Resources.Load<TileData>("ScriptableObjects/Tiles/MountainTile");
            Tile tileToMountainify = GetTileFromPosition(position);
            if (tileToMountainify == null) throw new Exception("Tile to mountainify is null");
            tileToMountainify.SetInitData(mountainTileData);
            GetComponent<GridRenderer>().OnTileRedraw(tileToMountainify);
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
        

        

        

       
    }
}
