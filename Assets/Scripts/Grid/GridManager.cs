using System;
using System.Collections.Generic;
using UnityEngine;

namespace Grid {
    public class GridManager : MonoBehaviour { 
        private Tile[,] _tiles;
        private Dictionary<int, GridEntity> _entities;
       [SerializeField] private Vector2Int size;
       [SerializeField] private GridEntityData soldierData, orcData;

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
           
           GridEntity soldierEntity = SpawnEntity(soldierData, new(0,0));
           GridEntity orcEntity = SpawnEntity(orcData, new(1,1));
           _entities[soldierEntity.Id] = soldierEntity;
           _entities[orcEntity.Id] = orcEntity;
       }
       
       
       
    }

}
