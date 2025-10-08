using System;
using System.Collections.Generic;
using StrategyGame.Grid;
using UnityEngine;

namespace StrategyGame.Core.Delegates {
    public static class EntityDelegates {
        #region Events
        public static event Action<GridEntity, List<Tile>> OnEntityMoveAlongPath;

        public static void InvokeOnEntityMoveAlongPath(GridEntity entity, List<Tile> path) {
            OnEntityMoveAlongPath?.Invoke(entity, path);
        }
        
        #endregion
        
        #region Funcs

        public static Func<int, GridEntity> GetGridEntityByID;
        public static Func<List<UnitSpawnQuery>, List<GridUnit>> SpawnUnits;
        public static Func<List<StructureSpawnQuery>, List<GridStructure>> SpawnStructures;
        public static Func<int, Transform> GetEntityVisualTransformByID;
        public static Func<int, GridEntity> GetGridEntityFromID;

        #endregion

    }
}
