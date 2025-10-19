namespace StrategyGame.Core.Enums {
    public class GameStateEnums {
        public enum TurnPhase {
            Player,
            Enemy,
            Event,
            None
        }

        public enum PlayerPhaseState {
            SelectUnitToMove,
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

        public enum ManualMoveSelectionState {
            AwaitingUnitSelection,
            FormingPath,
            None
        }
    }
}
