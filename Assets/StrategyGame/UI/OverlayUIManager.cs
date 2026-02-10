using System;
using System.ComponentModel;
using StrategyGame.Core.Delegates;
using UnityEngine;

namespace StrategyGame.UI {
    public enum AnimatorCategory {
        CanvasRoot,
        EntityHUD,
        WinLoseConditions,
        BattleOutcomePreview,
        BattleCinematicHUD
    }
    public class OverlayUIManager : MonoBehaviour {
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        [SerializeField] private Animator canvasRootAnimator; // Controls Global Overlay Blur
        [SerializeField] private Animator entityHUDAnimator;
        [SerializeField] private Animator winLoseConditionsAnimator;
        [SerializeField] private Animator battleOutcomePreviewAnimator;
        [SerializeField] private Animator battleCinematicHUDAnimator;
        
        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        private void OnEnable() {
            UIAnimationDelegates.OnPlayAnimation += OnPlayAnimation;
            UIAnimationDelegates.OnShow += Show;
            UIAnimationDelegates.OnHide += Hide;
            UIAnimationDelegates.OnShowIfHidden += ShowIfHidden;
            UIAnimationDelegates.OnHideIfVisible += HideIfVisible;
            UIAnimationDelegates.GetIsPlaying = GetCurrentStateName;
        }

        private void OnDisable() {
            UIAnimationDelegates.OnPlayAnimation -= OnPlayAnimation;
            UIAnimationDelegates.OnShow -= Show;
            UIAnimationDelegates.OnHide -= Hide;
            UIAnimationDelegates.OnShowIfHidden -= ShowIfHidden;
            UIAnimationDelegates.OnHideIfVisible -= HideIfVisible;
            UIAnimationDelegates.GetIsPlaying = null;
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
                case AnimatorCategory.BattleOutcomePreview:
                    battleOutcomePreviewAnimator.Play(animationName);
                    break;
                case AnimatorCategory.BattleCinematicHUD:
                    battleCinematicHUDAnimator.Play(animationName);
                    break;
                default:
                    throw new InvalidEnumArgumentException("Invalid animator category!");
            }
        }

        private bool GetCurrentStateName(AnimatorCategory category, string animationName) {
            Animator animator = GetAnimatorFromCategory(category);
            return animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
        }

        private Animator GetAnimatorFromCategory(AnimatorCategory category) {
            switch (category) {
                case AnimatorCategory.CanvasRoot:
                    return canvasRootAnimator;
                case AnimatorCategory.EntityHUD:
                    return entityHUDAnimator;
                case AnimatorCategory.WinLoseConditions:
                    return winLoseConditionsAnimator;
                case AnimatorCategory.BattleOutcomePreview:
                    return battleOutcomePreviewAnimator;
                case AnimatorCategory.BattleCinematicHUD:
                    return battleCinematicHUDAnimator;
                default:
                    throw new Exception("OverlayUIManager.GetIsPlaying: Invalid animator category!");
            }
        }
        
        
        private void Show(AnimatorCategory cat, bool instant = false)
        {
            
            UIAnimationDelegates.InvokeOnPlayAnimation(cat, instant ? "Visible" : "TweenIn");
        }

        private void Hide(AnimatorCategory cat, bool instant = false)
        {
     
            UIAnimationDelegates.InvokeOnPlayAnimation(cat, instant ? "Invisible" : "TweenOut");
        }

        private void ShowIfHidden(AnimatorCategory cat, bool instant = false)
        {
            if (UIAnimationDelegates.GetIsPlaying(cat, "Invisible") || UIAnimationDelegates.GetIsPlaying(cat, "TweenOut")) Show(cat, instant);
        }

        private void HideIfVisible(AnimatorCategory cat, bool instant = false)
        {
     
            if (UIAnimationDelegates.GetIsPlaying(cat, "Visible") || UIAnimationDelegates.GetIsPlaying(cat, "TweenIn")) Hide(cat, instant);
        }

        
        
    }
}
