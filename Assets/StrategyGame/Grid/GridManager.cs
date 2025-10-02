using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Grid {
    public class GridManager : MonoBehaviour { 
        private Tile[,] _tiles;
        private Dictionary<int, GridEntity> _entities;
       [SerializeField] private Vector2Int size;

       private void OnEnable() {
           GridDelegates.GetGridEntityByID = GetEntityByID;
       }

       private void OnDisable() {
           GridDelegates.GetGridEntityByID = null;
       }

       public Vector2Int GetSize() {
           return size;
       }

       private GridEntity GetEntityByID(int id) {
           return _entities[id];
       }
       
       public Tile GetTile(Vector2Int position) {
            return _tiles[position.x, position.y];
       }

       public GridEntity SpawnUnit(GridUnitData unitData, Vector2Int position) {
           GridEntity newEntity = new GridUnit(unitData, unitData);
           GridDelegates.InvokeOnEntitySpawned(newEntity, position);
           return newEntity;
       }
       
       private void Start() {
           _entities = new Dictionary<int, GridEntity>();
           _tiles = new Tile[size.x, size.y];
           for (int x = 0; x < size.x; x++) {
               for (int y = 0; y < size.y; y++) {
                   _tiles[x, y] = new Tile();
               }
           }
           
           GridEntity soldierEntity = SpawnUnit(Resources.Load<GridUnitData>("ScriptableObjects/Units/Soldier"), new(0,0));
           GridEntity orcEntity = SpawnUnit(Resources.Load<GridUnitData>("ScriptableObjects/Units/Orc"), new(1,1));
           GridEntity archerEntity = SpawnUnit(Resources.Load<GridUnitData>("ScriptableObjects/Units/Archer"), new(2,2));
           
           _entities[soldierEntity.ID] = soldierEntity;
           _entities[orcEntity.ID] = orcEntity;
           _entities[archerEntity.ID] = archerEntity;
       }
       
       
       
    }

}
