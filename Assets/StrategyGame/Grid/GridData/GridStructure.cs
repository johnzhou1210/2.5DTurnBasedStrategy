namespace StrategyGame.Grid.GridData {
    public class GridStructure : GridEntity
    {
        /* Inherited properties from parent:
         * VisualPrefab
         * Health and MaxHealth
         * FactionData
         * MovementRange and VisionRange
         */
        public GridUnitData GridStructureInitData { get; private set; }
        
        public GridStructure(GridEntityData gridEntityData, GridUnitData gridStructureData) : base(gridEntityData) {
            GridStructureInitData = gridStructureData;
        }
    }
}
