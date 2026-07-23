using System;
using StrategyGame.Core.Delegates;
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
      GameStateData.GameStateDatagram currentState = GameStateDelegates.GetCurrentGameState();
      if (currentState.Combat.TurnPhase is CombatStateEnums.TurnPhase.Event or CombatStateEnums.TurnPhase.None) return;
      bannerText.SetText($"{(currentState.Combat.TurnPhase == CombatStateEnums.TurnPhase.Player ? "PLAYER" : "ENEMY")} PHASE");
      topGradient.color = currentState.Combat.TurnPhase == CombatStateEnums.TurnPhase.Player ? Color.blue : Color.red;
      bottomGradient.color = currentState.Combat.TurnPhase == CombatStateEnums.TurnPhase.Player ? Color.blue : Color.red;
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
