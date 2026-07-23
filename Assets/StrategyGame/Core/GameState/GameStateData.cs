using System;

namespace StrategyGame.Core.GameState {
    public class GameStateData {
        [Serializable]
        public class GameStateDatagram {
            public CombatStateDatagram Combat;
            public MasterStateEnums.MasterState MasterState;
            
        }
    }
}
