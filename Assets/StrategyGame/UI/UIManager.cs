using System;
using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using UnityEngine;

namespace StrategyGame.UI {

    public enum UICategory {
        MainMenu,
        Battle,
    }
    
    public class UIManager : MonoBehaviour
    {
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        // [SerializeField] private GameObject MainMenuUI;
        [SerializeField] private GameObject battleUI;
        
        
        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        private void OnEnable() {
            UIDelegates.OnSetUIActive += SetUIActive;
        }

        private void OnDisable() {
            UIDelegates.OnSetUIActive -= SetUIActive;
        }


        // ==============================
        // CORE METHODS
        // ==============================
        private void SetUIActive(UICategory category, bool active) {
            switch (category) {
                case UICategory.Battle:
                    battleUI.gameObject.SetActive(active);
                    break;
                default:
                    break;
            }
        }

        
        // ==============================
        // HELPERS
        // ==============================
        private HashSet<UICategory> GetActiveUICategories() {
            HashSet<UICategory> result =  new HashSet<UICategory>();
            if (battleUI.activeInHierarchy) result.Add(UICategory.Battle);
            return result;
        }

        private HashSet<UICategory> GetInactiveUICategories() {
            HashSet<UICategory> result =  new HashSet<UICategory>();
            if (!battleUI.activeInHierarchy) result.Add(UICategory.Battle);
            return result;
        }
        
    }
}
