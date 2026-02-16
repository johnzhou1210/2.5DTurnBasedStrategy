using System;
using System.Collections;
using StrategyGame.Core.Delegates;
using UnityEngine;

namespace StrategyGame.Grid.Rendering {
    public class EntityVisual : MonoBehaviour {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        [SerializeField] private Renderer ringRenderer;
        [SerializeField] private Material defaultSpriteMaterial;

        [SerializeField] private SpriteRenderer entitySpriteRenderer;
        [field: SerializeField] public Animator Animator { get; private set; }

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
            entitySpriteRenderer.flipX = flipX;
        }

        private IEnumerator FadeCoroutine() {
            if (entitySpriteRenderer == null) {
                Debug.LogWarning("EntityVisual.FadeCoroutine: entitySpriteRenderer is null. Maybe SetEntitySprite wasn't called on this script?");
                yield break;
            }
            entitySpriteRenderer.material =  defaultSpriteMaterial;
            float startAlpha = entitySpriteRenderer.color.a;
            int steps = 200;

            for (int i = 0; i < steps; i++) {
                float alpha = Mathf.Lerp(startAlpha, 0f, (i + 1f) / steps);
                entitySpriteRenderer.color = new Color(entitySpriteRenderer.color.r, entitySpriteRenderer.color.g, entitySpriteRenderer.color.b, alpha);
                yield return new WaitForEndOfFrame();
            }
            // gameObject.SetActive(false);
            ringRenderer.enabled = false;
        }

        
    }
}
