using System;
using System.Collections;
using StrategyGame.Grid;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

namespace StrategyGame.Combat.Cinematics {
    public class CombatDirector : MonoBehaviour {
        /* Timelines needed:
         * Intro: Sets the scene for combat and combatants are visible
         * AttackerSwordNormal
         * AttackerSwordCrit
         * AttackerSpearNormal
         * 
         * Normal Bow Attack
         * Crit Attack
         * Hit Reaction
         * Counter Attack
         * Death Reaction
         * 
         * 
         */
        
        
        [SerializeField] private GameObject combatPuppetPrefab;
        [SerializeField] private PlayableDirector director;
        [SerializeField] private CinemachineCamera combatCam;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private CombatPuppet testPuppet;
        
        private CombatPuppet _attackerPuppet;
        private CombatPuppet _defenderPuppet;

        private GridEntity _attackerEntity;
        private GridEntity _defenderEntity;

        private CombatOutcome _combatOutcome;

        
        public IEnumerator PlayCombat(GridEntity attacker, GridEntity defender, CombatOutcome combatOutcome) {
            _attackerEntity = attacker;
            _defenderEntity = defender;
            _combatOutcome = combatOutcome;
            EnterCinematicMode();
            SpawnPuppets();
            BindTimeline();
            director.Play();
            yield return new WaitUntil((() => director.state != PlayState.Playing));
            CleanupPuppets();
            ExitCinematicMode();
        }

        private void EnterCinematicMode() {
            // Lock user grid input
            // Override camera
        }

        private void SpawnPuppets() {
            _attackerPuppet = Instantiate(combatPuppetPrefab).GetComponent<CombatPuppet>();
            _defenderPuppet = Instantiate(combatPuppetPrefab).GetComponent<CombatPuppet>();
            _attackerPuppet.Setup(_attackerEntity);
            _defenderPuppet.Setup(_defenderEntity);
            _attackerPuppet.transform.position = GetAttackPosition();
            _defenderPuppet.transform.position = GetDefendPosition();
        }
        private void BindTimeline() {
            // director.SetGenericBinding(attackerTrack, _attackerPuppet.Animator);
            // director.SetGenericBinding(defenderTrack, _defenderPuppet.Animator);
        }
        private void CleanupPuppets() {
            Destroy(_attackerPuppet.gameObject);
            Destroy(_defenderPuppet.gameObject);
        }
        private void ExitCinematicMode() {
            // Unlock user grid input
            // Undo camera override
        }

        /* Methods called via animation events */
        public void OnAttackImpact(int attackIndex) {
            bool hit = _combatOutcome.AttackHits[attackIndex];
            bool crit = _combatOutcome.AttackHitCrits[attackIndex];
            int impactDamage = _combatOutcome.AttackDamageInstances[attackIndex];
            if (!hit) {
                _defenderPuppet.PlayDodge();
                // Any miss SFX/VFX
                return;
            }
            _defenderPuppet.PlayHit(crit);
            SpawnDamageNumber(impactDamage, crit);
            if (_combatOutcome.DefenderDied) {
                _defenderPuppet.PlayDeath();
            }
        }
        public void OnCounterImpact(int counterIndex) {
            bool hit = _combatOutcome.DefendCounterHits[counterIndex];
            bool crit  = _combatOutcome.DefendCounterCrits[counterIndex];
            int counterDamage = _combatOutcome.CounterDamageInstances[counterIndex];
            if (!hit) {
                _attackerPuppet.PlayDodge();
                return;
            }
            _attackerPuppet.PlayHit(crit);
            SpawnDamageNumber(counterDamage, crit);
        }
        // ===================================== //
        
        private Vector3 GetAttackPosition() {
            return Vector3.zero;
        }
        private Vector3 GetDefendPosition() {
            return Vector3.zero;
        }
        
        private void SpawnDamageNumber(int damage, bool crit) {
            
        }

        private void Update() {
            if (playerInput.actions["Test1"].WasPressedThisFrame()) {
                Debug.Log("Testing puppet");
                testPuppet.PlayAttack(AttackType.Melee, false, 0); // Melee1
            }
            if (playerInput.actions["Test2"].WasPressedThisFrame()) {
                Debug.Log("Testing puppet");
                testPuppet.PlayAttack(AttackType.Melee, false, 1); // Melee2
            }
            if (playerInput.actions["Test3"].WasPressedThisFrame()) {
                Debug.Log("Testing puppet");
                testPuppet.PlayAttack(AttackType.Ranged, false, 1); // Attack3
            }
            if (playerInput.actions["Test4"].WasPressedThisFrame()) {
                Debug.Log("Testing puppet");
                testPuppet.PlayAttack(AttackType.Melee, false, 1);
            }
            if (playerInput.actions["Test5"].WasPressedThisFrame()) {
                Debug.Log("Testing puppet");
                testPuppet.PlayAttack(AttackType.Melee, false, 1);
            }
            if (playerInput.actions["Test6"].WasPressedThisFrame()) {
                Debug.Log("Testing puppet");
                testPuppet.PlayAttack(AttackType.Melee, false, 1);
            }
            if (playerInput.actions["Test7"].WasPressedThisFrame()) {
                Debug.Log("Testing puppet");
                testPuppet.PlayAttack(AttackType.Melee, false, 1);
            }
            if (playerInput.actions["Test8"].WasPressedThisFrame()) {
                Debug.Log("Testing puppet");
                testPuppet.PlayAttack(AttackType.Melee, false, 1);
            }
            if (playerInput.actions["Test9"].WasPressedThisFrame()) {
                Debug.Log("Testing puppet");
                testPuppet.PlayAttack(AttackType.Melee, false, 1);
            }
            if (playerInput.actions["Test10"].WasPressedThisFrame()) {
                Debug.Log("Testing puppet");
                testPuppet.PlayAttack(AttackType.Melee, false, 1);
            }
            if (playerInput.actions["Test11"].WasPressedThisFrame()) {
                Debug.Log("Testing puppet");
                testPuppet.PlayAttack(AttackType.Melee, false, 1);
            }
            if (playerInput.actions["Test12"].WasPressedThisFrame()) {
                Debug.Log("Testing puppet");
                testPuppet.PlayAttack(AttackType.Melee, false, 1);
            }
        }


    }
}
