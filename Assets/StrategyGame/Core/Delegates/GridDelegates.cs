using System;
using System.Collections.Generic;
using StrategyGame.Core.GameState;
using StrategyGame.Grid;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Core.Delegates {
    public static class GridDelegates
    {
        // ==============================
        // EVENTS
        // ==============================
        public static event Action<GridEntity, Vector2Int> OnEntitySpawned;
        public static event Action<Vector2Int, TileData> OnSetTileTerrainType;
        public static event Action<Vector2Int, Vector2Int> OnAStarPathPreview;
        public static event Action<ManualPath> OnManualPathPreview;
        public static event Action<Tile, Tile> OnInspectedTileChanged;
        public static event Action<Vector2Int, bool> OnSetTileVisualSelectionAnim;
        public static event Action OnClearPath;
        public static event Action OnGridRedraw;
        public static event Action<bool> OnSetDangerZoneVisibility;

        public static void InvokeOnEntitySpawned(GridEntity entity, Vector2Int position) {
            OnEntitySpawned?.Invoke(entity, position);
        }
        public static void InvokeOnSetTileTerrainType(Vector2Int coords, TileData tileData) {
            OnSetTileTerrainType?.Invoke(coords, tileData);
        }
        public static void InvokeOnAStarPathPreview(Vector2Int start, Vector2Int end) {
            OnAStarPathPreview?.Invoke(start, end);
        }
        public static void InvokeOnManualPathPreview(ManualPath path) {
            OnManualPathPreview?.Invoke(path);
        }
        
        public static void InvokeOnInspectedTileChanged(Tile oldTile, Tile newTile) {
            OnInspectedTileChanged?.Invoke(oldTile, newTile);
        }
        public static void InvokeOnSetTileVisualSelectionAnim(Vector2Int coords, bool val) {
            OnSetTileVisualSelectionAnim?.Invoke(coords, val);
        }

        public static void InvokeOnClearPath() {
            OnClearPath?.Invoke();
        }

        public static void InvokeOnGridRedraw() {
            OnGridRedraw?.Invoke();
        }
        public static void InvokeOnSetDangerZoneVisibility(bool val) {
            OnSetDangerZoneVisibility?.Invoke(val);
        }
        

        // ==============================
        // EVENTS
        // ==============================
        public static Func<Vector2Int, Tile> GetTileFromPosition;
        public static Func<GridEntity, Vector2Int, bool> AddEntityToGridFirstTime;
        public static Func<Vector2Int> GetGridDimensions;
        public static Func<Vector2Int, bool> SetInspectedTile;
        public static Func<Vector2Int, int, int, List<Tile>> GetTilesInRadius;

    }
}
