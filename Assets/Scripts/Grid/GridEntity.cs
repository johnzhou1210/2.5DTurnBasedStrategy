using UnityEngine;

namespace Grid {
    public class GridEntity {
        private static int _nextId = 0;
        public int Id { get; private set; }
        
        private Tile _tile;
        private GridEntityData _gridEntityData;


        public GridEntity(GridEntityData gridEntityData) {
            Id = _nextId++;
            _gridEntityData = gridEntityData;
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

