using System;
using StrategyGame.Grid;
using UnityEngine;

namespace StrategyGame.Core.Delegates {
    public static class GridDelegates
    {
        // ==============================
        // EVENTS
        // ==============================
        public static event Action<GridEntity, Vector2Int> OnEntitySpawned;
        public static event Action<Vector2Int> OnSelectTile;
        public static event Action<Tile, Tile> OnSetSelectedTile;
        public static event Action<Vector2Int> OnMountainifyTile;
        public static event Action<Vector2Int, Vector2Int> OnUpdatePathPreview;
    

        public static void InvokeOnEntitySpawned(GridEntity entity, Vector2Int position) {
            OnEntitySpawned?.Invoke(entity, position);
        }
        public static void InvokeOnSelectTile(Vector2Int coords) {
            OnSelectTile?.Invoke(coords);
        }
        public static void InvokeOnSetSelectedTile(Tile oldTile, Tile newTile) {
            OnSetSelectedTile?.Invoke(oldTile, newTile);
        }
        public static void InvokeOnMountainifyTile(Vector2Int coords) {
            OnMountainifyTile?.Invoke(coords);
        }
        public static void InvokeOnUpdatePathPreview(Vector2Int start, Vector2Int end) {
            OnUpdatePathPreview?.Invoke(start, end);
        }

        

        

        // ==============================
        // EVENTS
        // ==============================
        public static Func<Vector2Int, Tile> GetTileFromPosition;
        public static Func<Tile> GetSelectedTile;
        public static Func<GridEntity, Vector2Int, bool> AddEntityToGridFirstTime;
        public static Func<Vector2Int> GetGridDimensions;

    }
}
