using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Core.GameState;
using StrategyGame.Grid;
using StrategyGame.UI;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace StrategyGame.Combat.Cinematics {
    public class CombatDirector : MonoBehaviour {
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int AttackType = Animator.StringToHash("AttackType");
        private static readonly int Dodge = Animator.StringToHash("Dodge");
        private static readonly int Hurt = Animator.StringToHash("Hurt");
        private static readonly int Death = Animator.StringToHash("Death");
        /* Timelines needed:
         * CombatIntro: Sets the scene for combat and combatants are visible
         * CombatOutro
         * MeleeNormal
         * MeleeCrit
         * RangedNormal
         * RangedCrit
         * 
         * 
         */
        
        [SerializeField] private GameObject combatPuppetPrefab;
        [SerializeField] private PlayableDirector director;
        [SerializeField] private CinemachineCamera combatCam;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private CombatPuppet testPuppet;
        [SerializeField] private Transform attackerAnimatedRig;
        [SerializeField] private Transform defenderAnimatedRig;
        [SerializeField] private Transform defenderRigAdapter;
        
        [SerializeField] private CombatPuppet attackerPuppet;
        [SerializeField] private CombatPuppet defenderPuppet;
        
        [Header("Timeline Playable Assets")]
        [SerializeField] private PlayableAsset combatIntroPlayable;
        [SerializeField] private PlayableAsset combatOutroPlayable;
        [SerializeField] private PlayableAsset meleeNormalPlayable;
        [SerializeField] private PlayableAsset meleeCritPlayable;
        [SerializeField] private PlayableAsset rangedNormalPlayable;
        [SerializeField] private PlayableAsset rangedCritPlayable;

        private GridEntity _attackerEntity;
        private GridEntity _defenderEntity;
        private CombatOutcome _combatOutcome;
        private Camera _camera;
        private CombatTimeline _currCombatEvent;
        private bool _isAttackerTurn = true;
        private int _currAttackIndex = 0;
        private int _currCounterIndex = 0;
        private int _currAttackerHP;
        private int _currDefenderHP;


        public enum CombatTimeline {
            AttackerMeleeNormal,
            AttackerMeleeCrit,
            AttackerRangedNormal,
            AttackerRangedCrit,
            DefenderMeleeNormal,
            DefenderMeleeCrit,
            DefenderRangedNormal,
            DefenderRangedCrit,
            // Not actual timelines, but signals
            AttackerDies, // for if anyone dies
            DefenderDies
        }

        private void Start() {
            _camera = Camera.main;
        }

        private void OnEnable() {
            CombatCinematicsDelegates.GetDirector = () => this;
        }

        private void OnDisable() {
            CombatCinematicsDelegates.GetDirector = null;
        }

        public void InitializeCinematicData(GridEntity attacker, GridEntity defender, CombatOutcome combatOutcome) {
            _attackerEntity = attacker;
            _defenderEntity = defender;
            _combatOutcome = combatOutcome;
        }


        public IEnumerator PlayCombat() {
            void DirectorLoadAndPlay(PlayableAsset playableAsset) {
                director.playableAsset = playableAsset;
                director.Play();
            }
            
            EnterCinematicMode();
            SpawnPuppets();
            
           

            Debug.Log($"Order of events: {string.Join(",", _combatOutcome.OrderOfEvents)}");
            
            BillboardDelegates.InvokeOnSetLookatTargetTransform(combatCam.transform);
            
            // Director play intro cutscene
            defenderRigAdapter.localPosition = new Vector3(1.5f, 0, 0);
            int currEventIndex = 0;
            _currAttackIndex = 0;
            _currCounterIndex = 0;
            _currAttackerHP = _attackerEntity.Health;
            _currDefenderHP = _defenderEntity.Health;
            
            UIDelegates.InvokeOnCombatCinematicHUDUpdate(true, _currAttackerHP, _attackerEntity.MaxHealth, _currAttackerHP, _attackerEntity.DisplayName);
            UIDelegates.InvokeOnCombatCinematicHUDUpdate(false, _currDefenderHP, _defenderEntity.MaxHealth, _currDefenderHP, _defenderEntity.DisplayName);
            
           
            DirectorLoadAndPlay(combatIntroPlayable);
            yield return new WaitForSeconds(3f);
            UIAnimationDelegates.InvokeOnShowIfHidden(AnimatorCategory.BattleCinematicHUD);
            yield return new WaitUntil((() => director.state != PlayState.Playing));

            bool defenderActed = false;
            
            while (currEventIndex < _combatOutcome.OrderOfEvents.Count) {
                _currCombatEvent = _combatOutcome.OrderOfEvents[currEventIndex];
                if (_currCombatEvent is CombatTimeline.AttackerDies or CombatTimeline.DefenderDies) break;
                
                // Set the needed generic bindings
                PlayableAsset playableAsset = GetPlayableAssetFromCombatTimelineEnum(_currCombatEvent);
                if (_currCombatEvent is CombatTimeline.AttackerMeleeNormal or CombatTimeline.AttackerMeleeCrit or CombatTimeline.AttackerRangedNormal or CombatTimeline.AttackerRangedCrit) {
                    _isAttackerTurn = true;
                    defenderRigAdapter.localPosition = new Vector3(defenderActed ? 0f : 1.5f, 0, 0);
                    BindActingRig(playableAsset, attackerAnimatedRig.GetComponent<Animator>());
                }
                if (_currCombatEvent is CombatTimeline.DefenderMeleeNormal or CombatTimeline.DefenderMeleeCrit or CombatTimeline.DefenderRangedNormal or CombatTimeline.DefenderRangedCrit) {
                    _isAttackerTurn = false;
                    defenderActed = true;
                    defenderRigAdapter.localPosition = new Vector3(0f, 0, 0);
                    BindActingRig(playableAsset, defenderAnimatedRig.GetComponent<Animator>());
                }
                DirectorLoadAndPlay(playableAsset);
                yield return new WaitUntil((() => director.state != PlayState.Playing));
                // defenderRigAdapter.localPosition = new Vector3(0f, 0, 0);
                if (_isAttackerTurn) { _currAttackIndex++; } else { _currCounterIndex++;}
                currEventIndex++;
            }
            
            UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.BattleCinematicHUD);
            
            // Director play outro cutscene
            DirectorLoadAndPlay(combatOutroPlayable);
            yield return new WaitUntil((() => director.state != PlayState.Playing));
            defenderAnimatedRig.localPosition = new Vector3(0, 0, 0);
            CleanupPuppets();
            ExitCinematicMode();
        }

        private PlayableAsset GetPlayableAssetFromCombatTimelineEnum(CombatTimeline timeline) {
            if (timeline is CombatTimeline.AttackerMeleeNormal or CombatTimeline.DefenderMeleeNormal) return meleeNormalPlayable;
            if (timeline is CombatTimeline.AttackerMeleeCrit or CombatTimeline.DefenderMeleeCrit) return meleeCritPlayable;
            if (timeline is CombatTimeline.AttackerRangedNormal or CombatTimeline.DefenderRangedNormal) return rangedNormalPlayable;
            if (timeline is CombatTimeline.AttackerRangedCrit or  CombatTimeline.DefenderRangedCrit) return rangedCritPlayable;
            throw new Exception("CombatDirector.GetPlayableAssetFromCombatTimelineEnum: Invalid CombatTimeline enum!");
        }

        private void BindActingRig(PlayableAsset playableAsset, Animator actingRigAnimator) {
            TimelineAsset timelineAsset = playableAsset as TimelineAsset;
            if (timelineAsset == null) {
                throw new Exception("CombatDirector.BindActingRig: timelineAsset is null!");
            }
            foreach (TrackAsset track in timelineAsset.GetOutputTracks()) {
                if (track.name == "Animation Track") {
                    director.SetGenericBinding(track, actingRigAnimator);
                }
            }
        }
        

        private void EnterCinematicMode() {
            GameStateData currState = GameStateDelegates.GetCurrentGameState();
            UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.BattleOutcomePreview);
            // Lock user grid input
            // Override camera
        }

        private void SpawnPuppets() {
            
            attackerPuppet.Setup(_attackerEntity);
            defenderPuppet.Setup(_defenderEntity);
        }
       
        private void CleanupPuppets() {
            
        }
        private void ExitCinematicMode() {
            GameStateData currState = GameStateDelegates.GetCurrentGameState();
            UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.BattleOutcomePreview);
            GridDelegates.InvokeOnInspectedTileChanged(GridDelegates.GetTileFromPosition(currState.Combat.InspectedTilePosition), GridDelegates.GetTileFromPosition(_attackerEntity.GridPosition));
            currState.Combat.InspectedTilePosition = _attackerEntity.GridPosition;
            InputDelegates.InvokeOnReinstateGridCursorPosition(_attackerEntity.GridPosition);
            GameStateDelegates.InvokeOnApplyAttackOutcome(_combatOutcome);
            Invoke(nameof(FinalizeAction), 1f);
            // Unlock user grid input
            // Undo camera override
        }

        private void FinalizeAction() {
            GameStateDelegates.InvokeOnFinalizePlayerAction();
        }

        /* Methods called via animation events */
        public void StartSlowMo() {
            Time.timeScale = .5f;
        }
        public void EndSlowMo() {
            Time.timeScale = 1f;
        }
        public void OnAttackStart() {
            Animator targetAnimator = _isAttackerTurn ? attackerPuppet.GetComponent<Animator>() : defenderPuppet.GetComponent<Animator>();
            targetAnimator.SetTrigger(Attack);
            targetAnimator.SetInteger(AttackType, _currCombatEvent is CombatTimeline.AttackerMeleeNormal or CombatTimeline.AttackerMeleeCrit or CombatTimeline.DefenderMeleeNormal or CombatTimeline.DefenderMeleeCrit ? 0 : 1);
        }
        
        public void OnAttackImpact() {
            Animator targetAnimator = _isAttackerTurn ? defenderPuppet.GetComponent<Animator>() : attackerPuppet.GetComponent<Animator>();
            CombatPuppet targetPuppet = _isAttackerTurn ? defenderPuppet : attackerPuppet;
            bool[] hitsArr = _isAttackerTurn ? _combatOutcome.AttackHits : _combatOutcome.DefendCounterHits;
            bool[] critsArr = _isAttackerTurn ? _combatOutcome.AttackHitCrits :  _combatOutcome.DefendCounterCrits;
            int[] dmgInstancesArr = _isAttackerTurn ? _combatOutcome.AttackDamageInstances : _combatOutcome.CounterDamageInstances;
            int currIndex = _isAttackerTurn ? _currAttackIndex : _currCounterIndex;
            bool hit = hitsArr[currIndex];
            bool crit = critsArr[currIndex];
            int impactDamage = dmgInstancesArr[currIndex];
            int victimHPBeforeImpact = _isAttackerTurn ? _currDefenderHP : _currAttackerHP;
            if (_isAttackerTurn) {
                _currDefenderHP = Math.Max(_currDefenderHP - impactDamage, 0);
            } else {
                _currAttackerHP = Math.Max(_currAttackerHP - impactDamage, 0);
            }
            int victimHPAfterImpact = _isAttackerTurn ?  _currDefenderHP : _currAttackerHP;
            int victimMaxHP = _isAttackerTurn ? _defenderEntity.MaxHealth : _attackerEntity.MaxHealth;
            string victimName = _isAttackerTurn ? _defenderEntity.DisplayName : _attackerEntity.DisplayName;
            UIDelegates.InvokeOnCombatCinematicHUDUpdate(!_isAttackerTurn, victimHPAfterImpact, victimMaxHP, victimHPBeforeImpact, victimName);
            if (!hit) {
                targetAnimator.SetTrigger(Dodge);
                // Any miss SFX/VFX
                return;
            }
            targetAnimator.SetTrigger(Hurt);
            targetPuppet.SpawnDamageNumber(impactDamage, crit);
            if (victimHPAfterImpact <= 0) {
                targetAnimator.SetTrigger(Death);
            }
        }
        // ===================================== //
        
        private Vector3 GetAttackPosition() {
            return Vector3.zero;
        }
        private Vector3 GetDefendPosition() {
            return Vector3.zero;
        }
        
 


    }
}
