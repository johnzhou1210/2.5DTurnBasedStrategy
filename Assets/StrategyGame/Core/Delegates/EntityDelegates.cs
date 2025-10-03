using System;
using System.Collections.Generic;
using StrategyGame.Grid;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Core.Delegates {
    public static class EntityDelegates {
        #region Events
        
        #endregion
        
        #region Funcs

        public static Func<int, GridEntity> GetGridEntityByID;
        public static Func<List<UnitSpawnQuery>, List<GridUnit>> SpawnUnits;

        #endregion

    }
}
