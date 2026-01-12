using StrategyGame.Core.Delegates;
using UnityEngine;

namespace StrategyGame.UI.Menus {
public class CombatActionMenuController : MonoBehaviour {
    [SerializeField] private GameObject actionMenuItems;
    [SerializeField] private GameObject actionMenuRoot;

    private int _currentSelectedIndex = 0;
    private int _previousSelectedIndex = 0;


    private void OnEnable() {
        UIDelegates.OnSetCombatActionMenuVisibility += SetVisible;
        InputDelegates.OnDownPressed += SelectNextAction;
        InputDelegates.OnUpPressed += SelectPreviousAction;
        SelectAction(_currentSelectedIndex);
    }

    private void OnDisable() {
        UIDelegates.OnSetCombatActionMenuVisibility -= SetVisible;
        InputDelegates.OnDownPressed -= SelectNextAction;
        InputDelegates.OnUpPressed -= SelectPreviousAction;
    }

    private void SetVisible(bool visible) { actionMenuRoot.SetActive(visible); }

    private void SelectAction(int actionIndex) {
        _previousSelectedIndex = _currentSelectedIndex;
        _currentSelectedIndex = actionIndex;
        if (_previousSelectedIndex != _currentSelectedIndex) {
            GameObject previousSelectedActionItem =
                actionMenuItems.transform.GetChild(_previousSelectedIndex).gameObject;
            Animator previousActionItemAnimator = previousSelectedActionItem.GetComponent<Animator>();
            previousActionItemAnimator.Play("ActionDeselect");
        }

        GameObject selectedActionItem = actionMenuItems.transform.GetChild(actionIndex).gameObject;
        Animator actionItemAnimator = selectedActionItem.GetComponent<Animator>();
        actionItemAnimator.Play("ActionSelect");
    }

    private void SelectNextAction() {
        SelectAction((_currentSelectedIndex + 1) % actionMenuItems.transform.childCount);
    }

    private void SelectPreviousAction() {
        SelectAction((_currentSelectedIndex - 1 + actionMenuItems.transform.childCount) % actionMenuItems.transform.childCount);
    }
}
}