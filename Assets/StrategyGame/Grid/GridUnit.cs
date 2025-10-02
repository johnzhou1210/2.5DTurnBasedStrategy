namespace StrategyGame.Grid.GridData {
    public class GridUnit : GridEntity
    {
        public GridUnit(GridEntityData gridEntityData, GridUnitData gridUnitData) : base(gridEntityData) {
            GridUnitData = gridUnitData;
        }

        public GridUnitData GridUnitData { get; set; }
        
       
        
        
        

      
    }
}
