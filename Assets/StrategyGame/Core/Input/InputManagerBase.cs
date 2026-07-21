using System;
using System.Diagnostics;
using StrategyGame.Core.Delegates;
using StrategyGame.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace StrategyGame.Core.Input {
    public abstract class InputManagerBase : Singleton<InputManagerBase> {
        [SerializeField] protected PlayerInput playerInput;
        protected InputAction moveAction;
        protected InputAction selectAction;
        protected InputAction cancelAction;
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