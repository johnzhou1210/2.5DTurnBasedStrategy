namespace StrategyGame.Core.GameState {
    public class CombatStateEnums {
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

       
        
    }

    public class MasterStateEnums {
        public enum MasterState {
            Title,
            Home,
            Combat,
            
            
        }
    }
}
