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
        public GridEntity InspectedEntity;
        public GridEntity SelectedEntity;
        public Tile InspectedTile;
        public bool NextActorReady = true;
    }
}
