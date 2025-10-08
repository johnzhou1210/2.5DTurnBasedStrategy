using System;
using StrategyGame.Core.GameState;
using StrategyGame.Grid;

namespace StrategyGame.Core.Delegates {
    public static class GameStateDelegates {

        // ==============================
        // EVENTS
        // ==============================
        public static event Action<GameStateManager.TurnPhase> OnPhaseChanged;
        public static event Action OnGameStarted;

        public static void InvokeOnPhaseChanged(GameStateManager.TurnPhase phase) {
            OnPhaseChanged?.Invoke(phase);
        }
        public static void InvokeOnGameStarted() {
            OnGameStarted?.Invoke();
        }

        

        // ==============================
        // FUNCS
        // ==============================

        public static Func<GridEntity> GetCurrentSelectedEntity;

    }
}
