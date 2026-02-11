using System;
using StrategyGame.Core.Input;
using StrategyGame.UI;
using UnityEngine;

namespace StrategyGame.Core.Delegates {
    public static class InputDelegates {
        // ==============================
        // EVENTS
        // ==============================
        public static event Action<bool> OnSetMouseRaycastEnabled;
        public static event Action OnDownPressed;
        public static event Action OnUpPressed;
        public static event Action OnLeftPressed;
        public static event Action OnRightPressed;
        public static event Action OnConfirmPressed;
        public static event Action<Vector2Int?> OnReinstateGridCursorPosition;
        public static event Action<int> OnSetGridCursorInnerPointerVisibility;

        public static void InvokeOnSetMouseRaycastEnabled(bool value) {
            OnSetMouseRaycastEnabled?.Invoke(value);
        }

        public static void InvokeOnDownPressed() {
            OnDownPressed?.Invoke();
        }

        public static void InvokeOnUpPressed() {
            OnUpPressed?.Invoke();
        }

        public static void InvokeOnLeftPressed() {
            OnLeftPressed?.Invoke();
        }

        public static void InvokeOnRightPressed() {
            OnRightPressed?.Invoke();
        }

        public static void InvokeOnConfirmPressed() {
            OnConfirmPressed?.Invoke();
        }

        public static void InvokeOnReinstateGridCursorPosition(Vector2Int? position) {
            OnReinstateGridCursorPosition?.Invoke(position);
        }
        public static void InvokeOnSetGridCursorInnerPointerVisibility(int entityID) {
            OnSetGridCursorInnerPointerVisibility?.Invoke(entityID);
        }

        // ==============================
        // FUNCS
        // ==============================
        public static Func<Vector3> GetMouseRaycastPosition;
        public static Func<InputManager> GetUIManager;
        public static Func<Vector2Int> GetGridCursorPosition;
        public static Func<bool> GetDangerZoneVisible;

    }
}
