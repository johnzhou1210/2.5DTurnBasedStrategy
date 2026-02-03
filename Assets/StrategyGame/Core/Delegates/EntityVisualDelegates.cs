using System;
using UnityEngine;

namespace StrategyGame.Core.Delegates {
    public static class EntityVisualDelegates {
        public static event Action<int> OnFadeEntityVisuals;

        public static void InvokeOnFadeEntityVisuals(int id) {
            OnFadeEntityVisuals?.Invoke(id);
        }
        
        public static Func<int, Transform> GetEntityVisualTransformByID;
    }
}
