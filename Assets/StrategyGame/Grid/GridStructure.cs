using StrategyGame.Grid.GridData;

namespace StrategyGame.Grid {
    public class GridStructure : GridEntity
    {
        public GridStructure(GridEntityData gridEntityData, GridStructureData gridStructureData) : base(gridEntityData) {
            GridStructureData = gridStructureData;
        }

        public GridStructureData GridStructureData { get; set; }
        
       
        
        
        

      
    }
}
