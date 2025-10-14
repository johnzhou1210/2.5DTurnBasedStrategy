using System;
using System.Threading.Tasks;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.GameState;
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
        [SerializeField] private float pathSelectionMoveActionCooldown = 0.33f;
        [SerializeField] private float pathSelectionMoveActionCooldownAccelerationThreshold = 1f;
        [SerializeField] private float pathSelectionMoveActionMinimumCooldown = .08f;
        [SerializeField] private float pathSelectionMoveActionCooldownAccelerationRate = .05f;
        private InputAction _moveAction;
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
        }

        private void OnEnable() {
            InputDelegates.OnSetMouseRaycastEnabled += SetMouseRaycastEnabled;
            InputDelegates.GetUIManager = () => this;
        }

        private void OnDisable() {
            InputDelegates.OnSetMouseRaycastEnabled -= SetMouseRaycastEnabled;
            InputDelegates.GetUIManager = null;
        }

        private void Start() {
            GameStateManager.UnitMoveSelectionMode currentUnitMoveSelectionMode = GameStateDelegates.GetCurrentUnitMoveSelectionMode();
            gridMouseInputRaycaster.enabled = currentUnitMoveSelectionMode == GameStateManager.UnitMoveSelectionMode.Automatic;
            cameraRigController.SetPanningEnabled(currentUnitMoveSelectionMode == GameStateManager.UnitMoveSelectionMode.Automatic);
            // cameraRigController.SetZoomingEnabled(currentUnitMoveSelectionMode == GameStateManager.UnitMoveSelectionMode.Automatic);
        }
        
        

        private void Update() {
            _pathSelectionMoveActionTimer = Mathf.Max(0f, _pathSelectionMoveActionTimer - Time.deltaTime);

            Vector2 moveInput = _moveAction.ReadValue<Vector2>();
            Vector2Int moveDirection = new Vector2Int(
                moveInput.x > .5f ? 1 : moveInput.x < -.5f ? -1 : 0,
                moveInput.y > .5f ? 1 : moveInput.y < -.5f ? -1 : 0
            );

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

                Vector2Int moveVector = _isDiagonalMoveEnabled ? moveDirection : new Vector2Int(moveDirection.x, moveDirection.y);
                OnGridCursorMove(_gridCursorPosition + moveVector);
            } else {
                _pathSelectionMoveActionHeldDuration = 0f;
                _currentPathSelectionMoveActionCooldown = Mathf.Lerp(_currentPathSelectionMoveActionCooldown, pathSelectionMoveActionCooldown, Time.deltaTime * 5f);
            }

            if (_moveAction.WasReleasedThisFrame()) {
                _pathSelectionMoveActionTimer = 0f;
            }
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

        // ==============================
        // IMPLEMENTED METHODS
        // ==============================
        public void OnPointerMove(PointerEventData eventData) {
            GameStateDelegates.InvokeOnUnitMoveSelectionMode(GameStateManager.UnitMoveSelectionMode.Automatic);
        }

        public void OnGridCursorMove(Vector2Int newPosition) {
            Vector2Int gridDimensions = GridDelegates.GetGridDimensions();
            _gridCursorPosition = new Vector2Int(Math.Clamp(newPosition.x, 0, gridDimensions.x - 1), Math.Clamp(newPosition.y, 0, gridDimensions.y - 1));
            GridDelegates.InvokeOnSelectTile(_gridCursorPosition);
        }
        
        
        // ==============================
        // HELPERS
        // ==============================
       
        

       
    }
}
