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
        
        public void Setup(GridEntity entity) {
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
        

      
        
    }
}
