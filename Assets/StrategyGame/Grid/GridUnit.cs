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
       
        public override HashSet<Tile> GetAttackableTilesAtPosition(Vector2Int position) {
            HashSet<Tile> tilesWithinRange = GridDelegates.GetTilesInRadius(position, GridUnitData.Weapon.MinAttackRange, GridUnitData.Weapon.MaxAttackRange).ToHashSet();
            return tilesWithinRange;
        }

        public override HashSet<Tile> GetTilesWithinAttackRange() {
            Debug.Log("GridUnit.GetTilesWithinAttackRange: Calling override version");
            HashSet<Tile> reachableTiles = GetWalkableTiles(true);
            HashSet<Tile> dangerTiles = new HashSet<Tile>();
            foreach (Tile tile in reachableTiles) {
                dangerTiles.UnionWith(GetAttackableTilesAtPosition(tile.Position));
            }
            return dangerTiles;
        }
        
        public override HashSet<GridEntity> GetAttackableEntitiesAtPosition(Vector2Int position) {
            HashSet<Tile> attackableTiles = GetAttackableTilesAtPosition(position);
            HashSet<GridEntity> attackableEntities =  new HashSet<GridEntity>();
            foreach (Tile tile in attackableTiles) {
                if (tile.IsOccupied && !IsFriendlyWith(tile.Occupant)) {
                    attackableEntities.Add(tile.Occupant);
                }
            }
            return attackableEntities;
        }




    }
}
