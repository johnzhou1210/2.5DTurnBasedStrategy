namespace StrategyGame.Core.Enums {
    public class GameStateEnums {
        public enum TurnPhase {
            Player,
            Enemy,
            Event,
            None
        }

        public enum PlayerPhaseState {
            SelectUnitToControl,
            SelectUnitMoveDestination,
            UnitActionMenu,
            UnitSelectTarget,
            UnitAttackCutscene,
            None
        }

        public enum UnitMoveSelectionMode {
            Manual,
            Automatic,
            None
        }
        
    }
}
