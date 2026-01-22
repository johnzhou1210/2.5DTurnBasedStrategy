using System;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Core.GameState;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhaseBannerRenderer : MonoBehaviour {
   [SerializeField] private Animator animator;
   [SerializeField] private TextMeshProUGUI bannerText;
   [SerializeField] private Image topGradient;
   [SerializeField] private Image bottomGradient;

   private void OnEnable() {
      UIDelegates.OnPlayPhaseBannerAnimationSequence += PlayPhaseBannerAnimationSequence;
   }

   private void OnDisable() {
      UIDelegates.OnPlayPhaseBannerAnimationSequence -= PlayPhaseBannerAnimationSequence;
   }

   private void PlayPhaseBannerAnimationSequence() {
      // Update UI text and color
      GameStateData currentState = GameStateDelegates.GetCurrentGameState();
      if (currentState.Combat.TurnPhase is GameStateEnums.TurnPhase.Event or GameStateEnums.TurnPhase.None) return;
      bannerText.SetText($"{(currentState.Combat.TurnPhase == GameStateEnums.TurnPhase.Player ? "PLAYER" : "ENEMY")} PHASE");
      topGradient.color = currentState.Combat.TurnPhase == GameStateEnums.TurnPhase.Player ? Color.blue : Color.red;
      bottomGradient.color = currentState.Combat.TurnPhase == GameStateEnums.TurnPhase.Player ? Color.blue : Color.red;
      animator.Play("FadeIn");
   }

   public void PlayVisible() {
      animator.Play("Visible");
   }

   public void PlayFadeOut() {
      animator.Play("FadeOut");
   }
   
   public void PlayInvisible() {
      animator.Play("Invisible");
   }
   
}
