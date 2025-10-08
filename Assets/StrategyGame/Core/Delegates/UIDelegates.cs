using System;
using StrategyGame.UI;

namespace StrategyGame.Core.Delegates {
    public class UIDelegates {
        // ==============================
        // EVENTS
        // ==============================
        public static event Action<UICategory, bool> OnSetUIActive;

        public static void InvokeOnSetUIActive(UICategory category, bool active) {
            OnSetUIActive?.Invoke(category, active);
        }

        

        // ==============================
        // FUNCS
        // ==============================

    }
}
