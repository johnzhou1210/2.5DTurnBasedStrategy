using System;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Core.GameState;
using StrategyGame.Grid;
using TMPro;
using UnityEngine;

namespace StrategyGame.UI.Menus {
public class CombatActionMenuController : MonoBehaviour {
    [SerializeField] private GameObject actionMenuItems;
    [SerializeField] private GameObject actionMenuRoot;
    [SerializeField] private CanvasGroup canvasGroup;

    private int _currentSelectedIndex = 0;
    private int _previousSelectedIndex = 0;

    private enum ActionType {
        Attack,
        Skills,
        Item,
        Wait
    }

    private void OnEnable() {
        UIDelegates.OnSetCombatActionMenuVisibility += SetVisible;
        InputDelegates.OnDownPressed += SelectNextAction;
        InputDelegates.OnUpPressed += SelectPreviousAction;
        InputDelegates.OnConfirmPressed += ConfirmAction;
    }

    private void OnDisable() {
        UIDelegates.OnSetCombatActionMenuVisibility -= SetVisible;
        InputDelegates.OnDownPressed -= SelectNextAction;
        InputDelegates.OnUpPressed -= SelectPreviousAction;
        InputDelegates.OnConfirmPressed -= ConfirmAction;
    }

    private void SetVisible(bool visible) {
        if (visible) UpdateAttackActionAllowed();
        canvasGroup.alpha = visible ? 1 : 0;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
        SelectAction(_currentSelectedIndex);
    }

    private void SelectAction(int actionIndex) {
        _previousSelectedIndex = _currentSelectedIndex;
        _currentSelectedIndex = actionIndex;
        if (_previousSelectedIndex != _currentSelectedIndex) {
            GameObject previousSelectedActionItem =
                actionMenuItems.transform.GetChild(_previousSelectedIndex).gameObject;
            Animator previousActionItemAnimator = previousSelectedActionItem.GetComponent<Animator>();
            if (previousActionItemAnimator.enabled) previousActionItemAnimator.Play("ActionDeselect");
        }

        GameObject selectedActionItem = actionMenuItems.transform.GetChild(actionIndex).gameObject;
        Animator actionItemAnimator = selectedActionItem.GetComponent<Animator>();
       if (actionItemAnimator.enabled) actionItemAnimator.Play("ActionSelect");
    }

    private void SelectNextAction() {
        SelectAction((_currentSelectedIndex + 1) % actionMenuItems.transform.childCount);
    }

    private void SelectPreviousAction() {
        SelectAction((_currentSelectedIndex - 1 + actionMenuItems.transform.childCount) % actionMenuItems.transform.childCount);
    }

    private void ConfirmAction() {
        GameStateData currState = GameStateDelegates.GetCurrentGameState();
        GridEntity currentSelectedEntity = EntityDelegates.GetGridEntityByID(currState.Combat.SelectedEntityID);
        switch (_currentSelectedIndex) {
            case (int)ActionType.Attack:
                if (currentSelectedEntity.GetAttackableEntitiesAtPosition(currentSelectedEntity.GridPosition).Count == 0) break;
                GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.UnitSelectTarget);
                SetVisible(false);
            break;
            case (int)ActionType.Skills:
            break;
            case (int)ActionType.Item:
            break;
            case (int)ActionType.Wait:
                SetVisible(false);
                currState.Combat.ActorIDsRemaining.Remove(currState.Combat.SelectedEntityID);
                currState.Combat.PlayersCycleDeque.Remove(currState.Combat.SelectedEntityID);
                GameStateDelegates.InvokeOnPlayerPhaseStateChanged(currState.Combat.ActorIDsRemaining.Count == 0 ? GameStateEnums.PlayerPhaseState.None : GameStateEnums.PlayerPhaseState.SelectUnitToControl);
                if (currState.Combat.ActorIDsRemaining.Count == 0) {
                    GameStateDelegates.InvokeOnAdvanceTurnPhase();
                }
                SelectAction(0);
            break;
            default:
                throw new Exception("CombatActionMenuController.ConfirmAction: Invalid Action Type!");
        }
    }

    private void UpdateAttackActionAllowed() {
        Transform attackButtonTransform = actionMenuItems.transform.GetChild(0);
        GameStateData currState = GameStateDelegates.GetCurrentGameState();
        GridEntity currentSelectedEntity = EntityDelegates.GetGridEntityByID(currState.Combat.SelectedEntityID);
        TextMeshProUGUI attackTextMesh = attackButtonTransform.GetComponentInChildren<TextMeshProUGUI>();
        attackTextMesh.color = currentSelectedEntity.GetAttackableEntitiesAtPosition(currentSelectedEntity.GridPosition).Count == 0 ? Color.gray4 : Color.white;
    }
}
}