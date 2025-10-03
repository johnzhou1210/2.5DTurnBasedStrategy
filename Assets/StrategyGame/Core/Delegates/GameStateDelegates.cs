using System;
using StrategyGame.Core.GameState;

namespace StrategyGame.Core.Delegates {
    public static class GameStateDelegates {

        #region Events

        public static event Action<GameStateManager.TurnPhase> OnPhaseChanged;
        public static event Action OnGameStarted;

        public static void InvokeOnPhaseChanged(GameStateManager.TurnPhase phase) {
            OnPhaseChanged?.Invoke(phase);
        }

        public static void InvokeOnGameStarted() {
            OnGameStarted?.Invoke();
        }

        #endregion

        #region Funcs

        #endregion

    }
}
