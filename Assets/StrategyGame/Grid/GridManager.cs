using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Grid {
    public class GridManager : MonoBehaviour { 
        private Tile[,] _tiles;
        
       [SerializeField] private Vector2Int size;

       private void OnDisable() {
           GridDelegates.GetGridEntityByID = null;
       }

       public Vector2Int GetSize() {
           return size;
       }
       
       
       public Tile GetTile(Vector2Int position) {
            return _tiles[position.x, position.y];
       }

       
       
       private void Start() {
         
           _tiles = new Tile[size.x, size.y];
           for (int x = 0; x < size.x; x++) {
               for (int y = 0; y < size.y; y++) {
                   _tiles[x, y] = new Tile();
               }
           }
           
           GameStateDelegates.InvokeOnGameStarted();
       }
       
       
       
    }

}
