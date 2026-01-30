using System;
using StrategyGame.UI;

namespace StrategyGame.Core.Delegates {
    public static class UIAnimationDelegates {
        // ==============================
        // EVENTS
        // ==============================
        public static event Action<AnimatorCategory, string> OnPlayAnimation;
        public static event Action<AnimatorCategory, bool> OnShow;
        public static event Action<AnimatorCategory, bool> OnHide;
        public static event Action<AnimatorCategory, bool> OnShowIfHidden;
        public static event Action<AnimatorCategory, bool> OnHideIfVisible;
        

        public static void InvokeOnPlayAnimation(AnimatorCategory category, string animationName) {
            OnPlayAnimation?.Invoke(category, animationName);
        }
        public static void InvokeOnShow(AnimatorCategory category, bool instant = false) {
            OnShow?.Invoke(category, instant);
        }
        public static void InvokeOnHide(AnimatorCategory category, bool instant = false) {
            OnHide?.Invoke(category, instant);
        }
        public static void InvokeOnShowIfHidden(AnimatorCategory category, bool instant = false) {
            OnShowIfHidden?.Invoke(category, instant);
        }
        public static void InvokeOnHideIfVisible(AnimatorCategory category, bool instant = false) {
            OnHideIfVisible?.Invoke(category, instant);
        }
        
        
        // ==============================
        // FUNCS
        // ==============================
        public static Func<AnimatorCategory, string, bool> GetIsPlaying;
    }
}
