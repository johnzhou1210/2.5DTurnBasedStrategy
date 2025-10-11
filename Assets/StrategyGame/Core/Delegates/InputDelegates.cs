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

        public static void InvokeOnSetMouseRaycastEnabled(bool value) {
            OnSetMouseRaycastEnabled?.Invoke(value);
        }

        // ==============================
        // FUNCS
        // ==============================
        public static Func<Vector3> GetMouseRaycastPosition;
        public static Func<InputManager> GetUIManager;

    }
}
