using System.Collections.Generic;
using StrategyGame.Grid;
using UnityEngine;

namespace StrategyGame.Combat.Targeting {
    public abstract class AttackRange : ScriptableObject {
        public abstract HashSet<Tile> GetTiles(Vector2Int origin);
    }
}
