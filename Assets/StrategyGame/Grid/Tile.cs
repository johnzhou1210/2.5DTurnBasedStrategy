using System.Collections.Generic;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Grid {

    public enum Direction {
        North, East, South, West
    }
    
    public class Tile {
        // Blueprint data
        public TileData InitData { get; private set; }
        
        // Core data
        public Vector2Int Position { get; }
        public Dictionary<Direction, Tile> Neighbors { get; private set; }
        public int MovementCost { get; private set; }
        
        // Entity occupant data
        public GridEntity Occupant { get; private set; }
        public bool IsOccupied { get => Occupant != null; }
        
        // Visibility data
        public bool IsVisible { get; private set; }
        public bool IsSelected { get; private set; }
        
        // For pathfinding
        public int DistanceFromStart { get; private set; }
        public Tile Parent { get; private set; }

        public Tile(TileData tileData, Vector2Int position) {
            InitData = tileData;
            MovementCost = tileData.MovementCost;
            Position = position;
        }
        
        public override bool Equals(object obj) => obj is Tile t && t.Position == Position;
            public override int GetHashCode() => Position.GetHashCode();

        public bool AddOccupant(GridEntity entity) {
            if (Occupant != null) {
                Debug.LogWarning($"Failed to set occupant {entity.GridEntityData.name} to tile {Position} because it is already occupied.");
                return false;
            }
            Occupant = entity;
            entity.SetGridPosition(Position);
            Debug.Log($"Added occupant to tile {Position}.");
            return true;
        }

        public void SetNeighbors(Dictionary<Direction, Tile> dict) {
            Neighbors = dict;
        }

        public void SetInitData(TileData newInitData) {
            InitData = newInitData;
            MovementCost = newInitData.MovementCost;
        }

    }

}
