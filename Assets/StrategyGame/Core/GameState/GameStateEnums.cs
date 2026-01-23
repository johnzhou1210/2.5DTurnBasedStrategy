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
            UnitMovingToDestination,
            UnitActionMenu,
            UnitSelectTarget,
            UnitAttackCutscene,
            None
        }
        
        public enum EnemyPhaseState {
            SelectUnitToControl,
            SelectUnitMoveDestination,
            UnitMovingToDestination,
            UnitContemplateAction,
            UnitSelectTarget,
            UnitAttackCutscene,
            None
        }

        public enum UnitMoveSelectionMode {
            Manual,
            Automatic,
            None
        }

        public enum MasterState {
            Title,
            Home,
            Combat,
            
            
        }
        
    }
}
