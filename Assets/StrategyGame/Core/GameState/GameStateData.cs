using System.Collections.Generic;
using StrategyGame.Core.Enums;
using StrategyGame.Grid;

namespace StrategyGame.Core.GameState {
    public class GameStateData {
        public CombatStateData Combat;
    }

    public class CombatStateData {
        public GameStateEnums.TurnPhase TurnPhase = GameStateEnums.TurnPhase.None;
        public List<GridEntity> ActorsRemaining = new List<GridEntity>();
        public GameStateEnums.PlayerPhaseState PlayerPhase = GameStateEnums.PlayerPhaseState.None;
        public GameStateEnums.UnitMoveSelectionMode UnitMoveSelectionMode = GameStateEnums.UnitMoveSelectionMode.None;
    }
}
