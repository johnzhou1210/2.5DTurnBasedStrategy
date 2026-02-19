using TMPro;
using UnityEngine;

namespace StrategyGame.UI.Menus {
    public class SkillOrItemToolTipRenderer : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI subDescription;
        [SerializeField] private SkillDescriptionTicker skillDescriptionTicker;

        public void SetDescription(string text) {
            description.SetText(text);
            skillDescriptionTicker.Refresh();
        }
        public void SetSubDescription(string text) {
            subDescription.SetText(text);
        }
    }
}
