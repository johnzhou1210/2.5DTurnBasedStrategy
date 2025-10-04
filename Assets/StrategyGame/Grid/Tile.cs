using System.Collections.Generic;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Grid {
    public class Tile {
        // Blueprint data
        public TileData InitData { get; private set; }
        
        // Core data
        public Vector2Int Position { get; private set; }
        public List<Tile> Neighbours { get; private set; }
        public int MovementCost { get; private set; }
        
        // Entity occupant data
        public GridEntity Occupant { get; private set; }
        public bool IsOccupied { get => Occupant != null; }
        
        // Visibility data
        public bool IsVisible { get; private set; }
        public bool IsHighlighted { get; private set; }
        
        // For pathfinding
        public int DistanceFromStart { get; private set; }
        public Tile Parent { get; private set; }

        public Tile(TileData tileData, Vector2Int position) {
            InitData = tileData;
            MovementCost = tileData.MovementCost;
            Position = position;
        }
        
    }

}
