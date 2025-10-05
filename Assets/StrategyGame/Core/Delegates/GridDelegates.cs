using System;
using StrategyGame.Grid;
using UnityEngine;

namespace StrategyGame.Core.Delegates {
    public static class GridDelegates
    {
        #region Events

        public static event Action<GridEntity, Vector2Int> OnEntitySpawned;
        public static event Action<Vector2Int> OnSelectTile;
        public static event Action<Tile, Tile> OnSetSelectedTile;

        public static void InvokeOnEntitySpawned(GridEntity entity, Vector2Int position) {
            OnEntitySpawned?.Invoke(entity, position);
        }
        
        public static void InvokeOnSelectTile(Vector2Int coords) {
            OnSelectTile?.Invoke(coords);
        }

        public static void InvokeOnSetSelectedTile(Tile oldTile, Tile newTile) {
            OnSetSelectedTile?.Invoke(oldTile, newTile);
        }

       
        

        #endregion

        #region Funcs

        public static Func<int, Transform> GetEntityVisualTransformByID;
        public static Func<int, GridEntity> GetGridEntityFromID;
        public static Func<Vector2Int, Tile> GetTileFromPosition;
        public static Func<Tile> GetSelectedTile;
        public static Func<GridEntity, Vector2Int, bool> AddEntityToGridFirstTime;

        #endregion

    }
}
