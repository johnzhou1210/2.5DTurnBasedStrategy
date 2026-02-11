using System;
using DG.Tweening;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid;
using StrategyGame.UI.World;
using UnityEngine;

namespace StrategyGame.Combat.Cinematics {
    public enum AttackType
    {
        Melee = 0,
        Ranged = 1
    }
    
    public class CombatPuppet : MonoBehaviour {

      
        public Animator Animator;
        [SerializeField] private GameObject damageIndicatorPrefab;
        [SerializeField] private Transform damageIndicatorSpawnPoint;
        [SerializeField] private Transform billboardCanvasTransform;
        [SerializeField] private Transform projectileSpawnPointTransform;
        [SerializeField] private Transform vfxTransform;

        public Transform ProjectileSpawnPointTransform { get => projectileSpawnPointTransform; }
        public Transform VFXTransform { get => vfxTransform; }
        
        public CombatDirector CombatDirector { get; private set; }

        public void Setup(CombatDirector combatDirector, GridEntity entity) {
            CombatDirector = combatDirector;
            // Set animator controller
            Animator.runtimeAnimatorController = entity.AnimatorController;
            Animator.Play("Idle");
        }

        public void SpawnDamageNumber(int damage, bool isCrit) {
            GameObject damageIndicator = Instantiate(damageIndicatorPrefab, billboardCanvasTransform);
            damageIndicator.transform.position = damageIndicatorSpawnPoint.position;
            DamageIndicatorBillboard damageIndicatorComponent = damageIndicator.GetComponent<DamageIndicatorBillboard>();
            damageIndicatorComponent.Setup(damage, isCrit);
        }
        
        public void SpawnImpactVFX(GameObject VFXPrefab, Vector3 position, bool hit) {
            CombatDirector.SpawnImpactVFX(VFXPrefab, vfxTransform, position, hit);
        }

      
        
    }
}
