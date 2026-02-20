using System;
using TMPro;
using UnityEngine;

namespace StrategyGame.UI.Menus {
    public class SkillOrItemToolTipRenderer : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI subDescription;
        [SerializeField] private SkillDescriptionTicker skillDescriptionTicker;

        private RectTransform _rectTransform;

        private void Awake() {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void SetDescription(string text) {
            description.SetText(text);
            skillDescriptionTicker.Refresh();
        }
        public void SetSubDescription(string text) {
            subDescription.SetText(text);
        }

        public void SetAnchoredPositionY(float newY) {
            _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, newY);   
        }
    }
}
