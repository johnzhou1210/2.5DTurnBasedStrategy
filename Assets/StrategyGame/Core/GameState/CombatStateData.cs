using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening.Core.Easing;
using StrategyGame.Combat;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid;
using StrategyGame.UI;
using UnityEngine;

namespace StrategyGame.Core.GameState {
    

    [Serializable]
    public class CombatStateDatagram {
        public int TurnPhaseCycle = 0;
        public CombatStateEnums.TurnPhase TurnPhase = CombatStateEnums.TurnPhase.None;
        [SerializeReference] public List<int> ActorIDsRemaining = new List<int>();
        [SerializeReference] public HashSet<int> DeadEntityIDs = new HashSet<int>();
        public CombatStateEnums.PlayerPhaseState PlayerPhase = CombatStateEnums.PlayerPhaseState.None;
        public CombatStateEnums.EnemyPhaseState EnemyPhase = CombatStateEnums.EnemyPhaseState.None;
        public CombatStateEnums.UnitMoveSelectionMode UnitMoveSelectionMode = CombatStateEnums.UnitMoveSelectionMode.None;
        public Vector2Int InspectedTilePosition;
        public (Vector2Int, bool) PlayerPositionBeforeMovementAndFlipX;

        // Transient state data (not meant to be saved)
        public bool NextActorReady = true;
        public bool EnemyActorFinishedCombatCinematic = true;
        public bool PlayerDirectAttackAvailable = false;
        public int HighestPriorityTargetEntityID = -1;
        public CombatPreview CombatPreview;
        public int CurrentSelectedSkillID = -1;
        public int CurrentSelectedItemID = -1;
        [SerializeField] private int inspectedEntityID = -1;
        public int InspectedEntityID {
            get => inspectedEntityID;
            set {
                previousSelectedEntityID = inspectedEntityID;
                inspectedEntityID = value;
                if (PlayerPhase == CombatStateEnums.PlayerPhaseState.UnitSelectTarget) {
                    HandleCombatOutcomeInspectionUI();
                    return;
                }
                UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.BattleOutcomePreview);
                HandleEntityInspectionUI();
            }
        }
        public int SelectedEntityID = -1;
        [SerializeField] private int previousSelectedEntityID = -1;

        public int PreviousSelectedEntityID { get => previousSelectedEntityID; set => previousSelectedEntityID = value; }

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
            AbilityData currentAbility = DataDelegates.GetAbilityDataByID(CurrentSelectedSkillID);
            if (currentAbility == null) currentAbility = selectedEntity.BasicAttack;
            if (!selectedEntity.CanTargetWith(currentAbility, inspectedEntity)) {
                UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.BattleOutcomePreview);
                UIAnimationDelegates.InvokeOnShowIfHidden(AnimatorCategory.EntityHUD);
                return;
            }
            UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.EntityHUD, true);
            UIAnimationDelegates.InvokeOnShowIfHidden(AnimatorCategory.BattleOutcomePreview);

            // Calculate combat outcome
            HashSet<GridEntity> entitiesWithinDefenderRange = inspectedEntity.GetAttackableEntitiesAtPosition(inspectedEntity.GridPosition, inspectedEntity.BasicAttack);
            bool attackerInDefenderRange = entitiesWithinDefenderRange.Any(e => e.ID == selectedEntity.ID);
            CombatPreview combatPreview = CombatResolver.SimulateAttackPreview(selectedEntity.GetCombatStats(), inspectedEntity.GetCombatStats(), CurrentSelectedSkillID == -1 && CurrentSelectedItemID == -1 ?
                selectedEntity.BasicAttack :
                CurrentSelectedSkillID != -1 ? DataDelegates.GetAbilityDataByID(CurrentSelectedSkillID) : selectedEntity.BasicAttack, attackerInDefenderRange);
            Debug.Log(combatPreview);
            UIDelegates.InvokeOnBattleOutcomePreviewUpdate(combatPreview);
            CombatPreview = combatPreview;
        }
    }
}
