using StrategyGame.Utils;
using UnityEngine;

namespace StrategyGame.Core.Input {
    public class MenuInputManager : InputManagerBase {
        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        override protected void Awake() {
            base.Awake();
            ServiceLocator.Register(this);
        }

        override protected void OnDestroy() {
            base.OnDestroy();
            ServiceLocator.Unregister<MenuInputManager>();
        }
        
        // ===============================
        // ABSTRACT OVERRIDES
        // ===============================
        override protected void ProcessInput() {
            HandleCancellationInput();
            HandleInteractionInput();
            HandleAxisInput();
        }
        override protected void HandleAxisInput() {
            Vector2 axisInput = moveAction.ReadValue<Vector2>();
            if (axisInput == Vector2.zero) return;
            // Debug.Log($"MenuInputManager.HandleAxisInput: Handling axis input: {axisInput}");
            HandleMenuAxisInput(axisInput);
            
        }
        override protected void HandleInteractionInput() {
            if (!selectAction.WasPerformedThisFrame()) return;
            Debug.Log("MenuInputManager.HandleInteractionInput: Handling interaction input");
            HandleUIConfirmation();
        }
        override protected void HandleCancellationInput() {
            if (!cancelAction.WasPerformedThisFrame()) return;
            Debug.Log("MenuInputManager.HandleCancellationInput: Handling cancellation input");
        }
        
        // ================================
        // VIRTUAL OVERRIDES
        // ================================
        override protected void InitializeActions() {
            base.InitializeActions();
            Debug.Log("MenuInputManager.InitializeActions: Input actions initialized");
        }
        override protected void Update() {
            base.Update();
        }
        
    }
}
