using System;
using System.Threading.Tasks;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Core.GameState;
using StrategyGame.Grid;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace StrategyGame.Core.Input {
    public class InputManager : MonoBehaviour, IPointerMoveHandler {
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        [SerializeField] private MouseInputRaycaster gridMouseInputRaycaster;
        [SerializeField] private CameraRigController cameraRigController;
        [SerializeField] private PlayerInput playerInput;
        
        [Header("Path Selection Settings")]
        [SerializeField] private float pathSelectionMoveActionCooldown = 0.33f;
        [SerializeField] private float pathSelectionMoveActionCooldownAccelerationThreshold = 1f;
        [SerializeField] private float pathSelectionMoveActionMinimumCooldown = .08f;
        [SerializeField] private float pathSelectionMoveActionCooldownAccelerationRate = .05f;
        
        private InputAction _moveAction;
        private InputAction _selectAction;
        private float _pathSelectionMoveActionTimer;
        private float _currentPathSelectionMoveActionCooldown;
        private float _pathSelectionMoveActionHeldDuration;
        private bool _isDiagonalMoveEnabled = true;
        private Vector2Int _gridCursorPosition;
        
        
        
        
        
        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        private void Awake() {
            _moveAction = playerInput.actions["Move"];
            _selectAction = playerInput.actions["Select"];
        }

        private void OnEnable() {
            InputDelegates.OnSetMouseRaycastEnabled += SetMouseRaycastEnabled;
            InputDelegates.GetUIManager = () => this;
            InputDelegates.GetGridCursorPosition = () => _gridCursorPosition;
        }

        private void OnDisable() {
            InputDelegates.OnSetMouseRaycastEnabled -= SetMouseRaycastEnabled;
            InputDelegates.GetUIManager = null;
            InputDelegates.GetGridCursorPosition = null;
        }

        private void Start() {
            GameStateEnums.UnitMoveSelectionMode currentUnitMoveSelectionMode = GameStateDelegates.GetCurrentGameStateSnapshot().CurrentUnitMoveSelectionMode;
            gridMouseInputRaycaster.enabled = currentUnitMoveSelectionMode == GameStateEnums.UnitMoveSelectionMode.Automatic;
            cameraRigController.SetPanningEnabled(currentUnitMoveSelectionMode == GameStateEnums.UnitMoveSelectionMode.Automatic);
            // cameraRigController.SetZoomingEnabled(currentUnitMoveSelectionMode == UnitMoveSelectionMode.Automatic);
        }
        
        

        private void Update() {
            HandleSelectionInput();
            HandleMovementInput();
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
        private void HandleMovementInput() {
            _pathSelectionMoveActionTimer = Mathf.Max(0f, _pathSelectionMoveActionTimer - Time.deltaTime);
            Vector2 moveInput = _moveAction.ReadValue<Vector2>();
            Vector2Int moveDirection = new Vector2Int(
                moveInput.x > .5f ? 1 : moveInput.x < -.5f ? -1 : 0,
                moveInput.y > .5f ? 1 : moveInput.y < -.5f ? -1 : 0
            );
            if (_moveAction.WasReleasedThisFrame()) {
                _pathSelectionMoveActionTimer = 0f;
            }
            if (moveDirection != Vector2.zero) {
                // GameStateDelegates.InvokeOnUnitMoveSelectionChanged(GameStateManager.UnitMoveSelectionMode.Manual);
                _pathSelectionMoveActionHeldDuration += Time.deltaTime;
                if (_pathSelectionMoveActionHeldDuration > pathSelectionMoveActionCooldownAccelerationThreshold) {
                    float extraHeldTime = _pathSelectionMoveActionHeldDuration - pathSelectionMoveActionCooldownAccelerationThreshold;
                    float acceleratedCooldown = _currentPathSelectionMoveActionCooldown - extraHeldTime * pathSelectionMoveActionCooldownAccelerationRate;
                    _currentPathSelectionMoveActionCooldown = Mathf.Max(pathSelectionMoveActionMinimumCooldown, acceleratedCooldown);
                }
                if (_pathSelectionMoveActionTimer > 0f) return;
                _pathSelectionMoveActionTimer = _currentPathSelectionMoveActionCooldown;
                Vector2Int moveVector = moveDirection;
                if (!_isDiagonalMoveEnabled && moveVector.x != 0 && moveVector.y != 0) {
                    moveVector.y = 0;
                }
                
                
                
                OnGridCursorMove(_gridCursorPosition + moveVector);
            } else {
                _pathSelectionMoveActionHeldDuration = 0f;
                _currentPathSelectionMoveActionCooldown = Mathf.Lerp(_currentPathSelectionMoveActionCooldown, pathSelectionMoveActionCooldown, Time.deltaTime * 5f);
            }
        }
        private void HandleSelectionInput() {
            // if (GameStateDelegates.GetCurrentSelectedEntity() != null) {
            //     Debug.Log("Entity already selected");
            //     return;
            // }
            if (_selectAction.WasPressedThisFrame()) {  
                GameStateManager.GameStateSnapshot stateSnapshot = GameStateDelegates.GetCurrentGameStateSnapshot();
                if (stateSnapshot.CurrentPlayerPhaseState == GameStateEnums.PlayerPhaseState.SelectUnitToControl) {
                    // In order to select, there must be an entity
                    if (GameStateDelegates.GetCurrentInspectedEntity() == null) { Debug.LogWarning("InputManager | HandleSelectionInput : Current selected entity is null!");  return; }
                    Debug.Log("START FORMING PATH");
                    _isDiagonalMoveEnabled = false;
                    GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination);
                } else if (stateSnapshot.CurrentPlayerPhaseState== GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination) {
                    // TODO: Confirm chosen player path
                    Debug.Log("STOP FORMING PATH");
                    _isDiagonalMoveEnabled = true;
                    
                    bool isDestinationValid = true; // Change this later
                    GridEntity currentSelectedEntity = GameStateDelegates.GetCurrentSelectedEntity();
                    if (currentSelectedEntity == null) { Debug.LogWarning("InputManager | HandleSelectionInput : Current selected entity is null!"); return; }
                    if (isDestinationValid) {
                        // Move unit to destination
                        ManualPath manualPath = GameStateDelegates.GetManualPath();
                        currentSelectedEntity.MoveAlongPath(manualPath.Tiles);
                        GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.UnitMovingToDestination);
                    } else {
                        GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.SelectUnitToControl);
                    }
                    
                }
                
            }
        }
        // ==============================
        // IMPLEMENTED METHODS
        // ==============================
        public void OnPointerMove(PointerEventData eventData) {
            // GameStateDelegates.InvokeOnUnitMoveSelectionChanged(GameStateManager.UnitMoveSelectionMode.Automatic);
        }

        public void OnGridCursorMove(Vector2Int newPosition) {
            Vector2Int gridDimensions = GridDelegates.GetGridDimensions();
            Vector2Int oldGridCursorPosition = _gridCursorPosition;
            _gridCursorPosition = new Vector2Int(Math.Clamp(newPosition.x, 0, gridDimensions.x - 1), Math.Clamp(newPosition.y, 0, gridDimensions.y - 1));
            bool success = GridDelegates.SetInspectedTile(_gridCursorPosition);
            if (!success) {
                _gridCursorPosition = oldGridCursorPosition;
            }
        }
        
        
        // ==============================
        // HELPERS
        // ==============================
       
        

       
    }
}
