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
        public static event Action<Tile> OnTerrainUIUpdate;

        public static void InvokeOnSetUIActive(UICategory category, bool active) {
            OnSetUIActive?.Invoke(category, active);
        }
        public static void InvokeOnEntityHUDUpdate(GridEntity entity) {
            OnEntityHUDUpdate?.Invoke(entity);
        }
        public static void InvokeOnTerrainUIUpdate(Tile tile) {
            OnTerrainUIUpdate?.Invoke(tile);
        }
        

        

        // ==============================
        // FUNCS
        // ==============================

    }
}
