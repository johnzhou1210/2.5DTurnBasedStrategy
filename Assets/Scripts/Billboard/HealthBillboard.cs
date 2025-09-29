using System;
using System.ComponentModel;
using DG.Tweening;
using Grid;
using UnityEngine;
using UnityEngine.UI;

public class HealthBillboard : MonoBehaviour {
    private int _targetId;
    [SerializeField] private Slider slider;
    [SerializeField] private Image weaponTypeImage, weaponTypeFrame;
    
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
        weaponTypeFrame.color = GetColorFromWeaponType(entity.GridEntityData.WeaponType);
        weaponTypeImage.sprite = GetSpriteFromWeaponType(entity.GridEntityData.WeaponType);
    }

    private Color GetColorFromWeaponType(WeaponType weaponType) {
        switch (weaponType) {
            case WeaponType.SWORD:
                return Color.blue;
            case WeaponType.SPEAR:
                return Color.green;
            case WeaponType.AXE:
                return Color.red;
            case WeaponType.BOW:
                return new Color(1, .5f, 0);
            case WeaponType.STAFF:
                return new Color(.5f,0,1);
            default:
                throw new InvalidEnumArgumentException("Invalid WeaponType enum!");
        }
    }

    private Sprite GetSpriteFromWeaponType(WeaponType weaponType) {
        switch (weaponType) {
            case WeaponType.SWORD:
                return Resources.Load<Sprite>("WeaponTypeIcons/sword");
            case WeaponType.SPEAR:
                return Resources.Load<Sprite>("WeaponTypeIcons/spear");
            case WeaponType.AXE:
                return Resources.Load<Sprite>("WeaponTypeIcons/axe");
            case WeaponType.BOW:
                return Resources.Load<Sprite>("WeaponTypeIcons/bow");
            case WeaponType.STAFF:
                return Resources.Load<Sprite>("WeaponTypeIcons/staff");
            default:
                throw new InvalidEnumArgumentException("Invalid WeaponType enum!");
        }
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
