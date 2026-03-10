using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using StrategyGame.Audio;
using StrategyGame.Combat;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Core.GameState;
using StrategyGame.Factions;
using StrategyGame.Grid;
using StrategyGame.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace StrategyGame.Core.Input {
    public class InputManager : MonoBehaviour, IPointerMoveHandler {
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        [SerializeField] private MouseInputRaycaster gridMouseInputRaycaster;
        [SerializeField] private CameraRigController cameraRigController;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private GridCursorRenderer gridCursorRenderer;
        [Header("Path Selection Settings")] [SerializeField]
        private float pathSelectionMoveActionCooldown = 0.33f;
        [SerializeField] private float pathSelectionMoveActionCooldownAccelerationThreshold = 1f;
        [SerializeField] private float pathSelectionMoveActionMinimumCooldown = .08f;
        [SerializeField] private float pathSelectionMoveActionCooldownAccelerationRate = .05f;
        [Header("UI Selection Settings")] [SerializeField]
        private float uiSelectionHoldInitialDelay = .33f;
        [SerializeField] private float uiSelectionHoldRepeatRate = .1f;
        private InputAction _moveAction;
        private InputAction _selectAction;
        private InputAction _cancelAction;
        private InputAction _dangerZoneAction;
        private InputAction _cycleLeftAction;
        private InputAction _cycleRightAction;
        private float _pathSelectionMoveActionTimer;
        private float _currentPathSelectionMoveActionCooldown;
        private float _pathSelectionMoveActionHeldDuration;
        private float _uiSelectionHoldTimer;
        private float _uiSelectionNextRepeatTimer;
        private bool _isDiagonalMoveEnabled = true;
        private bool _isDangerZoneVisible = false;
        private Vector2Int _gridCursorPosition;
        public Vector2Int GridCursorPosition {
            get => _gridCursorPosition;
            set {
                _gridCursorPosition = value;
                gridCursorRenderer.MoveTo(_gridCursorPosition);
            }
        }
        private string _cachedScheme;

        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        private void InitializeActions() {
            var actions = playerInput.currentActionMap.asset;
            _moveAction = actions.FindAction("Move", true);
            _selectAction = actions.FindAction("Select", true);
            _cancelAction = actions.FindAction("Cancel", true);
            _dangerZoneAction = actions.FindAction("DangerZone", true);
            _cycleLeftAction = actions.FindAction("CycleLeft", true);
            _cycleRightAction = actions.FindAction("CycleRight", true);
            Debug.Log("InputManager.InitializeActions: Input actions initialized");
        }
        private void OnControlsChanged(PlayerInput obj) {
            // Only happens when the device actually changes!
            _cachedScheme = obj.currentControlScheme;
            Debug.Log($"Switching to: {_cachedScheme}");

            InitializeActions();
        }


        private void OnEnable() {
            playerInput.actions.Enable();
            playerInput.onControlsChanged += OnControlsChanged;
            InitializeActions();
            InputDelegates.OnSetMouseRaycastEnabled += SetMouseRaycastEnabled;
            InputDelegates.OnReinstateGridCursorPosition += ReinstateGridCursorPosition;
            
            InputDelegates.GetUIManager = () => this;
            InputDelegates.GetGridCursorPosition = () => GridCursorPosition;
            InputDelegates.GetDangerZoneVisible = () => _isDangerZoneVisible;
        }
        private void OnDisable() {
            playerInput.actions.Disable();
            playerInput.onControlsChanged -= OnControlsChanged;
            InputDelegates.OnSetMouseRaycastEnabled -= SetMouseRaycastEnabled;
            InputDelegates.OnReinstateGridCursorPosition -= ReinstateGridCursorPosition;
            
            InputDelegates.GetUIManager = null;
            InputDelegates.GetGridCursorPosition = null;
            InputDelegates.GetDangerZoneVisible = null;
            
        }
        private void Start() {
            GameStateEnums.UnitMoveSelectionMode currentUnitMoveSelectionMode = GameStateDelegates.GetCurrentGameState().Combat.UnitMoveSelectionMode;
            gridMouseInputRaycaster.enabled = currentUnitMoveSelectionMode == GameStateEnums.UnitMoveSelectionMode.Automatic;
            cameraRigController.SetPanningEnabled(currentUnitMoveSelectionMode == GameStateEnums.UnitMoveSelectionMode.Automatic);
            // cameraRigController.SetZoomingEnabled(currentUnitMoveSelectionMode == UnitMoveSelectionMode.Automatic);
        }
        private void Update() {
            HandleCancellationInput();
            HandleInteractionInput();
            HandleAxisInput();
            HandleCombatControls();
            HandleFastForward();
        }
        private void OnDestroy() {
            gridMouseInputRaycaster.enabled = false;
        }

        // ==============================
        // CORE METHODS
        // ==============================
        private void SetMouseRaycastEnabled(bool value) {
            gridMouseInputRaycaster.enabled = value;
        }
        private void HandleAxisInput() {
            _pathSelectionMoveActionTimer = Mathf.Max(0f, _pathSelectionMoveActionTimer - Time.deltaTime);
            Vector2 moveInput = _moveAction.ReadValue<Vector2>();
            GameStateData currentGameState = GameStateDelegates.GetCurrentGameState();
            if (currentGameState.Combat.TurnPhase != GameStateEnums.TurnPhase.Player)
                return;
            switch (currentGameState.Combat.PlayerPhase) {
                case GameStateEnums.PlayerPhaseState.SelectUnitToControl: HandleGridNavigationInput(moveInput); break;
                case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination: HandleGridNavigationInput(moveInput); break;
                case GameStateEnums.PlayerPhaseState.UnitMovingToDestination: break;
                case GameStateEnums.PlayerPhaseState.UnitActionMenu: HandleActionMenuInput(moveInput); break;
                case GameStateEnums.PlayerPhaseState.UnitSelectTarget: HandleGridNavigationInput(moveInput); break;
                case GameStateEnums.PlayerPhaseState.UnitAttackCutscene: break;
                case GameStateEnums.PlayerPhaseState.None: break;
                default: throw new Exception("InputManager.HandleAxisInput: Invalid player phase state enum!");
            }
        }
        private void HandleInteractionInput() {
            GameStateData currentGameState = GameStateDelegates.GetCurrentGameState();
            switch (currentGameState.Combat.TurnPhase) {
                case GameStateEnums.TurnPhase.Player:
                    switch (currentGameState.Combat.PlayerPhase) {
                        case GameStateEnums.PlayerPhaseState.SelectUnitToControl: HandleEntityTileSelection(); break;
                        case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination: HandleEntityTileSelection(); break;
                        case GameStateEnums.PlayerPhaseState.UnitMovingToDestination: 
                            // HandleFastForward(); 
                            break;
                        case GameStateEnums.PlayerPhaseState.UnitActionMenu: HandleUIConfirmation(); break;
                        case GameStateEnums.PlayerPhaseState.UnitSelectTarget: HandleEntityTileSelection(); break;
                        case GameStateEnums.PlayerPhaseState.UnitAttackCutscene: 
                            // HandleFastForward(); 
                            break;
                        case GameStateEnums.PlayerPhaseState.None: break;
                        default: throw new Exception("InputManager.HandleInteractionInput: Invalid player phase state enum!");
                    }
                    break;
                case GameStateEnums.TurnPhase.Enemy: 
                    // HandleFastForward(); 
                    break;
                case GameStateEnums.TurnPhase.Event:
                    // HandleFastForward();
                    break;
                case GameStateEnums.TurnPhase.None: break;
                default: throw new Exception("InputManager.HandleInteractionInput: Invalid turn phase state enum!");
            }
        }
        private void HandleCancellationInput() {
            if (!_cancelAction.WasPerformedThisFrame())
                return;
            GameStateData currentGameState = GameStateDelegates.GetCurrentGameState();
            switch (currentGameState.Combat.TurnPhase) {
                case GameStateEnums.TurnPhase.Player:
                    switch (currentGameState.Combat.PlayerPhase) {
                        case GameStateEnums.PlayerPhaseState.SelectUnitToControl: break;
                        case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination:
                            GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.SelectUnitToControl);
                            _isDiagonalMoveEnabled = true;
                            break;
                        case GameStateEnums.PlayerPhaseState.UnitMovingToDestination: break;
                        case GameStateEnums.PlayerPhaseState.UnitActionMenu: 
                            // Delegate task to CombatActionMenuController
                            InputDelegates.InvokeOnCancelPressed();
                            break;
                        case GameStateEnums.PlayerPhaseState.UnitSelectTarget:
                            currentGameState.Combat.InspectedEntityID = currentGameState.Combat.SelectedEntityID;
                            GridDelegates.SetInspectedTile(EntityDelegates.GetGridEntityByID(currentGameState.Combat.SelectedEntityID).GridPosition);
                            GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.UnitActionMenu);
                            AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/minimize_008"));
                            break;
                        case GameStateEnums.PlayerPhaseState.UnitAttackCutscene: break;
                        case GameStateEnums.PlayerPhaseState.None: break;
                        default: throw new Exception("InputManager.HandleCancellationInput: Invalid player phase state enum!");
                    }
                    break;
                case GameStateEnums.TurnPhase.Enemy: break;
                case GameStateEnums.TurnPhase.Event: break;
                case GameStateEnums.TurnPhase.None: break;
                default: throw new Exception("InputManager.HandleCancellationInput: Invalid turn phase state enum!");
            }
        }

        // ==============================
        // IMPLEMENTED METHODS
        // ==============================
        public void OnPointerMove(PointerEventData eventData) {
            // GameStateDelegates.InvokeOnUnitMoveSelectionChanged(GameStateManager.UnitMoveSelectionMode.Automatic);
        }
        public void OnGridCursorMove(Vector2Int originalPosition, Vector2Int moveVector) {
            GameStateData currentGameState = GameStateDelegates.GetCurrentGameState();
            Vector2Int gridDimensions = GridDelegates.GetGridDimensions();
            Vector2Int newPosition = originalPosition + moveVector;
            Vector2Int? bestTilePosition = null;
            if (moveVector == Vector2Int.zero) {
                GridCursorPosition = originalPosition;
                GridDelegates.SetInspectedTile(GridCursorPosition);
                return;
            }
            if (currentGameState.Combat.PlayerPhase == GameStateEnums.PlayerPhaseState.UnitSelectTarget) {
                // Choose best newPosition
                float lowestPenalty = float.MaxValue;
                GridEntity actorEntity = EntityDelegates.GetGridEntityByID(currentGameState.Combat.SelectedEntityID);
                AbilityData currentSkill = DataDelegates.GetAbilityDataByID(currentGameState.Combat.CurrentSelectedSkillID);
                if (currentSkill == null) currentSkill = actorEntity.BasicAttack;
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
        private void HandleGridNavigationInput(Vector2 moveInput) {
            Vector2Int moveDirection = new Vector2Int(moveInput.x > .5f ? 1 :
                moveInput.x < -.5f ? -1 : 0, moveInput.y > .5f ? 1 :
                moveInput.y < -.5f ? -1 : 0);
            if (_moveAction.WasReleasedThisFrame()) {
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
        private void HandleActionMenuInput(Vector2 moveInput) {
            int vertical = 0;
            if (moveInput.y > 0.5f)
                vertical = 1;
            if (moveInput.y < -0.5f)
                vertical = -1;
            void Move() {
                if (vertical == 1 || vertical == -1) AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/scratch_001"), volumeMultiplier:.5f);
                if (vertical == 1) {
                    InputDelegates.InvokeOnUpPressed();
                } else if (vertical == -1) {
                    InputDelegates.InvokeOnDownPressed();
                }
            }

            // No direction -> reset
            if (vertical == 0) {
                _uiSelectionHoldTimer = 0f;
                _uiSelectionNextRepeatTimer = uiSelectionHoldInitialDelay;
                return;
            }

            // New press (axis went from 0 -> non-zero)
            if (_moveAction.WasPressedThisFrame()) {
                Move();
                _uiSelectionHoldTimer = 0f;
                _uiSelectionNextRepeatTimer = uiSelectionHoldInitialDelay;
                return;
            }

            // Held
            _uiSelectionHoldTimer += Time.unscaledDeltaTime;
            if (_uiSelectionHoldTimer >= _uiSelectionNextRepeatTimer) {
                Move();
                _uiSelectionNextRepeatTimer += uiSelectionHoldRepeatRate;
            }
        }
        private void HandleEntityTileSelection() {
            if (!_selectAction.WasPressedThisFrame())
                return;
            GameStateData state = GameStateDelegates.GetCurrentGameState();
            switch (state.Combat.PlayerPhase) {
                // In order to select, there must be an entity
                case GameStateEnums.PlayerPhaseState.SelectUnitToControl when state.Combat.InspectedEntityID == -1:
                    Debug.Log("InputManager.HandleSelectionInput: Current selected entity is null!");
                    return;
                case GameStateEnums.PlayerPhaseState.SelectUnitToControl:
                    // Disallow if the unit's ID is not in ActorIDsRemaining list.
                    if (!state.Combat.ActorIDsRemaining.Contains(state.Combat.InspectedEntityID)) {
                        Debug.Log("InputManager.HandleEntityTileSelection: The currently inspected entity needs to wait for their turn phase or has already acted!");
                        return;
                    }
                    Debug.Log("InputManager.HandleEntityTileSelection: START FORMING PATH");
                    _isDiagonalMoveEnabled = false;
                    GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination);
                    AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/select_005"));
                    break;
                case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination: {
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
                    Debug.Log($"InputManager.HandleSelectionInput: Manual path tiles: {string.Join(", ", manualPathList)} | Selected entity movement range: {currentSelectedEntity.MovementRange} | Manual path set: {string.Join(", ", manualPathSet)}");
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
                            GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.UnitMovingToDestination);
                        } else if (conditionsNeededToMoveToDestination) {
                            // Move unit to destination
                            Debug.Log($"ConditionsNeededToMoveToDestination is true");
                            currentSelectedEntity.MoveAlongPath(manualPathList);
                            GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.UnitMovingToDestination);
                        } else {
                            Debug.Log($"InputManager.HandleEntityTileSelection: conditionsNeededToDirectlyAttackTarget is {conditionsNeededToDirectlyAttackTarget} and conditionsNeededToMoveToDestination is {conditionsNeededToMoveToDestination}");
                        }
                    } else {
                        Debug.Log($"InputManager.HandleSelectionInput: Current manual path is not allowed!");
                        // GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.SelectUnitToControl);
                    }
                    break;
                }
                case GameStateEnums.PlayerPhaseState.UnitSelectTarget:
                    GridEntity actingEntity = EntityDelegates.GetGridEntityByID(state.Combat.SelectedEntityID);
                    GridEntity targetEntity = GridDelegates.GetTileFromPosition(state.Combat.InspectedTilePosition).Occupant;
                    // Perform action on target
                    // Retrieve ability data from game state
                    AbilityData skillData = DataDelegates.GetAbilityDataByID(state.Combat.CurrentSelectedSkillID);
                    if (skillData == null) skillData = actingEntity.BasicAttack;
                   

                    if (targetEntity == null) {
                        Debug.LogWarning("InputManager.HandleEntityTileSelection: Exiting switch case early because targetEntity is null");
                        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/pluck_001"));
                        break;
                    }

                    bool targetIsAlly = actingEntity.IsFriendlyWith(targetEntity);

                    if ((skillData.CanTargetAllies && targetIsAlly) || (skillData.CanTargetEnemies && !targetIsAlly) || (skillData.CanTargetSelf && actingEntity.ID == targetEntity.ID)) {
                        
                        CombatOutcome attackOutcome = CombatResolver.ResolveCombatFromPreview(state.Combat.CombatPreview);
                        CombatCinematicsDelegates.GetDirector().InitializeCinematicData(actingEntity, targetEntity, attackOutcome);
                        GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.UnitAttackCutscene);
                        AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/select_004"));
                        break;
                    }
                    AudioManager.Instance.PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/pluck_001"));
                
                    break;
                default: throw new Exception($"InputManager.HandleSelectionInput : Unexpected player phase state for entity tile selection : {state.Combat.PlayerPhase}");
            }
        }
        private void HandleUIConfirmation() {
            if (!_selectAction.WasPressedThisFrame())
                return;
            // For now, we are assuming we are in UnitActionMenu state
            InputDelegates.InvokeOnConfirmPressed();
        }
        private void ReinstateGridCursorPosition(Vector2Int? position) {
            OnGridCursorMove(position ?? GridCursorPosition, Vector2Int.zero);
        }
        private void HandleCombatControls() {
            GameStateData currentState = GameStateDelegates.GetCurrentGameState();
            switch (currentState.Combat.TurnPhase) {
                case GameStateEnums.TurnPhase.Player:
                    HandleDangerZoneToggle();
                    HandleCycleInput();
                    break;
            }
        }
        private void HandleCycleInput() {
            GameStateData currentState = GameStateDelegates.GetCurrentGameState();
            switch (currentState.Combat.PlayerPhase) {
                case GameStateEnums.PlayerPhaseState.SelectUnitToControl:
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
                case GameStateEnums.PlayerPhaseState.UnitSelectTarget:
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
            GameStateData currentState = GameStateDelegates.GetCurrentGameState();
            if (_selectAction.IsPressed()) {
                Time.timeScale = 4f;
            } 
            if (_selectAction.WasReleasedThisFrame()) {
                Time.timeScale = 1f;
            }
        }
        
        
        private void ManualSetGridCursorPosition(Vector2Int coordinate) {
            GridCursorPosition = coordinate;
        }

        
    }
}
