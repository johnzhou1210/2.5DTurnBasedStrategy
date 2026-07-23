using System;
using System.Collections;
using System.Collections.Generic;
using StrategyGame.Audio;
using StrategyGame.Combat;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.GameState;
using StrategyGame.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace StrategyGame.UI.Menus {
    public enum ActionMenuPage {
        Main,
        Skills,
        Items,
        Current
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
        [SerializeField] private SkillOrItemToolTipRenderer skillOrItemToolTipRenderer;
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
            InputDelegates.OnCancelPressed += HandleCancellation;
            UIDelegates.OnResetCombatActionMenuIndices += ResetIndices;
        }
        private void OnDisable() {
            UIDelegates.OnSetCombatActionMenuVisibility -= SetVisible;
            InputDelegates.OnDownPressed -= SelectNextAction;
            InputDelegates.OnUpPressed -= SelectPreviousAction;
            InputDelegates.OnConfirmPressed -= ConfirmAction;
            InputDelegates.OnCancelPressed -= HandleCancellation;
            UIDelegates.OnResetCombatActionMenuIndices -= ResetIndices;
        }
        private void SetVisible(bool visible, ActionMenuPage page) {
            if (page == ActionMenuPage.Current) {
                // Don't overwrite _currentMenuPage.
            } else {
                _currentMenuPage = page;
            }
            
            if (visible) {
                UpdateAttackActionAllowed();
                UpdateSkillsActionAllowed();
                UpdateItemsActionAllowed();
            }
            rootCanvasGroup.alpha = visible ? 1 : 0;
            rootCanvasGroup.interactable = visible;
            rootCanvasGroup.blocksRaycasts = visible;

            // Fade/hide all non-root canvas groups before deciding which one to show
            actionMenuCanvasGroup.alpha = .25f;
            skillOrItemMenuCanvasGroup.alpha = 0f;
            Transform targetContainer = GetCurrItemsContainerTransform();

            GridEntity selectedEntity = EntityDelegates.GetGridEntityByID(GameStateDelegates.GetCurrentGameState().Combat.SelectedEntityID);
            switch (_currentMenuPage) {
                case ActionMenuPage.Main: actionMenuCanvasGroup.alpha = 1f; break;
                case ActionMenuPage.Skills:
                    skillOrItemMenuCanvasGroup.alpha = 1f;
                    // Clear and regenerate skill entries
                    ClearCurrContainerEntries(GetCurrItemsContainerTransform());
                    GenerateSkills(selectedEntity.AbilityMap);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(targetContainer.GetComponent<RectTransform>());
                    break;
                case ActionMenuPage.Items:
                    skillOrItemMenuCanvasGroup.alpha = 1f;
                    ClearCurrContainerEntries(GetCurrItemsContainerTransform());
                    // Clear and regenerate item entries
                    break;
                default: break;
            }
            int targetIndex = GetCurrSelectedIndex();
            if (targetIndex == -1) return;
            
            // Deselect everything first
            foreach (Transform child in targetContainer) {
                child.GetComponent<Animator>().Play("Deselected");
            }
            StartCoroutine(DelayedSelectAction(targetIndex));
            return;

            void GenerateSkills(Dictionary<int, int> abilityMap) {
                foreach (KeyValuePair<int, int> kvp in abilityMap) {
                    int abilityID = kvp.Key;
                    int currCooldown = kvp.Value;
                    AbilityData currAbility = DataDelegates.GetAbilityDataByID(abilityID);
                    GameObject newEntry = Instantiate(longEntryPrefab, targetContainer);
                    SkillOrItemEntryRenderer newEntryRenderer = newEntry.GetComponent<SkillOrItemEntryRenderer>();
                    if (newEntryRenderer == null)
                        throw new Exception("CombatActionMenuController.SetVisible: SkillOrItemEntryRenderer is null!");
                    newEntryRenderer.SetHeaderText(currAbility.name);
                    newEntryRenderer.SetCooldownInfo(currCooldown, currAbility.MaxCooldown);
                    newEntryRenderer.RelevantID = abilityID;
                }
            }
        }

        private IEnumerator DelayedSelectAction(int actionIndex) {
            yield return new WaitForEndOfFrame();
            SelectAction(actionIndex);
        }
        
        private void SelectAction(int actionIndex) {
            switch (_currentMenuPage) {
                case ActionMenuPage.Main:
                    _actionMenuPreviousSelectedIndex = _actionMenuCurrentSelectedIndex;
                    _actionMenuCurrentSelectedIndex = actionIndex;
                    if (_actionMenuPreviousSelectedIndex != _actionMenuCurrentSelectedIndex) {
                        GameObject previousSelectedActionItem = actionMenuItems.transform.GetChild(_actionMenuPreviousSelectedIndex).gameObject;
                        Animator previousActionItemAnimator = previousSelectedActionItem.GetComponent<Animator>();
                        previousActionItemAnimator.Play("ActionDeselect");
                    }
                    GameObject selectedActionItem = actionMenuItems.transform.GetChild(actionIndex).gameObject;
                    Animator actionItemAnimator = selectedActionItem.GetComponent<Animator>();
                    actionItemAnimator.Play("ActionSelect");
                    break;
                case ActionMenuPage.Skills:
                    _skillMenuPreviousSelectedIndex = _skillMenuCurrentSelectedIndex;
                    _skillMenuCurrentSelectedIndex = actionIndex;
                    if (_skillMenuPreviousSelectedIndex != _skillMenuCurrentSelectedIndex) {
                        GameObject previousSelectedSkillItem = skillOrItemMenuItems.transform.GetChild(_skillMenuPreviousSelectedIndex).gameObject;
                        Animator previousSkillItemAnimator = previousSelectedSkillItem.GetComponent<Animator>();
                        previousSkillItemAnimator.Play("ActionDeselect");
                    }
                    GameObject selectedSkillItem = skillOrItemMenuItems.transform.GetChild(actionIndex).gameObject;
                    Animator actionSkillAnimator = selectedSkillItem.GetComponent<Animator>();
                    actionSkillAnimator.Play("ActionSelect");

                    // Update tooltip
                    int currAbilityID = selectedSkillItem.GetComponent<SkillOrItemEntryRenderer>().RelevantID;
                    AbilityData currAbility = DataDelegates.GetAbilityDataByID(currAbilityID);
                    skillOrItemToolTipRenderer.SetDescription(currAbility.Description);
                    skillOrItemToolTipRenderer.SetSubDescription((currAbility.CooldownAtStart ? "In cooldown before first use | " : "") + $"{currAbility.MaxCooldown}-turn cooldown");
                    skillOrItemToolTipRenderer.SetAnchoredPositionY(-45f * _skillMenuCurrentSelectedIndex);
                    break;
                case ActionMenuPage.Items:
                    _itemMenuPreviousSelectedIndex = _itemMenuCurrentSelectedIndex;
                    _itemMenuCurrentSelectedIndex = actionIndex;
                    if (_itemMenuPreviousSelectedIndex != _itemMenuCurrentSelectedIndex) {
                        GameObject previousSelectedItemItem = skillOrItemMenuItems.transform.GetChild(_itemMenuPreviousSelectedIndex).gameObject;
                        Animator previousItemItemAnimator = previousSelectedItemItem.GetComponent<Animator>();
                        previousItemItemAnimator.Play("ActionDeselect");
                    }
                    GameObject selectedItemItem = skillOrItemMenuItems.transform.GetChild(actionIndex).gameObject;
                    Animator itemItemAnimator = selectedItemItem.GetComponent<Animator>();
                    itemItemAnimator.Play("ActionSelect");
                    break;
                default: break;
            }
        }
        private void SelectNextAction() {
            int targetCurrSelectedIndex = GetCurrSelectedIndex();
            Transform targetTransform = GetCurrItemsContainerTransform();
            if (targetCurrSelectedIndex == -1)
                return;
            if (targetTransform == null)
                return;
            SelectAction((targetCurrSelectedIndex + 1) % targetTransform.childCount);
        }
        private void SelectPreviousAction() {
            int targetCurrSelectedIndex = GetCurrSelectedIndex();
            Transform targetTransform = GetCurrItemsContainerTransform();
            if (targetCurrSelectedIndex == -1)
                return;
            if (targetTransform == null)
                return;
            SelectAction((targetCurrSelectedIndex - 1 + targetTransform.childCount) % targetTransform.childCount);
        }
        private void ConfirmAction() {
            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/select_005"));
            GameStateData.GameStateDatagram currState = GameStateDelegates.GetCurrentGameState();
            GridEntity currentSelectedEntity = EntityDelegates.GetGridEntityByID(currState.Combat.SelectedEntityID);
            int currSelectedIndex = GetCurrSelectedIndex();
            switch (_currentMenuPage) {
                case ActionMenuPage.Main:
                    switch (currSelectedIndex) {
                        case (int)ActionType.Attack:
                            if (currentSelectedEntity.GetAttackableEntitiesAtPosition(currentSelectedEntity.GridPosition, currentSelectedEntity.BasicAttack).Count == 0) break;
                            GameStateDelegates.InvokeOnPlayerPhaseStateChanged(CombatStateEnums.PlayerPhaseState.UnitSelectTarget);
                            SetVisible(false, ActionMenuPage.Main);
                            break;
                        case (int)ActionType.Skills: 
                            if (currentSelectedEntity.Abilities.Count == 0) break;
                            SetVisible(true, ActionMenuPage.Skills); 
                            break;
                        case (int)ActionType.Item:
                            if (currentSelectedEntity.Inventory.Count == 0) break;
                            SetVisible(true, ActionMenuPage.Items);
                            break;
                        case (int)ActionType.Wait:
                            SelectAction(0);
                            ResetIndices();
                            SetVisible(false, ActionMenuPage.Main);
                            GameStateDelegates.InvokeOnFinalizePlayerAction();
                            break;
                        default: throw new Exception("CombatActionMenuController.ConfirmAction: Invalid Action Type!");
                    }
                    break;
                case ActionMenuPage.Skills:
                    GameObject selectedSkillItem = skillOrItemMenuItems.transform.GetChild(_skillMenuCurrentSelectedIndex).gameObject;
                    SkillOrItemEntryRenderer skillOrItemEntry = selectedSkillItem.GetComponent<SkillOrItemEntryRenderer>();
                    int currAbilityID = skillOrItemEntry.RelevantID;
                    // If skill not ready, do nothing.
                    if (!currentSelectedEntity.AbilityMap.TryGetValue(currAbilityID, out int value)) throw new Exception("CombatActionMenuController.ConfirmAction: Ability not found in current selected entity's ability map!");
                    if (value != 0) {
                        // Provide some feedback to show it can't be selected
                        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/error_005"));
                        break;
                    }
                    // Go to target selection phase
                    currState.Combat.CurrentSelectedSkillID = currAbilityID;
                    ClearCurrContainerEntries(GetCurrItemsContainerTransform());
                    SetVisible(false, ActionMenuPage.Skills);
                    GameStateDelegates.InvokeOnPlayerPhaseStateChanged(CombatStateEnums.PlayerPhaseState.UnitSelectTarget);
                    break;
                case ActionMenuPage.Items:
                    // Go to target selection phase
                    break;
                case ActionMenuPage.Current: Debug.LogWarning("CombatActionMenuController.ConfirmAction: ActionMenuPage is Current, which means that the currentActionMenuPage hasn't been updated correctly."); break;
            }
        }
        private void UpdateAttackActionAllowed() {
            Transform attackButtonTransform = actionMenuItems.transform.GetChild(0);
            GameStateData.GameStateDatagram currState = GameStateDelegates.GetCurrentGameState();
            GridEntity currentSelectedEntity = EntityDelegates.GetGridEntityByID(currState.Combat.SelectedEntityID);
            TextMeshProUGUI attackTextMesh = attackButtonTransform.GetComponentInChildren<TextMeshProUGUI>();
            attackTextMesh.color = currentSelectedEntity.GetAttackableEntitiesAtPosition(currentSelectedEntity.GridPosition, currentSelectedEntity.BasicAttack).Count == 0 ? Color.gray4 : Color.white;
        }
        private void UpdateSkillsActionAllowed() {
            Transform skillsButtonTransform = actionMenuItems.transform.GetChild(1);
            GameStateData.GameStateDatagram currState = GameStateDelegates.GetCurrentGameState();
            GridEntity currentSelectedEntity = EntityDelegates.GetGridEntityByID(currState.Combat.SelectedEntityID);
            TextMeshProUGUI skillsTextMesh = skillsButtonTransform.GetComponentInChildren<TextMeshProUGUI>();
            skillsTextMesh.color = currentSelectedEntity.Abilities.Count == 0 ? Color.gray4 : Color.white;
        }
        private void UpdateItemsActionAllowed() {
            Transform itemsButtonTransform = actionMenuItems.transform.GetChild(2);
            GameStateData.GameStateDatagram currState = GameStateDelegates.GetCurrentGameState();
            GridEntity currentSelectedEntity = EntityDelegates.GetGridEntityByID(currState.Combat.SelectedEntityID);
            TextMeshProUGUI itemsTextMesh = itemsButtonTransform.GetComponentInChildren<TextMeshProUGUI>();
            itemsTextMesh.color = currentSelectedEntity.Inventory.Count == 0 ? Color.gray4 : Color.white;
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
        private void ClearCurrContainerEntries(Transform targetContainer) {
            for (int i = targetContainer.childCount - 1; i >= 0; i--) {
                Destroy(targetContainer.GetChild(i).gameObject);
            }
        }
        private void ResetIndices() {
        _actionMenuCurrentSelectedIndex = 0;
        _actionMenuPreviousSelectedIndex = 0;
        _skillMenuCurrentSelectedIndex = 0;
        _skillMenuPreviousSelectedIndex = 0;
        _itemMenuCurrentSelectedIndex = 0;
        _itemMenuPreviousSelectedIndex = 0;
        }
        private void HandleCancellation() {
            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/minimize_008"));
            switch (_currentMenuPage) {
                case ActionMenuPage.Main:
                    // Allow undoing of movement and go back to select unit move destination
                    GameStateDelegates.InvokeOnPlayerPhaseStateChanged(CombatStateEnums.PlayerPhaseState.SelectUnitMoveDestination); break;
                case ActionMenuPage.Skills: 
                    ClearCurrContainerEntries(GetCurrItemsContainerTransform());
                    SetVisible(true, ActionMenuPage.Main); 
                    break;
                case ActionMenuPage.Items: 
                    ClearCurrContainerEntries(GetCurrItemsContainerTransform());
                    SetVisible(true, ActionMenuPage.Main); 
                    break;
                case ActionMenuPage.Current: break;
                default: throw new Exception("CombatActionMenuController.HandleCancellation: Invalid ActionMenuPage (current)!");
            }
        }
    }
}
