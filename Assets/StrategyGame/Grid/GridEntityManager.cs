using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Grid {

    public struct UnitSpawnQuery {
        public GridUnitData UnitData;
        public Vector2Int SpawnPosition;
    }
    
     public struct StructureSpawnQuery {
            public GridUnitData UnitData;
            public Vector2Int SpawnPosition;
        }
    
    public class GridEntityManager : MonoBehaviour {
        public Dictionary<int, GridEntity> Entities { get; private set; }
        
        private void Awake() {
            Entities = new Dictionary<int, GridEntity>();
        }
        
        private void OnEnable() {
            EntityDelegates.GetGridEntityByID = GetGridEntityById;
            EntityDelegates.SpawnUnits = SpawnUnits;
        }

        private void OnDisable() {
            EntityDelegates.GetGridEntityByID = null;
            EntityDelegates.SpawnUnits = null;
        }
        
        private GridEntity GetGridEntityById(int id) {
            return Entities[id];
        }

        private List<GridUnit> SpawnUnits(List<UnitSpawnQuery> query) {
            List<GridUnit> spawnedUnits = new List<GridUnit>();
            foreach (UnitSpawnQuery q in query) {
                spawnedUnits.Add(SpawnUnit(q.UnitData, q.SpawnPosition));
            }
            return spawnedUnits;
        }
        
        private GridUnit SpawnUnit(GridUnitData unitData, Vector2Int position) {
            GridUnit newUnit = new GridUnit(unitData, unitData);
            Entities[newUnit.ID] = newUnit;
            GridDelegates.InvokeOnEntitySpawned(newUnit, position);
            return newUnit;
        }
    }
}
