using System;
using StrategyGame.Grid;
using StrategyGame.UI;

namespace StrategyGame.Core.Delegates {
    public class UIDelegates {
        // ==============================
        // EVENTS
        // ==============================
        public static event Action<UICategory, bool> OnSetUIActive;
        public static event Action<GridEntity> OnEntityHUDUpdate;

        public static void InvokeOnSetUIActive(UICategory category, bool active) {
            OnSetUIActive?.Invoke(category, active);
        }
        public static void InvokeOnEntityHUDUpdate(GridEntity entity) {
            OnEntityHUDUpdate?.Invoke(entity);
        }

        

        // ==============================
        // FUNCS
        // ==============================

    }
}
