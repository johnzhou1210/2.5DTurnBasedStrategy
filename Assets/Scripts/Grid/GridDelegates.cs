using System;
using Grid;
using UnityEngine;

public class GridDelegates
{
    #region Events

    public static event Action<GridEntity, Vector2Int> OnEntitySpawned;

    public static void InvokeOnEntitySpawned(GridEntity entity, Vector2Int position) {
        OnEntitySpawned?.Invoke(entity, position);
    }

    #endregion

    #region Funcs

    public static Func<int, Transform> GetEntityVisualTransformById;
    public static Func<int, GridEntity> GetGridEntityById;

    #endregion

}
