using System;
using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Grid;
using StrategyGame.UI;
using UnityEngine;

namespace StrategyGame.Core.GameState {
    [Serializable]
    public class GameStateData {
        public CombatStateData Combat;
        public GameStateEnums.MasterState MasterState;
    }

    [Serializable]
    public class CombatStateData {
        public int TurnPhaseCycle = 0;
        public GameStateEnums.TurnPhase TurnPhase = GameStateEnums.TurnPhase.None;
        [SerializeReference] public List<int> ActorIDsRemaining = new List<int>();
        public GameStateEnums.PlayerPhaseState PlayerPhase = GameStateEnums.PlayerPhaseState.None;
        public GameStateEnums.EnemyPhaseState EnemyPhase = GameStateEnums.EnemyPhaseState.None;
        public GameStateEnums.UnitMoveSelectionMode UnitMoveSelectionMode = GameStateEnums.UnitMoveSelectionMode.None;
        public Vector2Int InspectedTilePosition;
        public bool NextActorReady = true;

        [SerializeField] private int inspectedEntityID = -1;
        public int InspectedEntityID {
            get => inspectedEntityID;
            set {
                previousSelectedEntityID = inspectedEntityID;
                inspectedEntityID = value;
                if (inspectedEntityID == -1 && previousSelectedEntityID != -1) {
                    UIAnimationDelegates.InvokeOnPlayAnimation(AnimatorCategory.EntityHUD, "TweenOut");
                    return;
                }
                if (previousSelectedEntityID == -1 && inspectedEntityID == -1) return;
                if (previousSelectedEntityID != -1 && inspectedEntityID != -1) return;
                if (inspectedEntityID != -1) {
                    UIAnimationDelegates.InvokeOnPlayAnimation(AnimatorCategory.EntityHUD, "TweenIn");
                }
                else if (inspectedEntityID == -1) {
                    UIAnimationDelegates.InvokeOnPlayAnimation(AnimatorCategory.EntityHUD, "TweenOut");
                }
            }
        }
        public int SelectedEntityID = -1;
        [SerializeField] private int previousSelectedEntityID = -1;

        public int PreviousSelectedEntityID {
            get => previousSelectedEntityID;
            set => previousSelectedEntityID = value;
        }
    }
}
