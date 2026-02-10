using System;
using DG.Tweening;
using StrategyGame.Core.Delegates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StrategyGame.UI.HUD {
   public class BattleCinematicHUDRenderer : MonoBehaviour {
      [SerializeField] private CanvasGroup canvasGroup;
      [SerializeField] private Slider attackerFillSlider;
      [SerializeField] private Slider attackerLossSlider;
      [SerializeField] private Slider defenderFillSlider;
      [SerializeField] private Slider defenderLossSlider;
      [SerializeField] private Image attackerLossFill;
      [SerializeField] private Image defenderLossFill;
      [SerializeField] private float healthTransitionDuration = 1f;
      [SerializeField] private TextMeshProUGUI attackerHPText;
      [SerializeField] private TextMeshProUGUI defenderHPText;
      [SerializeField] private TextMeshProUGUI attackerNameText;
      [SerializeField] private TextMeshProUGUI defenderNameText;

      private void OnEnable() {
         UIDelegates.OnCombatCinematicHUDUpdate += UpdateUI;
      }

      private void OnDisable() {
         UIDelegates.OnCombatCinematicHUDUpdate -= UpdateUI;
      }
      

      private void UpdateUI(bool isAttacker, int health, int maxHealth, int oldHealth, string displayName) {
         // Name update
         TextMeshProUGUI targetNameText = isAttacker ? attackerNameText : defenderNameText;
         targetNameText.SetText(displayName);
         
         // Health update
         Slider targetFillSlider = isAttacker ? attackerFillSlider : defenderFillSlider;
         Slider targetLossSlider = isAttacker ? attackerLossSlider : defenderLossSlider;
         TextMeshProUGUI targetHPText = isAttacker ? attackerHPText : defenderHPText;
         Image targetLossFill = isAttacker ? attackerLossFill : defenderLossFill;
         float targetHealth = (float)health / maxHealth;
         // targetLossSlider.value = (float)oldHealth / maxHealth;
         // targetLossFill.DOFade(0f, 1f);
         // DOTween.To(() => targetFillSlider.value, x => targetFillSlider.value = x, targetHealth, healthTransitionDuration);
         targetFillSlider.value = targetHealth;
         targetLossSlider.value = (float)oldHealth / maxHealth;
         DOTween.To(()=> targetLossSlider.value, x => targetLossSlider.value = x, targetHealth, healthTransitionDuration);
         targetHPText.color = health != 0 ? Color.white : Color.red;
         targetHPText.SetText(health.ToString());
      }

   }
}
