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
            public GridStructureData StructureData;
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
            EntityDelegates.SpawnStructures = SpawnStructures;
        }

        private void OnDisable() {
            EntityDelegates.GetGridEntityByID = null;
            EntityDelegates.SpawnUnits = null;
            EntityDelegates.SpawnStructures = null;
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
        
        private List<GridStructure> SpawnStructures(List<StructureSpawnQuery> query) {
            List<GridStructure> spawnedStructures = new List<GridStructure>();
            foreach (StructureSpawnQuery q in query) {
                spawnedStructures.Add(SpawnStructure(q.StructureData, q.SpawnPosition));
            }
            return spawnedStructures;
        }
        
        private GridUnit SpawnUnit(GridUnitData unitData, Vector2Int position) {
            GridUnit newUnit = new GridUnit(unitData, unitData);
            Entities[newUnit.ID] = newUnit;
            bool success = GridDelegates.AddEntityToGridFirstTime(newUnit, position);
            if (!success) {
                Debug.LogWarning($"Failed to spawn {unitData.name} to {position}!");
                return null;
            }
            GridDelegates.InvokeOnEntitySpawned(newUnit, position);
            return newUnit;
        }
        
        private GridStructure SpawnStructure(GridStructureData structureData, Vector2Int position) {
            GridStructure newStructure = new GridStructure(structureData, structureData);
            Entities[newStructure.ID] = newStructure;
            bool success = GridDelegates.AddEntityToGridFirstTime(newStructure, position);
            if (!success) {
                Debug.LogWarning($"Failed to spawn {structureData.name} to {position}!");
                return null;
            }
            GridDelegates.InvokeOnEntitySpawned(newStructure, position);
            return newStructure;
        }
        


    }
}
