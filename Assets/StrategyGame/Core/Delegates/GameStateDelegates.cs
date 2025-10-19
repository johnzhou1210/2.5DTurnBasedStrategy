using System;
using StrategyGame.Core.Enums;
using StrategyGame.Core.GameState;
using StrategyGame.Grid;

namespace StrategyGame.Core.Delegates {
    public static class GameStateDelegates {

        // ==============================
        // EVENTS
        // ==============================
        public static event Action<GameStateEnums.TurnPhase> OnPhaseChanged;
        public static event Action<GameStateEnums.UnitMoveSelectionMode> OnUnitMoveSelectionChanged;
        public static event Action OnGameStarted;
        public static event Action<GameStateEnums.ManualMoveSelectionState> OnManualMoveSelectionChanged;

        public static void InvokeOnPhaseChanged(GameStateEnums.TurnPhase phase) {
            OnPhaseChanged?.Invoke(phase);
        }
        public static void InvokeOnGameStarted() {
            OnGameStarted?.Invoke();
        }
        public static void InvokeOnUnitMoveSelectionChanged(GameStateEnums.UnitMoveSelectionMode mode) {
            OnUnitMoveSelectionChanged?.Invoke(mode);
        }
        public static void InvokeOnManualMoveSelectionChanged(GameStateEnums.ManualMoveSelectionState state) {
            OnManualMoveSelectionChanged?.Invoke(state);
        }

        

        // ==============================
        // FUNCS
        // ==============================
        public static Func<GridEntity> GetCurrentInspectedEntity;
        public static Func<GridEntity> GetCurrentSelectedEntity;
        public static Func<GameStateManager.GameStateSnapshot> GetCurrentGameStateSnapshot;

    }
}
