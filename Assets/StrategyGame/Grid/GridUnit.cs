using System.Collections.Generic;
using System.Linq;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Grid {
    public class GridUnit : GridEntity
    {
        /* Inherited properties from parent:
         * VisualPrefab
         * Health and MaxHealth
         * FactionData
         * MovementRange and VisionRange
         */
        public GridUnitData GridUnitData { get; private set; }
        
        public GridUnit(GridEntityData gridEntityData, GridUnitData gridUnitData) : base(gridEntityData) {
            GridUnitData = gridUnitData;
        }


        public HashSet<Tile> GetValidTileDestinations() {
            HashSet<Tile> validTiles = new HashSet<Tile>();
            return validTiles;
        }
       
        public override HashSet<Tile> GetTilesWithinAttackRangeAtPosition(Vector2Int position) {
            HashSet<Tile> tilesWithinRange = GridDelegates.GetTilesInRadius(position, GridUnitData.Weapon.AttackRange).ToHashSet();
            return tilesWithinRange;
        }
        
        

      
    }
}
