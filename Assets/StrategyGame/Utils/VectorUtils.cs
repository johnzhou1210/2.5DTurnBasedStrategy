using StrategyGame.Grid;
using UnityEngine;

namespace StrategyGame.Utils {
    public static class VectorUtils {
        public static Vector3 Vector2IntToVector3(Vector2Int vector2Int) {
            return new Vector3(vector2Int.x, 0, vector2Int.y);
        }
        
    }
}
