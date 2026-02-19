using System;
using TMPro;
using UnityEngine;

namespace StrategyGame.UI.Menus {
    public class SkillDescriptionTicker : MonoBehaviour {
        [SerializeField] private float speed = 50f;
        [SerializeField] private float delayBeforeScroll = 1f;
        [SerializeField] private int maxChars = 42;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private RectTransform _rectTransform;
        private RectTransform _parentRect;
        private float _textWidth;
        
        private float _timer;
        private bool _shouldScroll;

        private Vector2 _originalPosition;

        private void Awake() {
            _rectTransform = GetComponent<RectTransform>();
            _parentRect = _rectTransform.parent.GetComponent<RectTransform>();
            _originalPosition = _rectTransform.anchoredPosition;
        }

        public void Refresh() {
            _rectTransform.anchoredPosition = _originalPosition;
            _timer = 0f;
            _textWidth = _rectTransform.rect.width;
            _shouldScroll = descriptionText.text.Length > maxChars;
        }

        private void Update() {
            if (!_shouldScroll) return;
            _timer += Time.deltaTime;
            if (_timer < delayBeforeScroll) return;
            _rectTransform.anchoredPosition += Vector2.left * (speed * Time.deltaTime);
            if (Mathf.Abs(_rectTransform.anchoredPosition.x) > _textWidth) {
                _rectTransform.anchoredPosition = _originalPosition;
                _timer = 0f;
            }
        }

        private void Start() {
            Refresh();
        }


    }
}
