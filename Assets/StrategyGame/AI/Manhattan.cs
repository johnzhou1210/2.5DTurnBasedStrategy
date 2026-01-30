using System;
using System.Collections.Generic;
using StrategyGame.Grid;
using UnityEngine;

namespace StrategyGame.AI {
    public static class Manhattan {
        public static int Distance(Vector2Int coord1, Vector2Int coord2) {
            return Math.Abs(coord1.x - coord2.x) + Math.Abs(coord1.y - coord2.y);
        }
        
       
    }
}
