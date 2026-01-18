using System;
using System.Collections.Generic;
using StrategyGame.Core.Enums;
using StrategyGame.Grid;
using UnityEngine;

namespace StrategyGame.Core.GameState {
    [Serializable]
    public class GameStateData {
        public CombatStateData Combat;
    }

    [Serializable]
    public class CombatStateData {
        public GameStateEnums.TurnPhase TurnPhase = GameStateEnums.TurnPhase.None;
        [SerializeReference] public List<int> ActorsIDsRemaining = new List<int>();
        public GameStateEnums.PlayerPhaseState PlayerPhase = GameStateEnums.PlayerPhaseState.None;
        public GameStateEnums.EnemyPhaseState EnemyPhase = GameStateEnums.EnemyPhaseState.None;
        public GameStateEnums.UnitMoveSelectionMode UnitMoveSelectionMode = GameStateEnums.UnitMoveSelectionMode.None;
        public int InspectedEntityID = -1;
        public int SelectedEntityID = -1;
        public Vector2Int InspectedTilePosition;
        public bool NextActorReady = true;
    }
}
