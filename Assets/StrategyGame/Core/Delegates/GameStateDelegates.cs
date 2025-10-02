using System;
using StrategyGame.Core.GameState;

namespace StrategyGame.Core.Delegates {
    public static class GameStateDelegates {

        #region Events

        public static event Action<GameStateManager.TurnPhase> OnPhaseChanged;

        public static void InvokeOnPhaseChanged(GameStateManager.TurnPhase phase) {
            OnPhaseChanged?.Invoke(phase);
        }

        #endregion

        #region Funcs

        #endregion

    }
}
