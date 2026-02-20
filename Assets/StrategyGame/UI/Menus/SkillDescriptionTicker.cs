using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace StrategyGame.UI.Menus {
    public class SkillDescriptionTicker : MonoBehaviour {
        [SerializeField] private float speed = 50f;
        [SerializeField] private float pauseDuration = 1.5f;
        [SerializeField] private TextMeshProUGUI descriptionText;
        private RectTransform _rectTransform;
        private RectTransform _parentRect;
        private float _textWidth;
        private float _scrollDistance;
        private float _timer;
        private bool _shouldScroll;
        private bool _isScrolling;
        private Vector2 _originalPosition;
        private void Awake() {
            _rectTransform = GetComponent<RectTransform>();
            _parentRect = _rectTransform.parent.GetComponent<RectTransform>();
            _originalPosition = _rectTransform.anchoredPosition;
        }
        public void Refresh()
        {
            // 1. Force TMP to calculate the size of the text string without wrapping
            // We pass float.PositiveInfinity to say "don't wrap, just tell me how long this is"
            Vector2 textSize = descriptionText.GetPreferredValues(descriptionText.text, float.PositiveInfinity, float.PositiveInfinity);
            float preferredWidth = textSize.x + 512f;

            // 2. Set the RectTransform width to match the full text length
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
    
            // 3. Reset to original position (which should be 0 if pivoted left)
            _rectTransform.anchoredPosition = _originalPosition;
    
            // 4. Calculate how much extra text exists beyond the parent's view
            _scrollDistance = preferredWidth - _parentRect.rect.width;
    
            // Safety check: if text is shorter than container, don't scroll
            _shouldScroll = _scrollDistance > 0;
            _isScrolling = false;
            _timer = 0f;
        }

        private void Update() {
            if (!_shouldScroll) return;

            if (!_isScrolling) {
                _timer += Time.deltaTime;
                if (_timer >= pauseDuration) {
                    _isScrolling = true;
                }
                return;
            }

            // Move left based on speed
            float currentX = _rectTransform.anchoredPosition.x;
            float targetX = _originalPosition.x - _scrollDistance;

            if (currentX > targetX) {
                _rectTransform.anchoredPosition += Vector2.left * (speed * Time.deltaTime);
            } else {
                // Reached the end
                _rectTransform.anchoredPosition = new Vector2(targetX, _originalPosition.y);
                _isScrolling = false;
                _shouldScroll = false; // "Lock" the update
                StartCoroutine(ResetAfterPause());
            }
        }

        private IEnumerator ResetAfterPause() {
            yield return new WaitForSeconds(pauseDuration);
            _rectTransform.anchoredPosition = _originalPosition;
            _timer = 0f;
            _shouldScroll = true; // "Unlock" the update
        }
    }
}
