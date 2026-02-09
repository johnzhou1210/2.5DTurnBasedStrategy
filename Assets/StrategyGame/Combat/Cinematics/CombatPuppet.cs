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
        private static readonly int Type = Animator.StringToHash("AttackType");
        private static readonly int IsCrit = Animator.StringToHash("IsCrit");
        private static readonly int AttackIndex = Animator.StringToHash("AttackIndex");
        private static readonly int Attack = Animator.StringToHash("Attack");
        public Animator Animator;
        
        public void Setup(GridEntity entity) {
            // Set animator controller
            Animator.runtimeAnimatorController = entity.AnimatorController;
        }

        public void PlayDodge() {
            
        }

        public void PlayDeath() {
            
        }

        public void PlayHit(bool crit) {
            
        }

        

        public void PlayAttack(AttackType type, bool isCrit, int attackIndex)
        {
            Animator.SetInteger(Type, (int)type);
            Animator.SetBool(IsCrit, isCrit);
            Animator.SetInteger(AttackIndex, attackIndex);
            Animator.SetTrigger(Attack);
        }

      
        
    }
}
