
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using StrategyGame.Audio;
using StrategyGame.Combat;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.GameState;
using StrategyGame.Factions;
using StrategyGame.Grid;
using StrategyGame.UI;
using StrategyGame.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace StrategyGame.Core.Input {
    public class CombatInputManager : InputManagerBase, IPointerMoveHandler {
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        [SerializeField] private MouseInputRaycaster gridMouseInputRaycaster;
        
        [Header("Path Selection Settings")] [SerializeField]
        private float pathSelectionMoveActionCooldown = 0.33f;
        [SerializeField] private float pathSelectionMoveActionCooldownAccelerationThreshold = 1f;
        [SerializeField] private float pathSelectionMoveActionMinimumCooldown = .08f;
        [SerializeField] private float pathSelectionMoveActionCooldownAccelerationRate = .05f;
        private InputAction _dangerZoneAction;
        private InputAction _cycleLeftAction;
        private InputAction _cycleRightAction;
        private float _pathSelectionMoveActionTimer;
        private float _currentPathSelectionMoveActionCooldown;
        private float _pathSelectionMoveActionHeldDuration;
        
        private bool _isDiagonalMoveEnabled = true;
        private bool _isDangerZoneVisible = false;
        
        private CameraRigController _cameraRigController;
        private GridCursorRenderer _gridCursorRenderer;
        private Vector2Int _gridCursorPosition;
        public Vector2Int GridCursorPosition {
            get => _gridCursorPosition;
            set {
                _gridCursorPosition = value;
                _gridCursorRenderer.MoveTo(_gridCursorPosition);
            }
        }

        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        override protected void Awake() {
            base.Awake();
            ServiceLocator.Register(this);
            enabled = false;
        }
        
        override protected void OnEnable() {
            base.OnEnable();
            InputDelegates.OnSetMouseRaycastEnabled += SetMouseRaycastEnabled;
            InputDelegates.OnReinstateGridCursorPosition += ReinstateGridCursorPosition;
            InputDelegates.GetGridCursorPosition = () => GridCursorPosition;
            InputDelegates.GetDangerZoneVisible = () => _isDangerZoneVisible;
        }
        override protected void OnDisable() {
            base.OnDisable();
            InputDelegates.OnSetMouseRaycastEnabled -= SetMouseRaycastEnabled;
            InputDelegates.OnReinstateGridCursorPosition -= ReinstateGridCursorPosition;
            InputDelegates.GetGridCursorPosition = null;
            InputDelegates.GetDangerZoneVisible = null;
        }
        
        
        override protected void OnDestroy() {
            // gridMouseInputRaycaster.enabled = false;
            base.OnDestroy();
            ServiceLocator.Unregister<CombatInputManager>();
        }
        
        
        // =================================
        // ABSTRACT OVERRIDES
        // =================================
        override protected void ProcessInput() {
            HandleCancellationInput();
            HandleInteractionInput();
            HandleAxisInput();
            HandleCombatControls();
            HandleFastForward();
        }
        override protected void HandleCancellationInput() {
            if (!cancelAction.WasPerformedThisFrame())
                return;
            GameStateData.GameStateDatagram currentGameState = GameStateDelegates.GetCurrentGameState();
            switch (currentGameState.Combat.TurnPhase) {
                case CombatStateEnums.TurnPhase.Player:
                    switch (currentGameState.Combat.PlayerPhase) {
                        case CombatStateEnums.PlayerPhaseState.SelectUnitToControl: break;
                        case CombatStateEnums.PlayerPhaseState.SelectUnitMoveDestination:
                            GameStateDelegates.InvokeOnPlayerPhaseStateChanged(CombatStateEnums.PlayerPhaseState.SelectUnitToControl);
                            _isDiagonalMoveEnabled = true;
                            break;
                        case CombatStateEnums.PlayerPhaseState.UnitMovingToDestination: break;
                        case CombatStateEnums.PlayerPhaseState.UnitActionMenu:
                            // Delegate task to CombatActionMenuController
                            InputDelegates.InvokeOnCancelPressed(); break;
                        case CombatStateEnums.PlayerPhaseState.UnitSelectTarget:
                            currentGameState.Combat.InspectedEntityID = currentGameState.Combat.SelectedEntityID;
                            GridDelegates.SetInspectedTile(EntityDelegates.GetGridEntityByID(currentGameState.Combat.SelectedEntityID).GridPosition);
                            GameStateDelegates.InvokeOnPlayerPhaseStateChanged(CombatStateEnums.PlayerPhaseState.UnitActionMenu);
                            ServiceLocator.Get<AudioManager>().PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/minimize_008"));
                            break;
                        case CombatStateEnums.PlayerPhaseState.UnitAttackCutscene: break;
                        case CombatStateEnums.PlayerPhaseState.None: break;
                        default: throw new Exception("InputManager.HandleCancellationInput: Invalid player phase state enum!");
                    }
                    break;
                case CombatStateEnums.TurnPhase.Enemy: break;
                case CombatStateEnums.TurnPhase.Event: break;
                case CombatStateEnums.TurnPhase.None: break;
                default: throw new Exception("InputManager.HandleCancellationInput: Invalid turn phase state enum!");
            }
        }
        override protected void HandleInteractionInput() {
            GameStateData.GameStateDatagram currentGameState = GameStateDelegates.GetCurrentGameState();
            switch (currentGameState.Combat.TurnPhase) {
                case CombatStateEnums.TurnPhase.Player:
                    switch (currentGameState.Combat.PlayerPhase) {
                        case CombatStateEnums.PlayerPhaseState.SelectUnitToControl: HandleEntityTileSelection(); break;
                        case CombatStateEnums.PlayerPhaseState.SelectUnitMoveDestination: HandleEntityTileSelection(); break;
                        case CombatStateEnums.PlayerPhaseState.UnitMovingToDestination:
                            // HandleFastForward(); 
                            break;
                        case CombatStateEnums.PlayerPhaseState.UnitActionMenu: HandleUIConfirmation(); break;
                        case CombatStateEnums.PlayerPhaseState.UnitSelectTarget: HandleEntityTileSelection(); break;
                        case CombatStateEnums.PlayerPhaseState.UnitAttackCutscene:
                            // HandleFastForward(); 
                            break;
                        case CombatStateEnums.PlayerPhaseState.None: break;
                        default: throw new Exception("InputManager.HandleInteractionInput: Invalid player phase state enum!");
                    }
                    break;
                case CombatStateEnums.TurnPhase.Enemy:
                    // HandleFastForward(); 
                    break;
                case CombatStateEnums.TurnPhase.Event:
                    // HandleFastForward();
                    break;
                case CombatStateEnums.TurnPhase.None: break;
                default: throw new Exception("InputManager.HandleInteractionInput: Invalid turn phase state enum!");
            }
        }
        override protected void HandleAxisInput() {
            _pathSelectionMoveActionTimer = Mathf.Max(0f, _pathSelectionMoveActionTimer - Time.deltaTime);
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            GameStateData.GameStateDatagram currentCombatState = GameStateDelegates.GetCurrentGameState();
            if (currentCombatState.Combat.TurnPhase != CombatStateEnums.TurnPhase.Player)
                return;
            switch (currentCombatState.Combat.PlayerPhase) {
                case CombatStateEnums.PlayerPhaseState.SelectUnitToControl: HandleGridNavigationInput(moveInput); break;
                case CombatStateEnums.PlayerPhaseState.SelectUnitMoveDestination: HandleGridNavigationInput(moveInput); break;
                case CombatStateEnums.PlayerPhaseState.UnitMovingToDestination: break;
                case CombatStateEnums.PlayerPhaseState.UnitActionMenu: HandleMenuAxisInput(moveInput); break;
                case CombatStateEnums.PlayerPhaseState.UnitSelectTarget: HandleGridNavigationInput(moveInput); break;
                case CombatStateEnums.PlayerPhaseState.UnitAttackCutscene: break;
                case CombatStateEnums.PlayerPhaseState.None: break;
                default: throw new Exception("InputManager.HandleAxisInput: Invalid player phase state enum!");
            }
        }
        
        
        // ==============================
        // VIRTUAL OVERRIDES
        // ==============================
        override protected void InitializeActions() {
            base.InitializeActions();
            InputActionAsset actions = playerInput.currentActionMap.asset;
            _dangerZoneAction = actions.FindAction("DangerZone", true);
            _cycleLeftAction = actions.FindAction("CycleLeft", true);
            _cycleRightAction = actions.FindAction("CycleRight", true);
            Debug.Log("CombatInputManager.InitializeActions: Input actions initialized");
        }
        override protected void Update() {
            base.Update();
        }
        
        
        
        // ==============================
        // CORE METHODS
        // ==============================
        private void SetMouseRaycastEnabled(bool value) {
            gridMouseInputRaycaster.enabled = value;
        }

        // ==============================
        // IMPLEMENTED METHODS
        // ==============================
        public void OnPointerMove(PointerEventData eventData) {
            // GameStateDelegates.InvokeOnUnitMoveSelectionChanged(GameStateManager.UnitMoveSelectionMode.Automatic);
        }
        public void OnGridCursorMove(Vector2Int originalPosition, Vector2Int moveVector) {
            GameStateData.GameStateDatagram currentCombatState = GameStateDelegates.GetCurrentGameState();
            Vector2Int gridDimensions = GridDelegates.GetGridDimensions();
            Vector2Int newPosition = originalPosition + moveVector;
            Vector2Int? bestTilePosition = null;
            if (moveVector == Vector2Int.zero) {
                GridCursorPosition = originalPosition;
                GridDelegates.SetInspectedTile(GridCursorPosition);
                return;
            }
            if (currentCombatState.Combat.PlayerPhase == CombatStateEnums.PlayerPhaseState.UnitSelectTarget) {
                // Choose best newPosition
                float lowestPenalty = float.MaxValue;
                GridEntity actorEntity = EntityDelegates.GetGridEntityByID(currentCombatState.Combat.SelectedEntityID);
                AbilityData currentSkill = DataDelegates.GetAbilityDataByID(currentCombatState.Combat.CurrentSelectedSkillID);
                if (currentSkill == null)
                    currentSkill = actorEntity.BasicAttack;
                HashSet<Tile> targetSelectionTiles = actorEntity.GetAttackableTilesAtPosition(actorEntity.GridPosition, currentSkill);
                foreach (Tile tile in targetSelectionTiles) {
                    if (tile.Position == originalPosition)
                        continue;
                    Vector2 toTile = tile.Position - originalPosition;
                    float dot = Vector2.Dot(toTile.normalized, moveVector);
                    if (dot <= .2f)
                        continue; // Ignore sideways/backwards tiles
                    float anglePenalty = 1f - dot;
                    float distance = toTile.sqrMagnitude;
                    float currPenalty = distance + anglePenalty * 5f;
                    if (moveVector.x != 0) // moving left/right
                    {
                        if (tile.Position.y == originalPosition.y)
                            currPenalty *= 0.7f; // prefer same row
                    }
                    if (moveVector.y != 0) // moving up/down
                    {
                        if (tile.Position.x == originalPosition.x)
                            currPenalty *= 0.7f; // prefer same column
                    }
                    if (currPenalty < lowestPenalty) {
                        lowestPenalty = currPenalty;
                        bestTilePosition = tile.Position;
                    }
                }
                if (bestTilePosition.HasValue)
                    GridCursorPosition = bestTilePosition.Value;
            } else {
                GridCursorPosition = new Vector2Int(Math.Clamp(newPosition.x, 0, gridDimensions.x - 1), Math.Clamp(newPosition.y, 0, gridDimensions.y - 1));
            }
            bool success = GridDelegates.SetInspectedTile(GridCursorPosition);
            if (!success) {
                Debug.LogWarning("InputManager.OnGridCursorMove: Success is false!");
                GridCursorPosition = originalPosition;
            }
        }

        // ==============================
        // HELPERS
        // ==============================
        // private void InitializeBattleInput() {
        //     CombatStateEnums.UnitMoveSelectionMode currentUnitMoveSelectionMode = GameStateDelegates.GetCurrentGameState().Combat.UnitMoveSelectionMode;
        //     gridMouseInputRaycaster.enabled = currentUnitMoveSelectionMode == CombatStateEnums.UnitMoveSelectionMode.Automatic;
        //     cameraRigController.SetPanningEnabled(currentUnitMoveSelectionMode == CombatStateEnums.UnitMoveSelectionMode.Automatic);
        //     // cameraRigController.SetZoomingEnabled(currentUnitMoveSelectionMode == UnitMoveSelectionMode.Automatic);
        // }
        public void SetCameraRigController(CameraRigController cameraRigController) {
            _cameraRigController = cameraRigController;
        }
        public void SetGridCursorRenderer(GridCursorRenderer gridCursorRenderer) {
            _gridCursorRenderer = gridCursorRenderer;
        }

        private void HandleGridNavigationInput(Vector2 moveInput) {
            Vector2Int moveDirection = new Vector2Int(moveInput.x > .5f ? 1 :
                moveInput.x < -.5f ? -1 : 0, moveInput.y > .5f ? 1 :
                moveInput.y < -.5f ? -1 : 0);
            if (moveAction.WasReleasedThisFrame()) {
                _pathSelectionMoveActionTimer = 0f;
            }
            if (moveDirection != Vector2.zero) {
                _pathSelectionMoveActionHeldDuration += Time.deltaTime;
                if (_pathSelectionMoveActionHeldDuration > pathSelectionMoveActionCooldownAccelerationThreshold) {
                    float extraHeldTime = _pathSelectionMoveActionHeldDuration - pathSelectionMoveActionCooldownAccelerationThreshold;
                    float acceleratedCooldown = _currentPathSelectionMoveActionCooldown - extraHeldTime * pathSelectionMoveActionCooldownAccelerationRate;
                    _currentPathSelectionMoveActionCooldown = Mathf.Max(pathSelectionMoveActionMinimumCooldown, acceleratedCooldown);
                }
                if (_pathSelectionMoveActionTimer > 0f)
                    return;
                _pathSelectionMoveActionTimer = _currentPathSelectionMoveActionCooldown;
                Vector2Int moveVector = moveDirection;
                if (!_isDiagonalMoveEnabled && moveVector.x != 0 && moveVector.y != 0) {
                    moveVector.y = 0;
                }
                OnGridCursorMove(GridCursorPosition, moveVector);
            } else {
                _pathSelectionMoveActionHeldDuration = 0f;
                _currentPathSelectionMoveActionCooldown = Mathf.Lerp(_currentPathSelectionMoveActionCooldown, pathSelectionMoveActionCooldown, Time.deltaTime * 5f);
            }
        }
       
        
        private void HandleEntityTileSelection() {
            if (!selectAction.WasPressedThisFrame())
                return;
            GameStateData.GameStateDatagram state = GameStateDelegates.GetCurrentGameState();
            switch (state.Combat.PlayerPhase) {
                // In order to select, there must be an entity
                case CombatStateEnums.PlayerPhaseState.SelectUnitToControl when state.Combat.InspectedEntityID == -1:
                    Debug.Log("InputManager.HandleSelectionInput: Current selected entity is null!");
                    return;
                case CombatStateEnums.PlayerPhaseState.SelectUnitToControl:
                    // Disallow if the unit's ID is not in ActorIDsRemaining list.
                    if (!state.Combat.ActorIDsRemaining.Contains(state.Combat.InspectedEntityID)) {
                        Debug.Log("InputManager.HandleEntityTileSelection: The currently inspected entity needs to wait for their turn phase or has already acted!");
                        return;
                    }
                    Debug.Log("InputManager.HandleEntityTileSelection: START FORMING PATH");
                    _isDiagonalMoveEnabled = false;
                    GameStateDelegates.InvokeOnPlayerPhaseStateChanged(CombatStateEnums.PlayerPhaseState.SelectUnitMoveDestination);
                    ServiceLocator.Get<AudioManager>().PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/select_005"));
                    break;
                case CombatStateEnums.PlayerPhaseState.SelectUnitMoveDestination: {
                    GridEntity currentSelectedEntity = EntityDelegates.GetGridEntityByID(state.Combat.SelectedEntityID);
                    if (currentSelectedEntity == null) {
                        Debug.LogWarning("InputManager.HandleEntityTileSelection: Current selected entity is null!");
                        return;
                    }
                    Debug.Log("InputManager.HandleEntityTileSelection: STOP FORMING PATH");
                    _isDiagonalMoveEnabled = true;
                    /* To check if destination is valid: the following must be true:
                     * 1) The length of the path must be less than or equal to unit's move stat
                     * 2) Intersecting valid tiles set with path must result in just the path set.
                     */
                    ManualPath manualPath = GameStateDelegates.GetManualPath();
                    HashSet<Tile> walkableTiles = currentSelectedEntity.GetWalkableTiles(true);
                    HashSet<Tile> manualPathSet = manualPath.Unique;
                    List<Tile> manualPathList = manualPath.Tiles;
                    manualPathSet.IntersectWith(walkableTiles);
                    manualPathSet.Add(GridDelegates.GetTileFromPosition(currentSelectedEntity.GridPosition));
                    Debug.Log($"InputManager.HandleSelectionInput: Walkable tiles: {string.Join(", ", walkableTiles)}");
                    Debug.Log(
                        $"InputManager.HandleSelectionInput: Manual path tiles: {string.Join(", ", manualPathList)} | Selected entity movement range: {currentSelectedEntity.MovementRange} | Manual path set: {string.Join(", ", manualPathSet)}");
                    Tile destinationTile = manualPathList[^1];
                    HashSet<GridEntity> attackerTrueAttackRange = currentSelectedEntity.GetEntitiesWithinAttackRange();
                    bool conditionsNeededToDirectlyAttackTarget = destinationTile.Occupant is { Faction: Faction.Enemy } && attackerTrueAttackRange.Any(e => e.ID == destinationTile.Occupant.ID);
                    bool isDestinationValid = manualPathList.Count - 1 <= currentSelectedEntity.MovementRange || conditionsNeededToDirectlyAttackTarget;
                    // Debug.Log($"Condition1: {manualPathList.Count - 1 <= currentSelectedEntity.MovementRange}");
                    Debug.Log($"InputManager.HandleSelectionInput: IsDestinationValid: {isDestinationValid}");
                    if (isDestinationValid) {
                        // Check if the tile at the destination has an occupant.
                        // If enemy occupant, directly go to choose target phase state, else, go to moving to destination state.
                        bool conditionsNeededToMoveToDestination = manualPathList.ToHashSet().IsSubsetOf(manualPathSet) && (destinationTile.Occupant == null || destinationTile.Occupant == currentSelectedEntity);
                        if (conditionsNeededToDirectlyAttackTarget) { // Assumes enemy
                            // Give player ability to directly attack the target
                            // Change state to moving to destination
                            // Remove the last item on the list because that one is occupied by an enemy
                            GameStateDelegates.GetCurrentGameState().Combat.PlayerDirectAttackAvailable = true;
                            manualPathList.RemoveAt(manualPathList.Count - 1);
                            currentSelectedEntity.MoveAlongPath(manualPathList);
                            GameStateDelegates.GetCurrentGameState().Combat.HighestPriorityTargetEntityID = destinationTile.Occupant.ID;
                            GameStateDelegates.InvokeOnPlayerPhaseStateChanged(CombatStateEnums.PlayerPhaseState.UnitMovingToDestination);
                        } else if (conditionsNeededToMoveToDestination) {
                            // Move unit to destination
                            Debug.Log($"ConditionsNeededToMoveToDestination is true");
                            currentSelectedEntity.MoveAlongPath(manualPathList);
                            GameStateDelegates.InvokeOnPlayerPhaseStateChanged(CombatStateEnums.PlayerPhaseState.UnitMovingToDestination);
                        } else {
                            Debug.Log(
                                $"InputManager.HandleEntityTileSelection: conditionsNeededToDirectlyAttackTarget is {conditionsNeededToDirectlyAttackTarget} and conditionsNeededToMoveToDestination is {conditionsNeededToMoveToDestination}");
                        }
                    } else {
                        Debug.Log($"InputManager.HandleSelectionInput: Current manual path is not allowed!");
                        // GameStateDelegates.InvokeOnPlayerPhaseStateChanged(CombatStateEnums.PlayerPhaseState.SelectUnitToControl);
                    }
                    break;
                }
                case CombatStateEnums.PlayerPhaseState.UnitSelectTarget:
                    GridEntity actingEntity = EntityDelegates.GetGridEntityByID(state.Combat.SelectedEntityID);
                    GridEntity targetEntity = GridDelegates.GetTileFromPosition(state.Combat.InspectedTilePosition).Occupant;
                    // Perform action on target
                    // Retrieve ability data from game state
                    AbilityData skillData = DataDelegates.GetAbilityDataByID(state.Combat.CurrentSelectedSkillID);
                    if (skillData == null)
                        skillData = actingEntity.BasicAttack;
                    if (targetEntity == null) {
                        Debug.LogWarning("InputManager.HandleEntityTileSelection: Exiting switch case early because targetEntity is null");
                        ServiceLocator.Get<AudioManager>().PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/pluck_001"));
                        break;
                    }
                    bool targetIsAlly = actingEntity.IsFriendlyWith(targetEntity);
                    if ((skillData.CanTargetAllies && targetIsAlly) || (skillData.CanTargetEnemies && !targetIsAlly) || (skillData.CanTargetSelf && actingEntity.ID == targetEntity.ID)) {
                        CombatOutcome attackOutcome = CombatResolver.ResolveCombatFromPreview(state.Combat.CombatPreview);
                        // Debug.Log($"InputManager.HandleEntityTileSelection: {state.Combat.CombatPreview}");
                        CombatCinematicsDelegates.GetDirector().InitializeCinematicData(actingEntity, targetEntity, attackOutcome);
                        GameStateDelegates.InvokeOnPlayerPhaseStateChanged(CombatStateEnums.PlayerPhaseState.UnitAttackCutscene);
                        ServiceLocator.Get<AudioManager>().PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/select_004"));
                        break;
                    }
                    ServiceLocator.Get<AudioManager>().PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/pluck_001"));
                    break;
                default: throw new Exception($"InputManager.HandleSelectionInput : Unexpected player phase state for entity tile selection : {state.Combat.PlayerPhase}");
            }
        }
        
        private void ReinstateGridCursorPosition(Vector2Int? position) {
            OnGridCursorMove(position ?? GridCursorPosition, Vector2Int.zero);
        }
        private void HandleCombatControls() {
            GameStateData.GameStateDatagram currentState = GameStateDelegates.GetCurrentGameState();
            switch (currentState.Combat.TurnPhase) {
                case CombatStateEnums.TurnPhase.Player:
                    HandleDangerZoneToggle();
                    HandleCycleInput();
                    break;
            }
        }
        private void HandleCycleInput() {
            GameStateData.GameStateDatagram currentState = GameStateDelegates.GetCurrentGameState();
            switch (currentState.Combat.PlayerPhase) {
                case CombatStateEnums.PlayerPhaseState.SelectUnitToControl:
                    var playerUnitsDeque = currentState.Combat.PlayersCycleDeque;
                    if (playerUnitsDeque.Count == 0)
                        return;
                    if (_cycleRightAction.WasPerformedThisFrame()) {
                        Debug.Log("InputManager.HandleCycleInput: Cycling right");
                        int first = playerUnitsDeque.First(); // A
                        playerUnitsDeque.RemoveFirst(); // [B C D]
                        playerUnitsDeque.AddLast(first); // [B C D A]
                        int newID = playerUnitsDeque.First(); // B
                        currentState.Combat.InspectedEntityID = newID;
                        GridCursorPosition = EntityDelegates.GetGridEntityByID(newID).GridPosition;
                        GridDelegates.SetInspectedTile(GridCursorPosition);
                    } else if (_cycleLeftAction.WasPerformedThisFrame()) {
                        Debug.Log("InputManager.HandleCycleInput: Cycling left");
                        int last = playerUnitsDeque.Last(); // D
                        playerUnitsDeque.RemoveLast(); // [A B C]
                        playerUnitsDeque.AddFirst(last); // [D A B C]
                        int newID = playerUnitsDeque.First(); // D
                        currentState.Combat.InspectedEntityID = newID;
                        GridCursorPosition = EntityDelegates.GetGridEntityByID(newID).GridPosition;
                        GridDelegates.SetInspectedTile(GridCursorPosition);
                    }
                    break;
                case CombatStateEnums.PlayerPhaseState.UnitSelectTarget:
                    var enemyTargetsDeque = currentState.Combat.EnemiesCycleDeque;
                    if (enemyTargetsDeque.Count == 0)
                        return;
                    if (_cycleRightAction.WasPerformedThisFrame()) {
                        Debug.Log("InputManager.HandleCycleInput: Cycling right");
                        int first = enemyTargetsDeque.First(); // A
                        enemyTargetsDeque.RemoveFirst(); // [B C D]
                        enemyTargetsDeque.AddLast(first); // [B C D A]
                        int newID = enemyTargetsDeque.First(); // B
                        currentState.Combat.InspectedEntityID = newID;
                        GridCursorPosition = EntityDelegates.GetGridEntityByID(newID).GridPosition;
                        GridDelegates.SetInspectedTile(GridCursorPosition);
                    } else if (_cycleLeftAction.WasPerformedThisFrame()) {
                        Debug.Log("InputManager.HandleCycleInput: Cycling left");
                        int last = enemyTargetsDeque.Last(); // D
                        enemyTargetsDeque.RemoveLast(); // [A B C]
                        enemyTargetsDeque.AddFirst(last); // [D A B C]
                        int newID = enemyTargetsDeque.First(); // D
                        currentState.Combat.InspectedEntityID = newID;
                        GridCursorPosition = EntityDelegates.GetGridEntityByID(newID).GridPosition;
                        GridDelegates.SetInspectedTile(GridCursorPosition);
                    }
                    break;
            }
        }
        private void HandleDangerZoneToggle() {
            if (!_dangerZoneAction.WasPerformedThisFrame())
                return;
            _isDangerZoneVisible = !_isDangerZoneVisible;
            GridDelegates.InvokeOnSetDangerZoneVisibility(_isDangerZoneVisible);
        }
        private void HandleFastForward() {
            return;
            GameStateData.GameStateDatagram currentState = GameStateDelegates.GetCurrentGameState();
            if (selectAction.IsPressed()) {
                Time.timeScale = 4f;
            }
            if (selectAction.WasReleasedThisFrame()) {
                Time.timeScale = 1f;
            }
        }
        private void ManualSetGridCursorPosition(Vector2Int coordinate) {
            GridCursorPosition = coordinate;
        }
    }
}