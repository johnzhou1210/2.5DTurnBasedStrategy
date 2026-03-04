using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Mono.CSharp;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Core.GameState;
using StrategyGame.Grid;
using StrategyGame.UI;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Timeline;

namespace StrategyGame.Combat.Cinematics {
    public class CombatDirector : MonoBehaviour {
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int AttackType = Animator.StringToHash("AttackType");
        private static readonly int Dodge = Animator.StringToHash("Dodge");
        private static readonly int Hurt = Animator.StringToHash("Hurt");
        private static readonly int Death = Animator.StringToHash("Death");
        private static readonly int SkillID = Animator.StringToHash("SkillID");
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
        [SerializeField] private CinemachineImpulseSource cameraShakerSource;
        [SerializeField] private CinemachineImpulseListener cameraShakerListener;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Transform attackerAnimatedRig;
        [SerializeField] private Transform defenderAnimatedRig;
        [SerializeField] private Transform defenderRigAdapter;

        [SerializeField] private GameObject breakEffectPrefab;
        [SerializeField] private Transform billboardCanvasTransform;
        
        [SerializeField] public CombatPuppet attackerPuppet;
        [SerializeField] public CombatPuppet defenderPuppet;
        [SerializeField] private Volume globalVolume;
        
        [Header("Timeline Playable Assets")]
        [SerializeField] private PlayableAsset combatIntroPlayable;
        [SerializeField] private PlayableAsset combatOutroPlayable;
        [SerializeField] private PlayableAsset meleeNormalPlayable;
        [SerializeField] private PlayableAsset meleeCritPlayable;
        [SerializeField] private PlayableAsset rangedNormalPlayable;
        [SerializeField] private PlayableAsset rangedCritPlayable;

        private DepthOfField _depthOfField;
        private ColorAdjustments _colorAdjustments;
        private ChromaticAberration _chromaticAberration;
        private Bloom _bloom;
        private LensDistortion _lensDistortion;
        
        private GridEntity _attackerEntity;
        private GridEntity _defenderEntity;
        private CombatOutcome _combatOutcome;
        private Camera _camera;
        private CombatTimeline _currCombatEvent;
        public bool IsAttackerTurn { get; private set; } = true;
        private int _currAttackIndex = 0;
        private int _currCounterIndex = 0;
        private int _currAttackerHP;
        private int _currDefenderHP;

        private Coroutine _slowMotionCoroutine;

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
            DefenderDies,
            AttackerSkill
        }

        private void Start() {
            _camera = Camera.main;
            globalVolume.profile.TryGet(out _depthOfField);
            globalVolume.profile.TryGet(out _colorAdjustments);
            globalVolume.profile.TryGet(out _chromaticAberration);
            globalVolume.profile.TryGet(out _bloom);
            globalVolume.profile.TryGet(out _lensDistortion);
        }

        private void OnEnable() {
            CombatCinematicsDelegates.GetDirector = () => this;
            CombatCinematicsDelegates.GetProjectileVisualData = GetProjectileVisualDataFromID;
        }

        private void OnDisable() {
            CombatCinematicsDelegates.GetDirector = null;
            CombatCinematicsDelegates.GetProjectileVisualData = null;

            if (_slowMotionCoroutine != null) {
                StopCoroutine(_slowMotionCoroutine);
                _slowMotionCoroutine = null;
            }
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

            SetDepthOfFieldAperture(6.33f);
            
            Debug.Log($"Order of events: {string.Join(",", _combatOutcome.OrderOfEvents)}");
            
            BillboardDelegates.InvokeOnSetLookatTargetTransform(combatCam.transform);
            
            // Director play intro cutscene
            defenderRigAdapter.localPosition = new Vector3(1.5f, 0, 0);
            int currEventIndex = 0;
            _currAttackIndex = 0;
            _currCounterIndex = 0;
            _currAttackerHP = _attackerEntity.Health;
            _currDefenderHP = _defenderEntity.Health;
            
            UIDelegates.InvokeOnCombatCinematicHUDUpdate(true, _currAttackerHP, _attackerEntity.MaxHealth, _currAttackerHP, _attackerEntity.ID);
            UIDelegates.InvokeOnCombatCinematicHUDUpdate(false, _currDefenderHP, _defenderEntity.MaxHealth, _currDefenderHP, _defenderEntity.ID);
           
            DirectorLoadAndPlay(combatIntroPlayable);
            // DOTween.To(() => _depthOfField.focusDistance.value, SetDepthOfFieldFocusDistance, .1f, 2f);
            yield return new WaitForSeconds(2f);
            // DOTween.To(() => _depthOfField.focusDistance.value, SetDepthOfFieldFocusDistance, 2.5f, 1.5f);
            yield return new WaitForSeconds(1f);
            UIAnimationDelegates.InvokeOnShowIfHidden(AnimatorCategory.BattleCinematicHUD);
            yield return new WaitUntil((() => director.state != PlayState.Playing));

            bool defenderActed = false;
            
            Debug.Log($"CombatDirector.PlayCombat: currEventIndex: {currEventIndex}, OrderOfEvents count: {_combatOutcome.OrderOfEvents.Count}");
            
            while (currEventIndex < _combatOutcome.OrderOfEvents.Count) {
                _currCombatEvent = _combatOutcome.OrderOfEvents[currEventIndex];
                if (_currCombatEvent is CombatTimeline.AttackerDies or CombatTimeline.DefenderDies) break;
                
                // Set the needed generic bindings
                PlayableAsset playableAsset = GetPlayableAssetFromCombatTimelineEnum(_currCombatEvent);
                if (_currCombatEvent is CombatTimeline.AttackerMeleeNormal or CombatTimeline.AttackerMeleeCrit or CombatTimeline.AttackerRangedNormal or CombatTimeline.AttackerRangedCrit) {
                    IsAttackerTurn = true;
                    defenderRigAdapter.localPosition = new Vector3(defenderActed ? 0f : 1.5f, 0, 0);
                    BindActingRig(playableAsset, attackerAnimatedRig.GetComponent<Animator>());
                }
                if (_currCombatEvent is CombatTimeline.DefenderMeleeNormal or CombatTimeline.DefenderMeleeCrit or CombatTimeline.DefenderRangedNormal or CombatTimeline.DefenderRangedCrit) {
                    IsAttackerTurn = false;
                    defenderActed = true;
                    defenderRigAdapter.localPosition = new Vector3(0f, 0, 0);
                    BindActingRig(playableAsset, defenderAnimatedRig.GetComponent<Animator>());
                }
                DirectorLoadAndPlay(playableAsset);
                
                CombatPuppet targetPuppet = IsAttackerTurn ? attackerPuppet : defenderPuppet;
                AbilityData ability = _combatOutcome.AttackerSkillID == -1 ? _attackerEntity.BasicAttack : DataDelegates.GetAbilityDataByID(_combatOutcome.AttackerSkillID);
                // If attacking rig's ability has aura, display it.
                if (IsAttackerTurn && ability.AuraPrefab != null) {
                    GameObject aura = Instantiate(ability.AuraPrefab, targetPuppet.transform);
                    aura.name = "Aura";
                }
                
                
                yield return new WaitUntil((() => director.state != PlayState.Playing));
                // defenderRigAdapter.localPosition = new Vector3(0f, 0, 0);
                if (IsAttackerTurn) { _currAttackIndex++; } else { _currCounterIndex++;}
                currEventIndex++;
            }
            
            UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.BattleCinematicHUD);
            // Make defender grid visual face attacker grid visual
            _defenderEntity.VisualFace(_attackerEntity);

            yield return new WaitForSeconds(1f);
            
            // Director play outro cutscene
            DirectorLoadAndPlay(combatOutroPlayable);
            yield return new WaitForSeconds(1f);
            SetDepthOfFieldAperture(16f);
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
            UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.BattleOutcomePreview);
        }

        private void SpawnPuppets() {
            
            attackerPuppet.Setup(this, _attackerEntity);
            defenderPuppet.Setup(this, _defenderEntity);
        }
       
        private void CleanupPuppets() {
            
        }
        private void ExitCinematicMode() {
            GameStateData currState = GameStateDelegates.GetCurrentGameState();
            UIAnimationDelegates.InvokeOnHideIfVisible(AnimatorCategory.BattleOutcomePreview);
            GridDelegates.InvokeOnInspectedTileChanged(GridDelegates.GetTileFromPosition(currState.Combat.InspectedTilePosition), GridDelegates.GetTileFromPosition(_attackerEntity.GridPosition));
            currState.Combat.InspectedTilePosition = _attackerEntity.GridPosition;
            GameStateDelegates.InvokeOnApplyAttackOutcome(_combatOutcome);
            Invoke(nameof(FinalizeAction), 1f);
            if (currState.Combat.TurnPhase == GameStateEnums.TurnPhase.Player) {
                InputDelegates.InvokeOnReinstateGridCursorPosition(_attackerEntity.GridPosition);
            } else if (currState.Combat.TurnPhase == GameStateEnums.TurnPhase.Enemy) {
            }
        }

        private void FinalizeAction() {
            GameStateData currState = GameStateDelegates.GetCurrentGameState();
            switch (currState.Combat.TurnPhase) {
                case GameStateEnums.TurnPhase.Player:
                    GameStateDelegates.InvokeOnFinalizePlayerAction();
                    break;
                case GameStateEnums.TurnPhase.Enemy:
                    // Send signal for game state manager to continue enemy coroutine
                    currState.Combat.EnemyActorFinishedCombatCinematic = true;
                    break;
                default:
                    break;
            }
            
        }

        /* Methods called via Signals */
        public void OnSpawnProjectile() {
            // Decide which projectile to spawn based on context
            // Decide which puppet to call the spawn on
            CombatPuppet targetPuppet = IsAttackerTurn ? attackerPuppet : defenderPuppet;
            AbilityData ability = _combatOutcome.AttackerSkillID == -1 ? _attackerEntity.BasicAttack : DataDelegates.GetAbilityDataByID(_combatOutcome.AttackerSkillID);
            SpawnProjectile(targetPuppet, ability.ProjectileVisualData.ProjectileID);
        }

        private int GetProjectileIDFromEntity(AbilityData abilityData) {
            switch (abilityData.name) {
                case "Arrow Shot":
                    return 0;
                case "Holy Light":
                    return 1;
                case "Heal":
                    return 0; // replace with heal id later
                default:
                    return 0;
            }
        }
        
        public void StartSlowMo() {
            Time.timeScale = .45f;
        }
        public void EndSlowMo() {
            Time.timeScale = 1f;
        }
        public void OnAttackStart() {
            Animator targetAnimator = IsAttackerTurn ? attackerPuppet.GetComponent<Animator>() : defenderPuppet.GetComponent<Animator>();
            targetAnimator.SetTrigger(Attack);
            targetAnimator.SetInteger(AttackType, _currCombatEvent is CombatTimeline.AttackerMeleeNormal or CombatTimeline.AttackerMeleeCrit or CombatTimeline.DefenderMeleeNormal or CombatTimeline.DefenderMeleeCrit ? 0 : 1);
            targetAnimator.SetInteger(SkillID, _combatOutcome.AttackerSkillID);
        }
        
        public void OnAttackImpact() {
            
            
            GridEntity victimEntity = IsAttackerTurn ? _defenderEntity : _attackerEntity;
            Animator targetAnimator = IsAttackerTurn ? defenderPuppet.GetComponent<Animator>() : attackerPuppet.GetComponent<Animator>();
            CombatPuppet initiatorPuppet = IsAttackerTurn ? attackerPuppet : defenderPuppet;
            CombatPuppet targetPuppet = IsAttackerTurn ? defenderPuppet : attackerPuppet;
            
            // Clear aura if any is present
            if (initiatorPuppet.transform.Find("Aura")) {
                Destroy(initiatorPuppet.transform.Find("Aura").gameObject); 
            }
            
            bool[] hitsArr = IsAttackerTurn ? _combatOutcome.AttackHits : _combatOutcome.DefendCounterHits;
            bool[] critsArr = IsAttackerTurn ? _combatOutcome.AttackHitCrits :  _combatOutcome.DefendCounterCrits;
            bool[] breaksArr = IsAttackerTurn ? _combatOutcome.AttackBreakHits : _combatOutcome.CounterBreakHits;
            int[] dmgInstancesArr = IsAttackerTurn ? _combatOutcome.AttackDamageInstances : _combatOutcome.CounterDamageInstances;
            int currIndex = IsAttackerTurn ? _currAttackIndex : _currCounterIndex;
            bool hit = hitsArr[currIndex];
            bool crit = critsArr[currIndex];
            bool targetBrokenThisSimulation = IsAttackerTurn ? _combatOutcome.DefenderBrokenThisSimulation : _combatOutcome.AttackerBrokenThisSimulation;
            bool isBreak = breaksArr[currIndex] && targetBrokenThisSimulation;
            int impactDamage = dmgInstancesArr[currIndex];
            int victimHPBeforeImpact = IsAttackerTurn ? _currDefenderHP : _currAttackerHP;
            
            if (_currCombatEvent is CombatTimeline.AttackerMeleeNormal or CombatTimeline.AttackerMeleeCrit or CombatTimeline.DefenderMeleeNormal or CombatTimeline.DefenderMeleeCrit) {
                SpawnImpactVFX(GetProjectileVisualDataFromID(0), initiatorPuppet.VFXTransform, hit, targetPuppet.transform.position, isBreak, true); // temp
            }
            
            if (IsAttackerTurn) {
                _currDefenderHP = Math.Max(_currDefenderHP - impactDamage, 0);
            } else {
                _currAttackerHP = Math.Max(_currAttackerHP - impactDamage, 0);
            }
            int victimHPAfterImpact = IsAttackerTurn ?  _currDefenderHP : _currAttackerHP;
            int victimMaxHP = IsAttackerTurn ? _defenderEntity.MaxHealth : _attackerEntity.MaxHealth;
            string victimName = IsAttackerTurn ? _defenderEntity.DisplayName : _attackerEntity.DisplayName;
            UIDelegates.InvokeOnCombatCinematicHUDUpdate(!IsAttackerTurn, victimHPAfterImpact, victimMaxHP, victimHPBeforeImpact, victimEntity.ID);
            if (!hit) {
                targetAnimator.SetTrigger(Dodge);
                // Any miss SFX/VFX
                return;
            }
            targetAnimator.SetTrigger(Hurt);
            if (crit || isBreak || victimHPAfterImpact <= 0) {
                if (_slowMotionCoroutine != null) {
                    StopCoroutine(_slowMotionCoroutine);
                    _slowMotionCoroutine = null;
                }
                _slowMotionCoroutine = StartCoroutine(SlowMotionCoroutine(crit, isBreak));
            }
            cameraShakerSource.ImpulseDefinition.AmplitudeGain = crit ? .67f : isBreak ? .33f : .1f;       
            cameraShakerSource.ImpulseDefinition.ImpulseDuration = crit ? .67f : isBreak ? .33f : .1f;
            cameraShakerSource.GenerateImpulse();
            targetPuppet.SpawnDamageNumber(impactDamage, crit, isBreak);
            if (victimHPAfterImpact <= 0) {
                targetAnimator.SetTrigger(Death);
            }
        }

        
        
        // ===================================== //
        public void SpawnProjectile(CombatPuppet spawner, int projectileID) {
            bool[] hitsArr = IsAttackerTurn ? _combatOutcome.AttackHits : _combatOutcome.DefendCounterHits;
            bool[] critsArr = IsAttackerTurn ? _combatOutcome.AttackHitCrits :  _combatOutcome.DefendCounterCrits;
            bool[] breaksArr = IsAttackerTurn ? _combatOutcome.AttackBreakHits : _combatOutcome.CounterBreakHits;
            int currIndex = IsAttackerTurn ? _currAttackIndex : _currCounterIndex;
            bool hit = hitsArr[currIndex];
            bool crit = critsArr[currIndex];
            bool targetBrokenThisSimulation = IsAttackerTurn ? _combatOutcome.DefenderBrokenThisSimulation : _combatOutcome.AttackerBrokenThisSimulation;
            bool isBreak = breaksArr[currIndex] && targetBrokenThisSimulation;
            
            ProjectileVisualData projectileVisualData = CombatCinematicsDelegates.GetProjectileVisualData(projectileID);
            GameObject projectile = Instantiate(projectileVisualData.ProjectilePrefab, spawner.VFXTransform);
            projectile.transform.position = spawner.ProjectileSpawnPointTransform.position;
            ProjectileVisual projectileVisual = projectile.GetComponent<ProjectileVisual>();
            if (projectileVisual == null) throw new Exception("CombatPuppet.SpawnProjectile: ProjectileVisual script not found!");
            projectileVisual.Setup(spawner, projectileVisualData, hit, isBreak);
        }
        
        public void SpawnImpactVFX(ProjectileVisualData projectileVisualData, Transform vfxTransform, bool hit, Vector3 position, bool isBreak, bool melee = false) {
            if (!melee) OnAttackImpact();
            if (!hit && melee) return;
            if (IsAttackerTurn && _combatOutcome.AttackerSkillID != -1) {
                AbilityData ability = DataDelegates.GetAbilityDataByID(_combatOutcome.AttackerSkillID);
                if (melee && ability.CollisionEffect != null) {
                    GameObject meleeCollisionVFX = Instantiate(ability.CollisionEffect, vfxTransform);
                    meleeCollisionVFX.transform.position = defenderPuppet.transform.position;
                    meleeCollisionVFX.name = "MeleeCollisionVFX";
                }
                // Add additional cool effects (e.g. giga impact)
                ImpactEffectType extraImpactEffect = DataDelegates.GetAbilityDataByID(_combatOutcome.AttackerSkillID).ImpactEffectType;
                switch (extraImpactEffect) {
                    case ImpactEffectType.None:
                        break;
                    case ImpactEffectType.GigaImpactMonochrome:
                        StartCoroutine(GigaImpactMonochromeEffect());
                        break;
                    case ImpactEffectType.GigaImpactFury:
                        StartCoroutine(GigaImpactFuryEffect());
                        break;
                    default:
                        throw new Exception($"Unknown impact effect type: {extraImpactEffect}");
                }
                
            }
            if (!hit) {
                if (projectileVisualData.MissVFXPrefab != null) {
                    GameObject missVFX = Instantiate(projectileVisualData.MissVFXPrefab, vfxTransform);
                    missVFX.transform.position = position;
                }
                if (projectileVisualData.MissBillboardVFXPrefab != null) {
                    GameObject missBillboardVFX = Instantiate(projectileVisualData.MissBillboardVFXPrefab, billboardCanvasTransform);
                    missBillboardVFX.transform.position = position;
                }
                return;
            }
            
            GameObject impactVFX = Instantiate(projectileVisualData.ImpactVFXPrefab, vfxTransform);
            impactVFX.transform.position = position;
            if (projectileVisualData.ImpactBillboardVFXPrefab != null) {
                GameObject billboardImpactVFX = Instantiate(projectileVisualData.ImpactBillboardVFXPrefab, billboardCanvasTransform);
                billboardImpactVFX.transform.position = position;
            }
            if (isBreak) {
                // spawn glass particles
                Debug.Log("BREAK!!!!!!!!!");
                GameObject breakEffect = Instantiate(breakEffectPrefab, billboardCanvasTransform);
                breakEffect.transform.position = position;
            }
            // Auto Cleanup is handled in an AutoCleanup script attached to the vfx

            IEnumerator GigaImpactMonochromeEffect() {
                float originalSaturation = _colorAdjustments.saturation.value;
                float originalPostExposure = _colorAdjustments.postExposure.value;
                float originalContrast = _colorAdjustments.contrast.value;
                float originalChromaticAberration = _chromaticAberration.intensity.value;
                float originalLensDistortion = _lensDistortion.intensity.value;
                float originalBloom = _bloom.intensity.value;
                SetSaturation(-100f);
                SetPostExposure(1f);
                SetContrast(100f);
                SetChromaticAberration(1f);
                SetLensDistortionIntensity(-1f);
                SetBloomIntensity(4f);
                cameraShakerSource.ImpulseDefinition.AmplitudeGain = 1f;       
                cameraShakerSource.ImpulseDefinition.ImpulseDuration = .5f;
                cameraShakerSource.GenerateImpulse();
                yield return new WaitForSeconds(.2f);
                SetSaturation(originalSaturation);
                SetPostExposure(originalPostExposure);
                SetContrast(originalContrast);
                SetChromaticAberration(originalChromaticAberration);
                SetLensDistortionIntensity(originalLensDistortion);
                SetBloomIntensity(originalBloom);
                yield return null;
            }
            IEnumerator GigaImpactFuryEffect() {
                float originalSaturation = _colorAdjustments.saturation.value;
                float originalPostExposure = _colorAdjustments.postExposure.value;
                float originalContrast = _colorAdjustments.contrast.value;
                float originalChromaticAberration = _chromaticAberration.intensity.value;
                float originalTimeScale = Time.timeScale;
                float originalLensDistortion = _lensDistortion.intensity.value;
                float originalBloom = _bloom.intensity.value;
                SetSaturation(100f);
                SetPostExposure(1f);
                SetContrast(100f);
                SetChromaticAberration(1f);
                SetLensDistortionIntensity(-.5f);
                SetBloomIntensity(4f);
                Time.timeScale = .5f;
                cameraShakerSource.ImpulseDefinition.AmplitudeGain = 1f;       
                cameraShakerSource.ImpulseDefinition.ImpulseDuration = .5f;
                cameraShakerSource.GenerateImpulse();
                yield return new WaitForSeconds(.25f);
                Time.timeScale = originalTimeScale;
                SetSaturation(originalSaturation);
                SetPostExposure(originalPostExposure);
                SetContrast(originalContrast);
                SetChromaticAberration(originalChromaticAberration);
                SetLensDistortionIntensity(originalLensDistortion);
                SetBloomIntensity(originalBloom);
                yield return null;
            }
        }
        
        
        private void SetDepthOfFieldAperture(float newVal) {
            if (_depthOfField == null) return;
            _depthOfField.aperture.value = newVal;
        }

        private void SetContrast(float newVal) {
            if (_colorAdjustments == null) return;
            _colorAdjustments.contrast.value = newVal;
        }

        private void SetSaturation(float newVal) {
            if (_colorAdjustments == null) return;
            _colorAdjustments.saturation.value = newVal;
        }

        private void SetChromaticAberration(float newVal) {
            if (_chromaticAberration == null) return;
            _chromaticAberration.intensity.value = newVal;
        }

        private void SetPostExposure(float newVal) {
            if (_colorAdjustments == null) return;
            _colorAdjustments.postExposure.value = newVal;
        }

        private void SetBloomIntensity(float newVal) {
            if (_bloom == null) return;
            _bloom.intensity.value = newVal;
        }

        private void SetLensDistortionIntensity(float newVal) {
            if (_lensDistortion == null) return;
            _lensDistortion.intensity.value = newVal;
        }
        private void SetDepthOfFieldFocusDistance(float newVal) {
            if (_depthOfField == null) return;
            _depthOfField.focusDistance.value = newVal;
        }

        private ProjectileVisualData GetProjectileVisualDataFromID(int projectileID) {
            ProjectileVisualData queryResult = DataDelegates.GetProjectileVisualDataByID(projectileID);
            return queryResult == null ? throw new Exception($"CombatDirector.GetProjectileVisualDataFromID: Could not find projectile of id {projectileID}!") : queryResult;
        }

        private IEnumerator SlowMotionCoroutine(bool isCrit, bool isBreak, float duration = 1f) {
            StartSlowMo();
            if (isCrit) {
                duration = 1.5f;
                DOTween.To(() => _colorAdjustments.contrast.value, SetContrast, 69f, duration / 8f);
                DOTween.To(() => _colorAdjustments.postExposure.value, SetPostExposure, -1.5f, duration / 8f);
                DOTween.To(() => _bloom.intensity.value, SetBloomIntensity, 3f, duration / 8f);
            }
            if (isBreak) {
                DOTween.To(() => _chromaticAberration.intensity.value, SetChromaticAberration, 1f, duration / 8f);
                DOTween.To(() => _lensDistortion.intensity.value, SetLensDistortionIntensity, -.25f, duration / 8f);
            }
            yield return new WaitForSeconds(duration/2f);
            if (isCrit) {
                DOTween.To(() => _colorAdjustments.contrast.value, SetContrast, 21.7f, duration / 8f);
                DOTween.To(() => _colorAdjustments.postExposure.value, SetPostExposure, 0f, duration / 8f);
                DOTween.To(() => _bloom.intensity.value, SetBloomIntensity, 1f, duration / 8f);
            }
            if (isBreak) {
                DOTween.To(() => _chromaticAberration.intensity.value, SetChromaticAberration, 0f, duration / 8f);
                DOTween.To(() => _lensDistortion.intensity.value, SetLensDistortionIntensity, 0f, duration / 8f);
            }
            yield return new WaitForSeconds(duration/2f);
            EndSlowMo();
        }
 


    }
}
