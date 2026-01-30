using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StrategyGame.Combat;
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
                if (PlayerPhase == GameStateEnums.PlayerPhaseState.UnitSelectTarget) {
                    HandleCombatOutcomeInspectionUI();
                    return;
                }
                HandleEntityInspectionUI();
            }
        }
        public int SelectedEntityID = -1;
        [SerializeField] private int previousSelectedEntityID = -1;

        public int PreviousSelectedEntityID {
            get => previousSelectedEntityID;
            set => previousSelectedEntityID = value;
        }

        public LinkedList<int> PlayersCycleDeque = new LinkedList<int>();
        public LinkedList<int> EnemiesCycleDeque = new LinkedList<int>();

        private void HandleEntityInspectionUI() {

            if (InspectedEntityID == -1) {
                UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.EntityHUD);
                UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.BattleOutcomePreview);
                return;
            }
            UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.BattleOutcomePreview);
            UIAnimationDelegates.InvokeOnShowIfHidden(AnimatorCategory.EntityHUD);
        }

        private void HandleCombatOutcomeInspectionUI() {
            if (InspectedEntityID == -1) {
                UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.EntityHUD);
                UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.BattleOutcomePreview);
                return;
            }
            GridEntity inspectedEntity = EntityDelegates.GetGridEntityByID(InspectedEntityID);
            GridEntity selectedEntity = EntityDelegates.GetGridEntityByID(SelectedEntityID);
            if (selectedEntity.IsFriendlyWith(inspectedEntity)) {
                UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.BattleOutcomePreview);
                UIAnimationDelegates.InvokeOnShowIfHidden(AnimatorCategory.EntityHUD);
                return;
            }
            UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.EntityHUD, true);
            UIAnimationDelegates.InvokeOnShowIfHidden(AnimatorCategory.BattleOutcomePreview);

            // Calculate combat outcome
            HashSet<GridEntity> entitiesWithinDefenderRange = inspectedEntity.GetAttackableEntitiesAtPosition(inspectedEntity.GridPosition);
            bool attackerInDefenderRange = entitiesWithinDefenderRange.Any(e => e.ID == selectedEntity.ID);
            CombatPreview combatPreview = CombatResolver.SimulateAttackPreview(new CombatStats {
                    HP = selectedEntity.Health,
                    Attack = selectedEntity.Attack,
                    Defense = selectedEntity.Defense,
                    Agility = selectedEntity.Agility,
                    Accuracy = selectedEntity.Accuracy,
                    Resistance = selectedEntity.Resistance,
                    Evasion = selectedEntity.Evasion,
                    Weapon = selectedEntity.Weapon,
                    EntityID = selectedEntity.ID
                }, new CombatStats {
                    HP = inspectedEntity.Health,
                    Attack = inspectedEntity.Attack,
                    Defense = inspectedEntity.Defense,
                    Agility = inspectedEntity.Agility,
                    Accuracy = inspectedEntity.Accuracy,
                    Resistance = inspectedEntity.Resistance,
                    Evasion = inspectedEntity.Evasion,
                    Weapon = inspectedEntity.Weapon,
                    EntityID = inspectedEntity.ID
                },
                Resources.Load<AbilityData>("ScriptableObjects/Abilities/Attack"), attackerInDefenderRange);
            Debug.Log(combatPreview);
        }
    }
}
