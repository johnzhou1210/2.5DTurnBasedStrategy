using System;
using Grid;
using UnityEngine;

public class GridDelegates
{
    #region Events

    public static event Action<GridEntityData, Vector2Int> OnEntitySpawned;

    public static void InvokeOnEntitySpawned(GridEntityData entityData, Vector2Int position) {
        OnEntitySpawned?.Invoke(entityData, position);
    }

    #endregion

    #region Funcs

    #endregion

}
