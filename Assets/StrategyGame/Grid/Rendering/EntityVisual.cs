using System;
using System.Collections;
using StrategyGame.Core.Delegates;
using UnityEngine;

namespace StrategyGame.Grid.Rendering {
    public class EntityVisual : MonoBehaviour {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        [SerializeField] private Renderer ringRenderer;
        [SerializeField] private Material defaultSpriteMaterial;

        private SpriteRenderer _spriteRenderer;

        private Coroutine _fadeCoroutine;

        private void Start() {
            ringRenderer.material.EnableKeyword("_EMISSION");
        }

        public void SetColor(Color c) {
            ringRenderer.material.color = c;
            ringRenderer.material.SetColor(EmissionColor, c * 5f);
        }
        
        private void OnDisable() {
            if (_fadeCoroutine != null) {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }

        public void Die() {
            // Temporary fade coroutine
            if (_fadeCoroutine != null) {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
            _fadeCoroutine = StartCoroutine(FadeCoroutine());
        }

        public void SetSpriteFlipX(bool flipX) {
            _spriteRenderer.flipX = flipX;
        }

        public void SetEntitySprite(SpriteRenderer spriteRenderer) {
            _spriteRenderer = spriteRenderer;
        }

        private IEnumerator FadeCoroutine() {
            if (_spriteRenderer == null) {
                Debug.LogWarning("EntityVisual.FadeCoroutine: _spriteRenderer is null. Maybe SetEntitySprite wasn't called on this script?");
                yield break;
            }
            _spriteRenderer.material =  defaultSpriteMaterial;
            float startAlpha = _spriteRenderer.color.a;
            int steps = 200;

            for (int i = 0; i < steps; i++) {
                float alpha = Mathf.Lerp(startAlpha, 0f, (i + 1f) / steps);
                _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, alpha);
                yield return new WaitForEndOfFrame();
            }
            // gameObject.SetActive(false);
            ringRenderer.enabled = false;
        }

        
    }
}
