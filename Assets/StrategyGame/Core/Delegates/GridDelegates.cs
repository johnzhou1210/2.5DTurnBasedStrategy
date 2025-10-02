using System;
using StrategyGame.Grid;
using UnityEngine;

namespace StrategyGame.Core.Delegates {
    public static class GridDelegates
    {
        #region Events

        public static event Action<GridEntity, Vector2Int> OnEntitySpawned;

        public static void InvokeOnEntitySpawned(GridEntity entity, Vector2Int position) {
            OnEntitySpawned?.Invoke(entity, position);
        }

        #endregion

        #region Funcs

        public static Func<int, Transform> GetEntityVisualTransformByID;
        public static Func<int, GridEntity> GetGridEntityByID;

        #endregion

    }
}
