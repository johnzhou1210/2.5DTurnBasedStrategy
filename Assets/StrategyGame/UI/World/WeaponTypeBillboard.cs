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
    
        private void OnEnable() {
            BillboardDelegates.OnUnitWeaponTypeChanged += UpdateUnitWeaponType;
        }

        private void OnDisable() {
            BillboardDelegates.OnUnitWeaponTypeChanged -= UpdateUnitWeaponType;
        }

        public void Initialize(GridUnit unit) {
            _targetID = unit.ID;
            if (TryGetComponent(out BillboardFollow billboardFollow)) {
                Transform targetTransform = GridDelegates.GetEntityVisualTransformByID(unit.ID);
                billboardFollow.SetTarget(targetTransform);
            }
            weaponTypeFrame.color = GetColorFromWeaponType(unit.GridUnitData.WeaponType);
            weaponTypeImage.sprite = GetSpriteFromWeaponType(unit.GridUnitData.WeaponType);
        }

        private void UpdateUnitWeaponType(GridUnit unit) {
            if (_targetID != unit.ID) return;
            weaponTypeFrame.color = GetColorFromWeaponType(unit.GridUnitData.WeaponType);
            weaponTypeImage.sprite = GetSpriteFromWeaponType(unit.GridUnitData.WeaponType);
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
    }
}
