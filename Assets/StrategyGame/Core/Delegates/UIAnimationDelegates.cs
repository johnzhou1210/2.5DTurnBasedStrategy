using System;
using StrategyGame.UI;

namespace StrategyGame.Core.Delegates {
    public static class UIAnimationDelegates {
        // ==============================
        // EVENTS
        // ==============================
        public static event Action<AnimatorCategory, string> OnPlayAnimation;

        public static void InvokeOnPlayAnimation(AnimatorCategory category, string animationName) {
            OnPlayAnimation?.Invoke(category, animationName);
        }

        // ==============================
        // FUNCS
        // ==============================
    }
}
