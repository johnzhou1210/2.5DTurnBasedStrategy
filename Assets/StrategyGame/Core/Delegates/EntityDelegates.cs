using System;
using StrategyGame.Grid;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Core.Delegates {
    public static class EntityDelegates {
        #region Events
        
        #endregion
        
        #region Funcs

        public static Func<int, GridEntity> GetGridEntityByID;
        public static Func<GridUnitData, Vector2Int, GridEntity> SpawnUnit;

        #endregion

    }
}
