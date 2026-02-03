using System;
using System.Collections;
using System.ComponentModel;
using DG.Tweening;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid;
using StrategyGame.Grid.GridData;
using StrategyGame.Grid.Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace StrategyGame.UI.World {
    public class HealthBillboard : MonoBehaviour {
        private int _targetID;
        [SerializeField] private Slider slider;
        [SerializeField] private Image weaponTypeImage, weaponTypeFrame;
        [SerializeField] private float healthTransitionDuration = 1f;

        private Coroutine _deathCoroutine;
        private Coroutine _fadeCoroutine;
        private CanvasGroup _canvasGroup;

        private void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable() {
            BillboardDelegates.OnHealthChanged += UpdateHealth;
        }

        private void OnDisable() {
            BillboardDelegates.OnHealthChanged -= UpdateHealth;
            if (_deathCoroutine != null) {
                StopCoroutine(_deathCoroutine);
                _deathCoroutine = null;
            }
            if (_fadeCoroutine != null) {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }

        public void Initialize(GridEntity entity) {
            _targetID = entity.ID;
            if (TryGetComponent(out BillboardFollow billboardFollow)) {
                Transform targetTransform = EntityVisualDelegates.GetEntityVisualTransformByID(entity.ID);
                billboardFollow.SetTarget(targetTransform);
            }
            UpdateHealth(entity.ID, entity.Health, entity.MaxHealth);
        }

       

        private void UpdateHealth(int id, int health, int maxHealth) {
            if (_targetID != id) {
                // Debug.Log($"Expected id of {_targetID}, but got {id}");
                return;
            }
            Debug.Log($"Updating health {id} to {health} / {maxHealth}");
            float target = (float)health / maxHealth;
            DOTween.To(() => slider.value, x => slider.value = x, target, healthTransitionDuration);
            if (health == 0) {
                if (_deathCoroutine != null) {
                    Debug.LogWarning("HealthBillboard.UpdateHealth: Aborting extra death coroutine because one already exists.");
                    return;
                }
                _deathCoroutine = StartCoroutine(DeathCoroutine());
            }
        }

        private IEnumerator DeathCoroutine() {
            yield return new WaitForSeconds(healthTransitionDuration);
            EntityVisual targetVisual = EntityVisualDelegates.GetEntityVisualTransformByID(_targetID).GetComponent<EntityVisual>();
            if (targetVisual == null) {
                throw new Exception("HealthBillboard.DeathCoroutine: targetVisual not found!");
            }
            _fadeCoroutine = StartCoroutine(FadeCoroutine());
            EntityVisualDelegates.InvokeOnFadeEntityVisuals(_targetID);
            targetVisual.Die();
            _deathCoroutine = null;
        }
        
        private IEnumerator FadeCoroutine() {
            float startAlpha = _canvasGroup.alpha;
            int steps = 200;
            for (int i = 0; i < steps; i++) {
                float alpha = Mathf.Lerp(startAlpha, 0f, (i + 1f) / steps);
                _canvasGroup.alpha = alpha;
                yield return new WaitForEndOfFrame();
            }
            // gameObject.SetActive(false);
            _fadeCoroutine = null;
        }
    }
}
