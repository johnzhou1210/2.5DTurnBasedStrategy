using System;
using Mono.CSharp.Linq;
using StrategyGame.Audio;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Input;
using StrategyGame.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StrategyGame.UI.Menus {
    public class TitleScreenMenuController : MonoBehaviour {
        private enum MainMenuItemType {
            NewGame,
            Continue,
            Settings,
            QuitGame
        }
        
        [SerializeField] private GameObject mainMenuItems;
        private int _mainMenuCurrentSelectedIndex = 0;
        private int _mainMenuPreviousSelectedIndex = 0;

        private void OnEnable() {
            InputDelegates.OnDownPressed += GoDown;
            InputDelegates.OnUpPressed += GoUp;
            InputDelegates.OnConfirmPressed += ConfirmSelection;
        }
        private void OnDisable() {
            InputDelegates.OnDownPressed -= GoDown;
            InputDelegates.OnUpPressed -= GoUp;
            InputDelegates.OnConfirmPressed -= ConfirmSelection;
        }

        private void Start() {
            SelectItem(_mainMenuCurrentSelectedIndex);
        }

        private void GoDown() {
            int targetCurrSelectedIndex = GetCurrSelectedIndex();
            Transform targetTransform = GetCurrItemsContainerTransform();
            if (targetCurrSelectedIndex == -1) return;
            if (targetTransform == null) return;
            SelectItem((targetCurrSelectedIndex + 1) % targetTransform.childCount);
        }
        private void GoUp() {
            int  targetCurrSelectedIndex = GetCurrSelectedIndex();
            Transform targetTransform = GetCurrItemsContainerTransform();
            if (targetCurrSelectedIndex == -1) return;
            if (targetTransform == null) return;
            SelectItem((targetCurrSelectedIndex - 1 + targetTransform.childCount) % targetTransform.childCount);
        }

        private void ResetIndices() {
            _mainMenuCurrentSelectedIndex = 0;
        }

        private int GetCurrSelectedIndex() {
            return _mainMenuCurrentSelectedIndex;
        }
        private Transform GetCurrItemsContainerTransform() {
            return mainMenuItems.transform;
        }
        private void SelectItem(int itemIndex) {
            _mainMenuPreviousSelectedIndex = _mainMenuCurrentSelectedIndex;
            _mainMenuCurrentSelectedIndex = itemIndex;
            Debug.Log($"TitleScreenMenuController.SelectItem: Main menu selected item index is {_mainMenuCurrentSelectedIndex}");
            if (_mainMenuPreviousSelectedIndex != _mainMenuCurrentSelectedIndex) {
                GameObject previousSelectedItem = mainMenuItems.transform.GetChild(_mainMenuPreviousSelectedIndex).gameObject;
                Animator previousItemAnimator = previousSelectedItem.GetComponent<Animator>();
                previousItemAnimator.Play("TitleItemDeselect");
            }
            GameObject selectedItem = mainMenuItems.transform.GetChild(itemIndex).gameObject;
            Animator selectedItemAnimator = selectedItem.GetComponent<Animator>();
            selectedItemAnimator.Play("TitleItemSelect");
        }

        private void ConfirmSelection() {
            switch (_mainMenuCurrentSelectedIndex) {
                case (int)MainMenuItemType.NewGame:
                    ServiceLocator.Get<AudioManager>().PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/select_005"), volumeMultiplier: 1f);
                    ServiceLocator.Get<CombatInputManager>().enabled = true;
                    SceneManager.LoadScene("Scenes/MainScene");
                    ServiceLocator.Get<MenuInputManager>().enabled = false;
                    break;
                case (int)MainMenuItemType.Continue:
                    ServiceLocator.Get<AudioManager>().PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/back_001"), volumeMultiplier: 1f);
                    break;
                case (int)MainMenuItemType.Settings:
                    ServiceLocator.Get<AudioManager>().PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/back_001"), volumeMultiplier: 1f);
                    break;
                case (int)MainMenuItemType.QuitGame:
                    ServiceLocator.Get<AudioManager>().PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/select_005"), volumeMultiplier: 1f);
                    break;
            }
        }


    }
}
