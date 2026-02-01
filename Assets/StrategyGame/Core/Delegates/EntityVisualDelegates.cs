using System;
using UnityEngine;

namespace StrategyGame.Core.Delegates {
    public static class EntityVisualDelegates {
        public static event Action OnFadeEntityVisuals;

        public static void InvokeOnFadeEntityVisuals() {
            OnFadeEntityVisuals?.Invoke();
        }
        
        public static Func<int, Transform> GetEntityVisualTransformByID;
    }
}
