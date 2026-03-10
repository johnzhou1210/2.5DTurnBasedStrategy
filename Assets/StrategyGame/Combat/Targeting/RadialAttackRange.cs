using System.Collections.Generic;
using System.Linq;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid;
using UnityEngine;

namespace StrategyGame.Combat.Targeting {
    [CreateAssetMenu(menuName = "Strategy Game/Attack Ranges/Radial")]
    public class RadialAttackRange : Targeting.AttackRange {
        public int MinAttackRange;
        public int MaxAttackRange;
        public override HashSet<Tile> GetTiles(Vector2Int origin) {
            return GridDelegates.GetTilesInRadius(origin, MinAttackRange, MaxAttackRange).ToHashSet();
        }
    }
}
