using System;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Core.GameState;
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
        switch (_currentSelectedIndex) {
            case (int)ActionType.Attack:
            break;
            case (int)ActionType.Skills:
            break;
            case (int)ActionType.Item:
            break;
            case (int)ActionType.Wait:
                GameStateData currState = GameStateDelegates.GetCurrentGameState();
                currState.Combat.ActorsIDsRemaining.Remove(currState.Combat.SelectedEntityID);
                if (currState.Combat.ActorsIDsRemaining.Count == 0) {
                    GameStateDelegates.InvokeOnAdvanceTurnPhase();
                }
                GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.SelectUnitToControl);
                SetVisible(false);
                SelectAction(0);
            break;
            default:
                throw new Exception("CombatActionMenuController.ConfirmAction: Invalid Action Type!");
        }
    }
}
}