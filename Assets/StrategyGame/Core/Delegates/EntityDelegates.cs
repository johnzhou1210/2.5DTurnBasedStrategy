using System;
using System.Collections.Generic;
using StrategyGame.Factions;
using StrategyGame.Grid;
using UnityEngine;

namespace StrategyGame.Core.Delegates {
    public static class EntityDelegates {
        // ==============================
        // EVENTS
        // ==============================
        public static event Action<GridEntity, List<Tile>> OnEntityMoveAlongPath;
        
        public static void InvokeOnEntityMoveAlongPath(GridEntity entity, List<Tile> path) {
            OnEntityMoveAlongPath?.Invoke(entity, path);
        }
        
       
        
        // ==============================
        // FUNCS
        // ==============================
        public static Func<int, GridEntity> GetGridEntityByID;
        public static Func<List<UnitSpawnQuery>, List<GridUnit>> SpawnUnits;
        public static Func<List<StructureSpawnQuery>, List<GridStructure>> SpawnStructures;
        
        public static Func<Faction, bool, List<int>> GetAllGridEntityIDsByFaction;

    }
}
