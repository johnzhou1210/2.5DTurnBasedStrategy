using System;
using StrategyGame.Combat;
using StrategyGame.Combat.Cinematics;

namespace StrategyGame.Core.Delegates {
    public static class DataDelegates {
        public static Func<int, AbilityData> GetAbilityDataByID;
        public static Func<int, ProjectileVisualData> GetProjectileVisualDataByID;
    }
}
