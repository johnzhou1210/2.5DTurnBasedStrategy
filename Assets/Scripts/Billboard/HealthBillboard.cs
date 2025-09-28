using System;
using DG.Tweening;
using Grid;
using UnityEngine;
using UnityEngine.UI;

public class HealthBillboard : MonoBehaviour {
    private int _targetId;
    [SerializeField] private Slider slider;
    
    private void OnEnable() {
        BillboardDelegates.OnHealthChanged += UpdateHealth;
    }

    private void OnDisable() {
        BillboardDelegates.OnHealthChanged -= UpdateHealth;
    }

    public void Initialize(GridEntity entity) {
        _targetId = entity.Id;
        if (TryGetComponent(out BillboardFollow billboardFollow)) {
            Transform targetTransform = GridDelegates.GetEntityVisualTransformById(entity.Id);
            billboardFollow.SetTarget(targetTransform);
        }
        UpdateHealth(entity.Id, entity.Health, entity.MaxHealth);
    }

    private void UpdateHealth(int id, int health, int maxHealth) {
        if (_targetId != id) {
            Debug.Log($"Expected id of {_targetId}, but got {id}");
            return;
        }
        Debug.Log($"Updating health {id} to {health} / {maxHealth}");
        float target = (float)health / maxHealth;
        DOTween.To(() => slider.value, x => slider.value = x, target, 1f);
    }
}
