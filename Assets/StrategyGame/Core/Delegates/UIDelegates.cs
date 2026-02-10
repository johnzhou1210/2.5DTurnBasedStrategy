using System;
using StrategyGame.Combat;
using StrategyGame.Grid;
using StrategyGame.UI;

namespace StrategyGame.Core.Delegates {
    public static class UIDelegates {
        // ==============================
        // EVENTS
        // ==============================
        public static event Action<UICategory, bool> OnSetUIActive;
        public static event Action<GridEntity> OnEntityHUDUpdate;
        public static event Action<CombatPreview> OnBattleOutcomePreviewUpdate;
        public static event Action<Tile> OnTerrainUIUpdate;
        public static event Action<bool> OnSetCombatActionMenuVisibility;
        public static event Action OnUpdateTurnIndicatorRenderer;
        public static event Action OnPlayPhaseBannerAnimationSequence;
        public static event Action<bool, int, int, int, string> OnCombatCinematicHUDUpdate;

        public static void InvokeOnSetUIActive(UICategory category, bool active) {
            OnSetUIActive?.Invoke(category, active);
        }
        public static void InvokeOnEntityHUDUpdate(GridEntity entity) {
            OnEntityHUDUpdate?.Invoke(entity);
        }
        public static void InvokeOnTerrainUIUpdate(Tile tile) {
            OnTerrainUIUpdate?.Invoke(tile);
        }

        public static void InvokeOnSetCombatActionMenuVisibility(bool visible) {
            OnSetCombatActionMenuVisibility?.Invoke(visible);
        }

        public static void InvokeOnUpdateTurnIndicatorRenderer() {
            OnUpdateTurnIndicatorRenderer?.Invoke();
        }

        public static void InvokeOnPlayPhaseBannerAnimationSequence() {
            OnPlayPhaseBannerAnimationSequence?.Invoke();
        }

        public static void InvokeOnBattleOutcomePreviewUpdate(CombatPreview preview) {
            OnBattleOutcomePreviewUpdate?.Invoke(preview);
        }
        public static void InvokeOnCombatCinematicHUDUpdate(bool isAttacker, int health, int maxHealth, int oldHealth, string displayName) {
            OnCombatCinematicHUDUpdate?.Invoke(isAttacker, health, maxHealth, oldHealth, displayName);
        }

        

        // ==============================
        // FUNCS
        // ==============================

    }
}
