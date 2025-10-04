using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Grid {
    public class GridManager : MonoBehaviour {
        public Tile[,] Tiles { get; private set; }

        [SerializeField] private Vector2Int size;

       private void OnDisable() {
           GridDelegates.GetGridEntityByID = null;
       }

       public Vector2Int GetSize() {
           return size;
       }
       
       
       public Tile GetTile(Vector2Int position) {
            return Tiles[position.x, position.y];
       }

       
       
       private void Start() {
           TileData defaultTileData = Resources.Load<TileData>("ScriptableObjects/Tiles/DefaultTile");
         
           Tiles = new Tile[size.x, size.y];
           for (int x = 0; x < size.x; x++) {
               for (int y = 0; y < size.y; y++) {
                   Tiles[x, y] = new Tile(defaultTileData, new Vector2Int(x, y));
               }
           }

           GetComponent<GridRenderer>().OnGridRedraw();
           GameStateDelegates.InvokeOnGameStarted();
       }
       
       
       
    }

}
