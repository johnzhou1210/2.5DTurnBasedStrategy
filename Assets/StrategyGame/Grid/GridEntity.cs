using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Grid {
    public class GridEntity {
        private static int _nextId = 0;
        public int Id { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        
        private Tile _tile;
        public GridEntityData GridEntityData { get; private set; }


        public GridEntity(GridEntityData gridEntityData) {
            Id = _nextId++;
            GridEntityData = gridEntityData;
            Health = GridEntityData.Health;
            MaxHealth = GridEntityData.MaxHealth;
        }

        public GameObject GetSpritePrefab() {
            return GridEntityData.SpritePrefab;
        }
        
        
        public void Move(Vector2Int newPosition) {
            Vector2Int startPosition = GetPosition();
        }

        public void Move(Tile targetTile) {
            
        }

        private bool IsValidPosition(Vector2Int position) {
            
            return true;
        }

        public Vector2Int GetPosition() {
            return _tile.Position;
        }
    
    }
}

