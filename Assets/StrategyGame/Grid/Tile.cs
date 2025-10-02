using UnityEngine;

namespace StrategyGame.Grid {
    public class Tile {
        public int Elevation { get; private set; }
        public Vector2Int Position { get; private set; }

        public Tile(int elevation = 0) {
            Elevation = elevation;
        }
        
    }

}
