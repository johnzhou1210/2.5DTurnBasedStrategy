using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StrategyGame.UI.Menus {
    public class SkillOrItemEntryRenderer : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private GameObject cooldownInfo;
        [SerializeField] private Slider cooldownSlider;
        [SerializeField] private CanvasGroup cooldownInfoCanvasGroup;
        
        public void SetHeaderText(string text) {
            headerText.SetText(text);
        }

        public void SetCooldownInfo(int turnsLeft, int maxTurns) {
            if (maxTurns < 0) {
                cooldownInfoCanvasGroup.alpha = 0;
            }
            cooldownInfoCanvasGroup.alpha = turnsLeft > 0 ? 1f : 0f;
            cooldownSlider.value = (float)turnsLeft / maxTurns;
            
        }
    }
}
