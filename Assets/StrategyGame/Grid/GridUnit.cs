using System.Collections.Generic;
using StrategyGame.Grid.GridData;

namespace StrategyGame.Grid {
    public class GridUnit : GridEntity
    {
        /* Inherited properties from parent:
         * VisualPrefab
         * Health and MaxHealth
         * FactionData
         * MovementRange and VisionRange
         */
        public GridUnitData GridUnitInitData { get; private set; }
        
        public GridUnit(GridEntityData gridEntityData, GridUnitData gridUnitData) : base(gridEntityData) {
            GridUnitInitData = gridUnitData;
        }


        public HashSet<Tile> GetValidTileDestinations() {
            HashSet<Tile> validTiles = new HashSet<Tile>();
            return validTiles;
        }
       
        
        
        

      
    }
}
