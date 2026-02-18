using System;
using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Core.GameState;
using StrategyGame.Grid;
using TMPro;
using UnityEngine;

namespace StrategyGame.UI.Menus {
    public enum ActionMenuPage {
        Main,
        Skills,
        Items,
        None
    }
public class CombatActionMenuController : MonoBehaviour {
    private enum ActionType {
        Attack,
        Skills,
        Item,
        Wait
    }
    
    [SerializeField] private GameObject actionMenuItems;
    [SerializeField] private GameObject actionMenuRoot;
    [SerializeField] private GameObject skillOrItemMenuRoot;
    [SerializeField] private GameObject skillOrItemMenuItems;
    [SerializeField] private CanvasGroup rootCanvasGroup;
    [SerializeField] private CanvasGroup actionMenuCanvasGroup;
    [SerializeField] private CanvasGroup skillOrItemMenuCanvasGroup;

    [SerializeField] private GameObject longEntryPrefab;

    private int _actionMenuCurrentSelectedIndex = 0;
    private int _actionMenuPreviousSelectedIndex = 0;
    private int _skillMenuCurrentSelectedIndex = 0;
    private int _skillMenuPreviousSelectedIndex = 0;
    private int _itemMenuCurrentSelectedIndex = 0;
    private int _itemMenuPreviousSelectedIndex = 0;
    
    private ActionMenuPage _currentMenuPage = ActionMenuPage.Main;
    

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

    private void SetVisible(bool visible, ActionMenuPage page) {
        _currentMenuPage = page;
        if (visible) UpdateAttackActionAllowed();
        rootCanvasGroup.alpha = _currentMenuPage == ActionMenuPage.None ? 0 : 1f;
        rootCanvasGroup.interactable = visible;
        rootCanvasGroup.blocksRaycasts = visible;
        
        // Fade/hide all non-root canvas groups before deciding which one to show
        actionMenuCanvasGroup.alpha = .1f;
        skillOrItemMenuCanvasGroup.alpha = 0f;
        
        Transform targetContainer = GetCurrItemsContainerTransform();

        void ClearCurrContainerEntries() {
            
            foreach (Transform child in targetContainer) {
                Destroy(child.gameObject);
            }
        }
        void GenerateEntriesIntoCurrContainer(List<ScriptableObject> entries) {
            foreach (ScriptableObject entry in entries) {
                GameObject newEntry = Instantiate(longEntryPrefab, targetContainer);
                
            }
        }

        switch (_currentMenuPage) {
            case ActionMenuPage.Main:
                actionMenuCanvasGroup.alpha = 1f;
                break;
            case ActionMenuPage.Skills:
                skillOrItemMenuCanvasGroup.alpha = 1f;
                // Clear and regenerate skill entries
                ClearCurrContainerEntries();
                break;
            case ActionMenuPage.Items:
                skillOrItemMenuCanvasGroup.alpha = 1f;
                // Clear and regenerate item entries
                ClearCurrContainerEntries();
                break;
            default:
                break;
        }
        
        
        int targetIndex = GetCurrSelectedIndex();
        if (targetIndex == -1) throw new Exception("CombatActionMenuController.SetVisible: Invalid target index!");
        SelectAction(targetIndex);
    }

    private void SelectAction(int actionIndex) {
        switch (_currentMenuPage) {
            case ActionMenuPage.Main:
                _actionMenuPreviousSelectedIndex = _actionMenuCurrentSelectedIndex;
                _actionMenuCurrentSelectedIndex = actionIndex;
                if (_actionMenuPreviousSelectedIndex != _actionMenuCurrentSelectedIndex) {
                    GameObject previousSelectedActionItem = actionMenuItems.transform.GetChild(_actionMenuPreviousSelectedIndex).gameObject;
                    Animator previousActionItemAnimator = previousSelectedActionItem.GetComponent<Animator>();
                    if (previousActionItemAnimator.enabled) previousActionItemAnimator.Play("ActionDeselect");
                }
                GameObject selectedActionItem = actionMenuItems.transform.GetChild(actionIndex).gameObject;
                Animator actionItemAnimator = selectedActionItem.GetComponent<Animator>();
                if (actionItemAnimator.enabled) actionItemAnimator.Play("ActionSelect");
                break;
            case ActionMenuPage.Skills:
                _skillMenuPreviousSelectedIndex = _skillMenuCurrentSelectedIndex;
                _skillMenuCurrentSelectedIndex = actionIndex;
                if (_skillMenuPreviousSelectedIndex != _skillMenuCurrentSelectedIndex) {
                    GameObject previousSelectedSkillItem = skillOrItemMenuItems.transform.GetChild(_skillMenuPreviousSelectedIndex).gameObject;
                    Animator previousSkillItemAnimator = previousSelectedSkillItem.GetComponent<Animator>();
                    if (previousSkillItemAnimator.enabled) previousSkillItemAnimator.Play("ActionDeselectLong");
                }
                GameObject selectedSkillItem = skillOrItemMenuItems.transform.GetChild(actionIndex).gameObject;
                Animator actionSkillAnimator = selectedSkillItem.GetComponent<Animator>();
                if (actionSkillAnimator.enabled) actionSkillAnimator.Play("ActionSelectLong");
                break;
            case ActionMenuPage.Items:
                _itemMenuPreviousSelectedIndex = _itemMenuCurrentSelectedIndex;
                _itemMenuCurrentSelectedIndex = actionIndex;
                if (_itemMenuPreviousSelectedIndex != _itemMenuCurrentSelectedIndex) {
                    GameObject previousSelectedItemItem = skillOrItemMenuItems.transform.GetChild(_itemMenuPreviousSelectedIndex).gameObject;
                    Animator previousItemItemAnimator = previousSelectedItemItem.GetComponent<Animator>();
                    if (previousItemItemAnimator.enabled) previousItemItemAnimator.Play("ActionDeselectLong");
                }
                GameObject selectedItemItem = skillOrItemMenuItems.transform.GetChild(actionIndex).gameObject;
                Animator itemItemAnimator = selectedItemItem.GetComponent<Animator>();
                if (itemItemAnimator.enabled) itemItemAnimator.Play("ActionSelectLong");
                break;
            default:
                break;
        }
        

       
    }

    private void SelectNextAction() {
        int targetCurrSelectedIndex = GetCurrSelectedIndex();
        Transform targetTransform = GetCurrItemsContainerTransform();
        if (targetCurrSelectedIndex == -1) return;
        if (targetTransform == null) return;
        SelectAction((targetCurrSelectedIndex + 1) % targetTransform.childCount);
    }

    private void SelectPreviousAction() {
        int targetCurrSelectedIndex = GetCurrSelectedIndex();
        Transform targetTransform = GetCurrItemsContainerTransform();
        if (targetCurrSelectedIndex == -1) return;
        if (targetTransform == null) return;
        SelectAction((targetCurrSelectedIndex - 1 + targetTransform.childCount) % targetTransform.childCount);
    }

    private void ConfirmAction() {
        GameStateData currState = GameStateDelegates.GetCurrentGameState();
        GridEntity currentSelectedEntity = EntityDelegates.GetGridEntityByID(currState.Combat.SelectedEntityID);
        int currSelectedIndex = GetCurrSelectedIndex();

        switch (_currentMenuPage) {
            case ActionMenuPage.Main:
                switch (currSelectedIndex) {
                    case (int)ActionType.Attack:
                        if (currentSelectedEntity.GetAttackableEntitiesAtPosition(currentSelectedEntity.GridPosition).Count == 0) break;
                        GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.UnitSelectTarget);
                        SetVisible(false, ActionMenuPage.None);
                        break;
                    case (int)ActionType.Skills:
                        SetVisible(true, ActionMenuPage.Skills);
                        break;
                    case (int)ActionType.Item:
                        SetVisible(true, ActionMenuPage.Items);
                        break;
                    case (int)ActionType.Wait:
                        SetVisible(false, ActionMenuPage.None);
                        GameStateDelegates.InvokeOnFinalizePlayerAction();
                        SelectAction(0);
                        break;
                    default:
                        throw new Exception("CombatActionMenuController.ConfirmAction: Invalid Action Type!");
                }
                break;
            case ActionMenuPage.Skills:
                // Go to target selection phase
                break;
            case ActionMenuPage.Items:
                // Go to target selection phase
                break;
            case ActionMenuPage.None:
                Debug.LogWarning("CombatActionMenuController.ConfirmAction: ActionMenuPage is None, doing nothing.");
                break;
        }
        
       
    }

    private void UpdateAttackActionAllowed() {
        Transform attackButtonTransform = actionMenuItems.transform.GetChild(0);
        GameStateData currState = GameStateDelegates.GetCurrentGameState();
        GridEntity currentSelectedEntity = EntityDelegates.GetGridEntityByID(currState.Combat.SelectedEntityID);
        TextMeshProUGUI attackTextMesh = attackButtonTransform.GetComponentInChildren<TextMeshProUGUI>();
        attackTextMesh.color = currentSelectedEntity.GetAttackableEntitiesAtPosition(currentSelectedEntity.GridPosition).Count == 0 ? Color.gray4 : Color.white;
    }

    private int GetCurrSelectedIndex() {
        int currSelectedIndex = _currentMenuPage switch {
            ActionMenuPage.Main => _actionMenuCurrentSelectedIndex,
            ActionMenuPage.Skills => _skillMenuCurrentSelectedIndex,
            ActionMenuPage.Items => _itemMenuCurrentSelectedIndex,
            _ => -1
        };
        return currSelectedIndex;
    }
    private Transform GetCurrItemsContainerTransform() {
        Transform currItemsContainerTransform = _currentMenuPage switch {
            ActionMenuPage.Main => actionMenuItems.transform,
            ActionMenuPage.Skills => skillOrItemMenuItems.transform,
            ActionMenuPage.Items => skillOrItemMenuItems.transform,
            _ => null
        };
        return currItemsContainerTransform;
    }
}
}