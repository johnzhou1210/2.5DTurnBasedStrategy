using System;
using System.Collections;
using System.ComponentModel;
using DG.Tweening;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid;
using StrategyGame.Grid.GridData;
using UnityEngine;
using UnityEngine.UI;

namespace StrategyGame.UI.World {
    public class WeaponTypeBillboard : MonoBehaviour {
        private int _targetID;
        [SerializeField] private Image weaponTypeImage, weaponTypeFrame;
    
        private CanvasGroup _canvasGroup;
        private Coroutine _fadeCoroutine;

        private void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnDestroy() {
            if (_fadeCoroutine != null) {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }

        private void OnEnable() {
            BillboardDelegates.OnUnitWeaponTypeChanged += UpdateUnitWeaponType;
            EntityVisualDelegates.OnFadeEntityVisuals += FadeBillboard;
        }

        private void OnDisable() {
            BillboardDelegates.OnUnitWeaponTypeChanged -= UpdateUnitWeaponType;
            EntityVisualDelegates.OnFadeEntityVisuals -= FadeBillboard;
        }

        public void Initialize(GridUnit unit) {
            _targetID = unit.ID;
            if (TryGetComponent(out BillboardFollow billboardFollow)) {
                Transform targetTransform = EntityVisualDelegates.GetEntityVisualTransformByID(unit.ID);
                billboardFollow.SetTarget(targetTransform);
            }
            if (unit.GridUnitData.Weapon == null) {
                Debug.LogWarning("WeaponTypeBillboard.Initialize: Unit does not have weapon, hiding weapon frame icon.");
                weaponTypeFrame.enabled = false;
                weaponTypeImage.enabled = false;
                return;
            }
            weaponTypeFrame.color = GetColorFromWeaponType(unit.GridUnitData.Weapon.WeaponType);
            weaponTypeImage.sprite = GetSpriteFromWeaponType(unit.GridUnitData.Weapon.WeaponType);
        }

        private void UpdateUnitWeaponType(GridUnit unit) {
            if (_targetID != unit.ID) return;
            weaponTypeFrame.color = GetColorFromWeaponType(unit.GridUnitData.Weapon.WeaponType);
            weaponTypeImage.sprite = GetSpriteFromWeaponType(unit.GridUnitData.Weapon.WeaponType);
        }

        private Color GetColorFromWeaponType(WeaponType weaponType) {
            switch (weaponType) {
                case WeaponType.Sword:
                    return Color.blue;
                case WeaponType.Spear:
                    return Color.green;
                case WeaponType.Axe:
                    return Color.red;
                case WeaponType.Bow:
                    return new Color(1, .75f, 0);
                case WeaponType.Staff:
                    return new Color(.5f,0,1);
                default:
                    throw new InvalidEnumArgumentException("Invalid WeaponType enum!");
            }
        }

        private Sprite GetSpriteFromWeaponType(WeaponType weaponType) {
            switch (weaponType) {
                case WeaponType.Sword:
                    return Resources.Load<Sprite>("WeaponTypeIcons/sword");
                case WeaponType.Spear:
                    return Resources.Load<Sprite>("WeaponTypeIcons/spear");
                case WeaponType.Axe:
                    return Resources.Load<Sprite>("WeaponTypeIcons/axe");
                case WeaponType.Bow:
                    return Resources.Load<Sprite>("WeaponTypeIcons/bow");
                case WeaponType.Staff:
                    return Resources.Load<Sprite>("WeaponTypeIcons/staff");
                default:
                    throw new InvalidEnumArgumentException("Invalid WeaponType enum!");
            }
        }
        private void FadeBillboard() {
            _fadeCoroutine = StartCoroutine(FadeCoroutine());
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
