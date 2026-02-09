using System;
using StrategyGame.Combat.Cinematics;

namespace StrategyGame.Core.Delegates {
    public static class CombatCinematicsDelegates {
        public static Func<CombatDirector> GetDirector;
    }
}
