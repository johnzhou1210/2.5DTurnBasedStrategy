using System.ComponentModel;
using DG.Tweening;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid;
using StrategyGame.Grid.GridData;
using UnityEngine;
using UnityEngine.UI;

namespace StrategyGame.UI.World {
    public class HealthBillboard : MonoBehaviour {
        private int _targetID;
        [SerializeField] private Slider slider;
        [SerializeField] private Image weaponTypeImage, weaponTypeFrame;
    
        private void OnEnable() {
            BillboardDelegates.OnHealthChanged += UpdateHealth;
        }

        private void OnDisable() {
            BillboardDelegates.OnHealthChanged -= UpdateHealth;
        }

        public void Initialize(GridEntity entity) {
            _targetID = entity.ID;
            if (TryGetComponent(out BillboardFollow billboardFollow)) {
                Transform targetTransform = GridDelegates.GetEntityVisualTransformByID(entity.ID);
                billboardFollow.SetTarget(targetTransform);
            }
            UpdateHealth(entity.ID, entity.Health, entity.MaxHealth);
        }

       

        private void UpdateHealth(int id, int health, int maxHealth) {
            if (_targetID != id) {
                Debug.Log($"Expected id of {_targetID}, but got {id}");
                return;
            }
            Debug.Log($"Updating health {id} to {health} / {maxHealth}");
            float target = (float)health / maxHealth;
            DOTween.To(() => slider.value, x => slider.value = x, target, 1f);
        }
    }
}
