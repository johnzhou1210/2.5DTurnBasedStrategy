using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using StrategyGame.Combat;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Core.GameState;
using StrategyGame.Factions;
using StrategyGame.Grid;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace StrategyGame.Core.Input
{
    public class InputManager : MonoBehaviour, IPointerMoveHandler
    {
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        [SerializeField] private MouseInputRaycaster gridMouseInputRaycaster;
        [SerializeField] private CameraRigController cameraRigController;
        [SerializeField] private PlayerInput playerInput;

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


        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        private void Awake()
        {
            _moveAction = playerInput.actions["Move"];
            _selectAction = playerInput.actions["Select"];
            _cancelAction = playerInput.actions["Cancel"];
            _dangerZoneAction = playerInput.actions["DangerZone"];
            _cycleLeftAction = playerInput.actions["CycleLeft"];
            _cycleRightAction = playerInput.actions["CycleRight"];
        }

        private void OnEnable()
        {
            InputDelegates.OnSetMouseRaycastEnabled += SetMouseRaycastEnabled;
            InputDelegates.OnReinstateGridCursorPosition += ReinstateGridCursorPosition;
            InputDelegates.GetUIManager = () => this;
            InputDelegates.GetGridCursorPosition = () => _gridCursorPosition;
            InputDelegates.GetDangerZoneVisible = () => _isDangerZoneVisible;
        }

        private void OnDisable()
        {
            InputDelegates.OnSetMouseRaycastEnabled -= SetMouseRaycastEnabled;
            InputDelegates.OnReinstateGridCursorPosition -= ReinstateGridCursorPosition;
            InputDelegates.GetUIManager = null;
            InputDelegates.GetGridCursorPosition = null;
            InputDelegates.GetDangerZoneVisible = null;
        }

        private void Start()
        {
            GameStateEnums.UnitMoveSelectionMode currentUnitMoveSelectionMode =
                GameStateDelegates.GetCurrentGameState().Combat.UnitMoveSelectionMode;
            gridMouseInputRaycaster.enabled =
                currentUnitMoveSelectionMode == GameStateEnums.UnitMoveSelectionMode.Automatic;
            cameraRigController.SetPanningEnabled(currentUnitMoveSelectionMode ==
                                                  GameStateEnums.UnitMoveSelectionMode.Automatic);
            // cameraRigController.SetZoomingEnabled(currentUnitMoveSelectionMode == UnitMoveSelectionMode.Automatic);
        }

        private void Update()
        {
            HandleCancellationInput();
            HandleInteractionInput();
            HandleAxisInput();
            HandleCombatControls();
        }


        private void OnDestroy()
        {
            gridMouseInputRaycaster.enabled = false;
        }


        // ==============================
        // CORE METHODS
        // ==============================
        private void SetMouseRaycastEnabled(bool value)
        {
            gridMouseInputRaycaster.enabled = value;
        }

        private void HandleAxisInput()
        {
            _pathSelectionMoveActionTimer = Mathf.Max(0f, _pathSelectionMoveActionTimer - Time.deltaTime);
            Vector2 moveInput = _moveAction.ReadValue<Vector2>();
            GameStateData currentGameState = GameStateDelegates.GetCurrentGameState();
            if (currentGameState.Combat.TurnPhase != GameStateEnums.TurnPhase.Player) return;
            switch (currentGameState.Combat.PlayerPhase)
            {
                case GameStateEnums.PlayerPhaseState.SelectUnitToControl:
                    HandleGridNavigationInput(moveInput);
                    break;
                case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination:
                    HandleGridNavigationInput(moveInput);
                    break;
                case GameStateEnums.PlayerPhaseState.UnitMovingToDestination:
                    break;
                case GameStateEnums.PlayerPhaseState.UnitActionMenu:
                    HandleActionMenuInput(moveInput);
                    break;
                case GameStateEnums.PlayerPhaseState.UnitSelectTarget:
                    break;
                case GameStateEnums.PlayerPhaseState.UnitAttackCutscene:
                    break;
                case GameStateEnums.PlayerPhaseState.None:
                    break;
                default:
                    throw new Exception("InputManager.HandleAxisInput: Invalid player phase state enum!");
            }
        }

        private void HandleInteractionInput()
        {
            GameStateData currentGameState = GameStateDelegates.GetCurrentGameState();
            switch (currentGameState.Combat.TurnPhase) {
                case GameStateEnums.TurnPhase.Player:
                    switch (currentGameState.Combat.PlayerPhase)
                    {
                        case GameStateEnums.PlayerPhaseState.SelectUnitToControl:
                            HandleEntityTileSelection();
                            break;
                        case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination:
                            HandleEntityTileSelection();
                            break;
                        case GameStateEnums.PlayerPhaseState.UnitMovingToDestination:
                            break;
                        case GameStateEnums.PlayerPhaseState.UnitActionMenu:
                            HandleUIConfirmation();
                            break;
                        case GameStateEnums.PlayerPhaseState.UnitSelectTarget:
                            HandleEntityTileSelection();
                            break;
                        case GameStateEnums.PlayerPhaseState.UnitAttackCutscene:
                            break;
                        case GameStateEnums.PlayerPhaseState.None:
                            break;
                        default:
                            throw new Exception("InputManager.HandleInteractionInput: Invalid player phase state enum!");
                    }
                    break;
                case GameStateEnums.TurnPhase.Enemy:
                    break;
                case GameStateEnums.TurnPhase.Event:
                    break;
                case GameStateEnums.TurnPhase.None:
                    break;
                default:
                    throw new Exception("InputManager.HandleInteractionInput: Invalid turn phase state enum!");
            }
        }

        private void HandleCancellationInput() {
            if (!_cancelAction.WasPerformedThisFrame()) return;
            GameStateData currentGameState = GameStateDelegates.GetCurrentGameState();
            switch (currentGameState.Combat.TurnPhase) {
                case GameStateEnums.TurnPhase.Player:
                    switch (currentGameState.Combat.PlayerPhase) {
                        case GameStateEnums.PlayerPhaseState.SelectUnitToControl:
                            break;
                        case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination:
                            GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.SelectUnitToControl);
                            _isDiagonalMoveEnabled = true;
                            break;
                        case GameStateEnums.PlayerPhaseState.UnitMovingToDestination:
                            break;
                        case GameStateEnums.PlayerPhaseState.UnitActionMenu:
                            break;
                        case GameStateEnums.PlayerPhaseState.UnitSelectTarget:
                            currentGameState.Combat.InspectedEntityID = currentGameState.Combat.SelectedEntityID;
                            GridDelegates.SetInspectedTile(EntityDelegates.GetGridEntityByID(currentGameState.Combat.SelectedEntityID).GridPosition);
                            GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.UnitActionMenu);
                            break;
                        case GameStateEnums.PlayerPhaseState.UnitAttackCutscene:
                            break;
                        case GameStateEnums.PlayerPhaseState.None:
                            break;
                        default:
                            throw new Exception("InputManager.HandleCancellationInput: Invalid player phase state enum!");
                    }
                    break;
                case GameStateEnums.TurnPhase.Enemy:
                    break;
                case GameStateEnums.TurnPhase.Event:
                    break;
                case GameStateEnums.TurnPhase.None:
                    break;
                default:
                    throw new Exception("InputManager.HandleCancellationInput: Invalid turn phase state enum!");
            }
        }

        // ==============================
        // IMPLEMENTED METHODS
        // ==============================
        public void OnPointerMove(PointerEventData eventData)
        {
            // GameStateDelegates.InvokeOnUnitMoveSelectionChanged(GameStateManager.UnitMoveSelectionMode.Automatic);
        }

        public void OnGridCursorMove(Vector2Int newPosition)
        {
            Vector2Int gridDimensions = GridDelegates.GetGridDimensions();
            Vector2Int oldGridCursorPosition = _gridCursorPosition;
            _gridCursorPosition = new Vector2Int(Math.Clamp(newPosition.x, 0, gridDimensions.x - 1),
                Math.Clamp(newPosition.y, 0, gridDimensions.y - 1));
            bool success = GridDelegates.SetInspectedTile(_gridCursorPosition);
            if (!success)
            {
                _gridCursorPosition = oldGridCursorPosition;
            }
        }


        // ==============================
        // HELPERS
        // ==============================
        private void HandleGridNavigationInput(Vector2 moveInput)
        {
            Vector2Int moveDirection = new Vector2Int(
                moveInput.x > .5f ? 1 : moveInput.x < -.5f ? -1 : 0,
                moveInput.y > .5f ? 1 : moveInput.y < -.5f ? -1 : 0
            );
            if (_moveAction.WasReleasedThisFrame())
            {
                _pathSelectionMoveActionTimer = 0f;
            }

            if (moveDirection != Vector2.zero)
            {
                _pathSelectionMoveActionHeldDuration += Time.deltaTime;
                if (_pathSelectionMoveActionHeldDuration > pathSelectionMoveActionCooldownAccelerationThreshold)
                {
                    float extraHeldTime = _pathSelectionMoveActionHeldDuration -
                                          pathSelectionMoveActionCooldownAccelerationThreshold;
                    float acceleratedCooldown = _currentPathSelectionMoveActionCooldown -
                                                extraHeldTime * pathSelectionMoveActionCooldownAccelerationRate;
                    _currentPathSelectionMoveActionCooldown =
                        Mathf.Max(pathSelectionMoveActionMinimumCooldown, acceleratedCooldown);
                }

                if (_pathSelectionMoveActionTimer > 0f) return;
                _pathSelectionMoveActionTimer = _currentPathSelectionMoveActionCooldown;
                Vector2Int moveVector = moveDirection;
                if (!_isDiagonalMoveEnabled && moveVector.x != 0 && moveVector.y != 0)
                {
                    moveVector.y = 0;
                }

                OnGridCursorMove(_gridCursorPosition + moveVector);
            }
            else
            {
                _pathSelectionMoveActionHeldDuration = 0f;
                _currentPathSelectionMoveActionCooldown = Mathf.Lerp(_currentPathSelectionMoveActionCooldown,
                    pathSelectionMoveActionCooldown, Time.deltaTime * 5f);
            }
        }

        private void HandleActionMenuInput(Vector2 moveInput)
        {
            int vertical = 0;
            if (moveInput.y > 0.5f) vertical = 1;
            if (moveInput.y < -0.5f) vertical = -1;

            void Move()
            {
                if (vertical == 1)
                {
                    InputDelegates.InvokeOnUpPressed();
                }
                else if (vertical == -1)
                {
                    InputDelegates.InvokeOnDownPressed();
                }
            }

            // No direction -> reset
            if (vertical == 0)
            {
                _uiSelectionHoldTimer = 0f;
                _uiSelectionNextRepeatTimer = uiSelectionHoldInitialDelay;
                return;
            }

            // New press (axis went from 0 -> non-zero)
            if (_moveAction.WasPressedThisFrame())
            {
                Move();
                _uiSelectionHoldTimer = 0f;
                _uiSelectionNextRepeatTimer = uiSelectionHoldInitialDelay;
                return;
            }

            // Held
            _uiSelectionHoldTimer += Time.unscaledDeltaTime;
            if (_uiSelectionHoldTimer >= _uiSelectionNextRepeatTimer)
            {
                Move();
                _uiSelectionNextRepeatTimer += uiSelectionHoldRepeatRate;
            }
        }


        private void HandleEntityTileSelection()
        {
            if (!_selectAction.WasPressedThisFrame()) return;
            GameStateData state = GameStateDelegates.GetCurrentGameState();
            switch (state.Combat.PlayerPhase)
            {
                // In order to select, there must be an entity
                case GameStateEnums.PlayerPhaseState.SelectUnitToControl when state.Combat.InspectedEntityID == -1:
                    Debug.Log("InputManager.HandleSelectionInput: Current selected entity is null!");
                    return;
                case GameStateEnums.PlayerPhaseState.SelectUnitToControl:
                    // Disallow if the unit's ID is not in ActorIDsRemaining list.
                    if (!state.Combat.ActorIDsRemaining.Contains(state.Combat.InspectedEntityID))
                    {
                        Debug.Log(
                            "InputManager.HandleEntityTileSelection: The currently inspected entity needs to wait for their turn phase or has already acted!");
                        return;
                    }

                    Debug.Log("InputManager.HandleEntityTileSelection: START FORMING PATH");
                    _isDiagonalMoveEnabled = false;
                    GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState
                        .SelectUnitMoveDestination);
                    break;
                case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination:
                {
                    GridEntity currentSelectedEntity = EntityDelegates.GetGridEntityByID(state.Combat.SelectedEntityID);
                    if (currentSelectedEntity == null)
                    {
                        Debug.LogWarning("InputManager.HandleSelectionInput: Current selected entity is null!");
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
                    bool isDestinationValid = manualPathList.Count - 1 <= currentSelectedEntity.MovementRange &&
                        manualPathList.ToHashSet().IsSubsetOf(manualPathSet) &&
                        manualPathList[^1].Occupant == null || manualPathList[^1].Occupant == currentSelectedEntity;
                    Debug.Log($"Condition1: {manualPathList.Count - 1 <= currentSelectedEntity.MovementRange}, Condition2: {manualPathList.ToHashSet().IsSubsetOf(manualPathSet)}, Condition3: {manualPathList[^1].Occupant == null || manualPathList[^1].Occupant == currentSelectedEntity}");
                    if (isDestinationValid)
                    {
                        // Move unit to destination
                        currentSelectedEntity.MoveAlongPath(manualPathList);
                        GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.UnitMovingToDestination);
                    }
                    else
                    {
                        Debug.Log($"InputManager.HandleSelectionInput: Current manual path is not allowed!");
                        // GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.SelectUnitToControl);
                    }

                    break;
                }
                case GameStateEnums.PlayerPhaseState.UnitSelectTarget:
                    // Perform action on target
                    // For now, assume attack
                    GridEntity actingEntity = EntityDelegates.GetGridEntityByID(state.Combat.SelectedEntityID);
                    GridEntity targetEntity = GridDelegates.GetTileFromPosition(state.Combat.InspectedTilePosition).Occupant;
                    if (targetEntity == null) throw new Exception("InputManager.HandleEntityTileSelection: targetEntity is null!");
                    
                    Debug.Log($"Attacker weapon: {actingEntity.Weapon}");
                    Debug.Log($"Defender weapon: {targetEntity.Weapon}");
                    
                    HashSet<GridEntity> entitiesWithinDefenderRange = targetEntity.GetAttackableEntitiesAtPosition(targetEntity.GridPosition);
                    bool attackerInDefenderRange = entitiesWithinDefenderRange.Any(e => e.ID == actingEntity.ID);
                    
                    CombatOutcome attackOutcome = CombatResolver.SimulateAttack(new CombatStats {
                        HP = actingEntity.Health,
                        Attack = actingEntity.Attack,
                        Defense = actingEntity.Defense,
                        Agility = actingEntity.Agility,
                        Accuracy = actingEntity.Accuracy,
                        Resistance = actingEntity.Resistance,
                        Evasion = actingEntity.Evasion,
                        Weapon = actingEntity.Weapon,
                        EntityID = actingEntity.ID
                    }, new CombatStats {
                        HP = targetEntity.Health,
                        Attack = targetEntity.Attack,
                        Defense = targetEntity.Defense,
                        Agility = targetEntity.Agility,
                        Accuracy = targetEntity.Accuracy,
                        Resistance = targetEntity.Resistance,
                        Evasion = targetEntity.Evasion,
                        Weapon = targetEntity.Weapon,
                        EntityID = targetEntity.ID
                    }, Resources.Load<AbilityData>("ScriptableObjects/Abilities/Attack"), attackerInDefenderRange);
                    Debug.Log($"Attack outcome: {attackOutcome}");
                    break;
                default:
                    throw new Exception(
                        $"InputManager.HandleSelectionInput : Unexpected player phase state for entity tile selection : {state.Combat.PlayerPhase}");
            }
        }

        private void HandleUIConfirmation()
        {
            if (!_selectAction.WasPressedThisFrame()) return;
            // For now, we are assuming we are in UnitActionMenu state
            InputDelegates.InvokeOnConfirmPressed();
        }

        private void ReinstateGridCursorPosition() {
            OnGridCursorMove(_gridCursorPosition);
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
                    if (playerUnitsDeque.Count <= 1) return;
                    if (_cycleRightAction.WasPerformedThisFrame()) {
                        Debug.Log("InputManager.HandleCycleInput: Cycling right");
                        int first = playerUnitsDeque.First();  // A
                        playerUnitsDeque.RemoveFirst();        // [B C D]
                        playerUnitsDeque.AddLast(first);       // [B C D A]
                        int newID = playerUnitsDeque.First();  // B
                        currentState.Combat.InspectedEntityID = newID;
                        GridDelegates.SetInspectedTile(EntityDelegates.GetGridEntityByID(newID).GridPosition);
                    }
                    else if (_cycleLeftAction.WasPerformedThisFrame()) {
                        Debug.Log("InputManager.HandleCycleInput: Cycling left");
                        int last = playerUnitsDeque.Last();    // D
                        playerUnitsDeque.RemoveLast();         // [A B C]
                        playerUnitsDeque.AddFirst(last);       // [D A B C]
                        int newID = playerUnitsDeque.First();  // D
                        currentState.Combat.InspectedEntityID = newID;
                        GridDelegates.SetInspectedTile(EntityDelegates.GetGridEntityByID(newID).GridPosition);
                    }
                    break;
                case GameStateEnums.PlayerPhaseState.UnitSelectTarget:
                    var enemyTargetsDeque = currentState.Combat.EnemiesCycleDeque;
                    if (enemyTargetsDeque.Count <= 1) return;
                    if (_cycleRightAction.WasPerformedThisFrame()) {
                        Debug.Log("InputManager.HandleCycleInput: Cycling right");
                        int first = enemyTargetsDeque.First();  // A
                        enemyTargetsDeque.RemoveFirst();        // [B C D]
                        enemyTargetsDeque.AddLast(first);       // [B C D A]
                        int newID = enemyTargetsDeque.First();  // B
                        currentState.Combat.InspectedEntityID = newID;
                        GridDelegates.SetInspectedTile(EntityDelegates.GetGridEntityByID(newID).GridPosition);
                    }
                    else if (_cycleLeftAction.WasPerformedThisFrame()) {
                        Debug.Log("InputManager.HandleCycleInput: Cycling left");
                        int last = enemyTargetsDeque.Last();    // D
                        enemyTargetsDeque.RemoveLast();         // [A B C]
                        enemyTargetsDeque.AddFirst(last);       // [D A B C]
                        int newID = enemyTargetsDeque.First();  // D
                        currentState.Combat.InspectedEntityID = newID;
                        GridDelegates.SetInspectedTile(EntityDelegates.GetGridEntityByID(newID).GridPosition);
                    }
                    GridDelegates.InvokeOnSetTileVisualSelectionAnim(currentState.Combat.InspectedTilePosition, true);
                    break;
            }
            
        }

        
        private void HandleDangerZoneToggle() {
            if (!_dangerZoneAction.WasPerformedThisFrame()) return;
            _isDangerZoneVisible = !_isDangerZoneVisible;
            GridDelegates.InvokeOnSetDangerZoneVisibility(_isDangerZoneVisible);
        }
    }
}