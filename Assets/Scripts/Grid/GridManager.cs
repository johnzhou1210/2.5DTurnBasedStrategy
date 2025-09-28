using System;
using System.Collections.Generic;
using UnityEngine;

namespace Grid {
    public class GridManager : MonoBehaviour { 
        private Tile[,] _tiles;
        private Dictionary<int, GridEntity> _entities;
       [SerializeField] private Vector2Int size;
       [SerializeField] private GridEntityData soldierData, orcData;

       public Vector2Int GetSize() {
           return size;
       }
       
       public Tile GetTile(Vector2Int position) {
            return _tiles[position.x, position.y];
       }

       public void SpawnEntity(GridEntityData entityData, Vector2Int position) {
           GridDelegates.InvokeOnEntitySpawned(entityData, position);
       }
       
       private void Start() {
           _tiles = new Tile[size.x, size.y];
           for (int x = 0; x < size.x; x++) {
               for (int y = 0; y < size.y; y++) {
                   _tiles[x, y] = new Tile();
               }
           }
           
           SpawnEntity(soldierData, new(0,0));
           SpawnEntity(orcData, new(1,1));
       }
       
       
       
    }

}
