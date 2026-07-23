using System;
using StrategyGame.Core.Delegates;
using StrategyGame.Utils;
using UnityEngine;

namespace StrategyGame.Core.GameState {
    public class MasterStateManager : Singleton<MasterStateManager> {
        [SerializeField] private GameStateData.GameStateDatagram currentState;
        public GameStateData.GameStateDatagram CurrentState => currentState;
        
        private void OnEnable() {
            currentState = new GameStateData.GameStateDatagram {
                Combat = new CombatStateDatagram { TurnPhase = CombatStateEnums.TurnPhase.None, PlayerPhase = CombatStateEnums.PlayerPhaseState.SelectUnitToControl, UnitMoveSelectionMode = CombatStateEnums.UnitMoveSelectionMode.Manual, },
                MasterState = MasterStateEnums.MasterState.Title
            };
            GameStateDelegates.GetCurrentGameState = GetCurrentGameState;
        }

        private void OnDisable() {
            GameStateDelegates.GetCurrentGameState = null;
        }

        private GameStateData.GameStateDatagram GetCurrentGameState() {
            return CurrentState;
        }
        
    }
}
