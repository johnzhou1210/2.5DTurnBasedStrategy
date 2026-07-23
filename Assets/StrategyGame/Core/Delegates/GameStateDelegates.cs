using System;
using StrategyGame.Combat;
using StrategyGame.Core.GameState;
using StrategyGame.Grid;

namespace StrategyGame.Core.Delegates {
    public static class GameStateDelegates {

        // ==============================
        // EVENTS
        // ==============================
        public static event Action<CombatStateEnums.UnitMoveSelectionMode> OnUnitMoveSelectionChanged;
        public static event Action OnGameStarted;
        public static event Action<CombatStateEnums.PlayerPhaseState> OnPlayerPhaseStateChanged;
        public static event Action OnAdvanceTurnPhase;
        public static event Action<CombatStateEnums.TurnPhase> OnTurnPhaseChanged;
        public static event Action<CombatOutcome> OnApplyAttackOutcome;
        public static event Action OnFinalizePlayerAction;
        
        public static void InvokeOnGameStarted() {
            OnGameStarted?.Invoke();
        }
        public static void InvokeOnUnitMoveSelectionChanged(CombatStateEnums.UnitMoveSelectionMode mode) {
            OnUnitMoveSelectionChanged?.Invoke(mode);
        }
        public static void InvokeOnPlayerPhaseStateChanged(CombatStateEnums.PlayerPhaseState state) {
            OnPlayerPhaseStateChanged?.Invoke(state);
        }
        public static void InvokeOnAdvanceTurnPhase() {
            OnAdvanceTurnPhase?.Invoke();
        }
        public static void InvokeOnTurnPhaseChanged(CombatStateEnums.TurnPhase phase) {
            OnTurnPhaseChanged?.Invoke(phase);
        }
        public static void InvokeOnApplyAttackOutcome(CombatOutcome outcome) {
            OnApplyAttackOutcome?.Invoke(outcome);
        }
        public static void InvokeOnFinalizePlayerAction() {
            OnFinalizePlayerAction?.Invoke();
        }
     
        

        // ==============================
        // FUNCS
        // ==============================
        public static Func<GameStateData.GameStateDatagram> GetCurrentGameState;
        public static Func<ManualPath> GetManualPath;
        public static Func<int> ManualPathSelectionGetSpentMovementCost;

    }
}
