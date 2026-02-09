using StrategyGame.Grid;
using UnityEditor.Animations;
using UnityEngine;

namespace StrategyGame.Combat.Cinematics {
    public enum AttackType
    {
        Melee = 0,
        Ranged = 1
    }
    
    public class CombatPuppet : MonoBehaviour {

      
        public Animator Animator;
        
        public void Setup(GridEntity entity) {
            // Set animator controller
            Animator.runtimeAnimatorController = entity.AnimatorController;
            Animator.Play("Idle");
        }

        public void PlayDodge() {
            
        }

        public void PlayDeath() {
            
        }

        public void PlayHit(bool crit) {
            
        }
        

      
        
    }
}
