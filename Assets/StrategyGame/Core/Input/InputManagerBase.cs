using System;
using System.Diagnostics;
using StrategyGame.Audio;
using StrategyGame.Core.Delegates;
using StrategyGame.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace StrategyGame.Core.Input {
    public abstract class InputManagerBase : Singleton<InputManagerBase> {
        [SerializeField] protected PlayerInput playerInput;
        
        [Header("UI Selection Settings")]
        [SerializeField] private float uiSelectionHoldRepeatRate = .1f;
        [SerializeField] protected float uiSelectionHoldInitialDelay = .33f;
        
        protected InputAction moveAction;
        protected InputAction selectAction;
        protected InputAction cancelAction;
        private float _uiSelectionHoldTimer;
        private float _uiSelectionNextRepeatTimer;
        private string _cachedScheme;

        // ================================
        // MONOBEHAVIOUR LIFECYCLE
        // ================================
        override protected void Awake() {
            if (FindObjectsOfType<InputManagerBase>().Length > 2) {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }
        protected virtual void OnEnable() {
            playerInput.actions.Enable();
            playerInput.onControlsChanged += OnControlsChanged;
            InitializeActions();
        }
        protected virtual void OnDisable() {
            playerInput.actions.Disable();
            playerInput.onControlsChanged -= OnControlsChanged;
        }
        protected virtual void Update() {
            ProcessInput();
        }

        // ================================
        // ABSTRACT METHODS
        // ================================
        protected abstract void ProcessInput();
        protected abstract void HandleCancellationInput();
        protected abstract void HandleInteractionInput();
        protected abstract void HandleAxisInput();
        
        // ===========================
        // VIRTUAL METHODS
        // ===========================
        protected virtual void InitializeActions() {
            InputActionAsset actions = playerInput.currentActionMap.asset;
            moveAction = actions.FindAction("Move", true);
            selectAction = actions.FindAction("Select", true);
            cancelAction = actions.FindAction("Cancel", true);
            Debug.Log("InputManagerBase.InitializeActions: Initialized Actions");
        }
        
        protected virtual void HandleMenuAxisInput(Vector2 moveInput) {
            int vertical = 0;
            if (moveInput.y > 0.5f)
                vertical = 1;
            if (moveInput.y < -0.5f)
                vertical = -1;
            void Move() {
                if (vertical == 1 || vertical == -1)
                    ServiceLocator.Get<AudioManager>().PlaySFXAtPointUI(Resources.Load<AudioClip>("Audio/Interface/Audio/scratch_001"), volumeMultiplier: .5f);
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
            if (moveAction.WasPressedThisFrame()) {
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
        
        // ============================
        // PRIVATE METHODS
        // ============================
        private void OnControlsChanged(PlayerInput obj) {
            // Only happens when the device actually changes!
            _cachedScheme = obj.currentControlScheme;
            Debug.Log($"Switching to: {_cachedScheme}");
            InitializeActions();
        }

        
    }
}