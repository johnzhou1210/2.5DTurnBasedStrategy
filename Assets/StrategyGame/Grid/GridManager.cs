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
           GridDelegates.GetGridEntityById = GetEntityById;
       }

       private void OnDisable() {
           GridDelegates.GetGridEntityById = null;
       }

       public Vector2Int GetSize() {
           return size;
       }

       private GridEntity GetEntityById(int id) {
           return _entities[id];
       }
       
       public Tile GetTile(Vector2Int position) {
            return _tiles[position.x, position.y];
       }

       public GridEntity SpawnEntity(GridEntityData entityData, Vector2Int position) {
           GridEntity newEntity = new GridEntity(entityData);
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
           
           GridEntity soldierEntity = SpawnEntity(Resources.Load<GridEntityData>("ScriptableObjects/Entities/Soldier"), new(0,0));
           GridEntity orcEntity = SpawnEntity(Resources.Load<GridEntityData>("ScriptableObjects/Entities/Orc"), new(1,1));
           GridEntity archerEntity = SpawnEntity(Resources.Load<GridEntityData>("ScriptableObjects/Entities/Archer"), new(2,2));
           _entities[soldierEntity.Id] = soldierEntity;
           _entities[orcEntity.Id] = orcEntity;
           _entities[archerEntity.Id] = archerEntity;
       }
       
       
       
    }

}
