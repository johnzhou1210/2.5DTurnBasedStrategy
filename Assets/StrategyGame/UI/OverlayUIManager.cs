using System;
using System.ComponentModel;
using StrategyGame.Core.Delegates;
using UnityEngine;

namespace StrategyGame.UI {
    public enum AnimatorCategory {
        CanvasRoot,
        EntityHUD,
        WinLoseConditions
    }
    public class OverlayUIManager : MonoBehaviour {
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        [SerializeField] private Animator canvasRootAnimator; // Controls Global Overlay Blur
        [SerializeField] private Animator entityHUDAnimator;
        [SerializeField] private Animator winLoseConditionsAnimator;
        
        
        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        private void OnEnable() {
            UIAnimationDelegates.OnPlayAnimation += OnPlayAnimation;
        }

        private void OnDisable() {
            UIAnimationDelegates.OnPlayAnimation -= OnPlayAnimation;
        }

        // ==============================
        // CORE METHODS
        // ==============================
        private void OnPlayAnimation(AnimatorCategory category, string animationName) {
            switch (category) {
                case AnimatorCategory.CanvasRoot:
                    canvasRootAnimator.Play(animationName);
                    break;
                case AnimatorCategory.EntityHUD:
                    entityHUDAnimator.Play(animationName);
                    break;
                case AnimatorCategory.WinLoseConditions:
                    winLoseConditionsAnimator.Play(animationName);
                    break;
                default:
                    throw new InvalidEnumArgumentException("Invalid animator category!");
            }
        }
        
        
    }
}
