using System;
using StrategyGame.Grid;
using UnityEngine;

namespace StrategyGame.Core.Delegates {
    public static class EntityVisualDelegates {
        public static event Action<int> OnFadeEntityVisuals;
        public static event Action<GridEntity, GridEntity> OnVisualFace;

        public static void InvokeOnFadeEntityVisuals(int id) {
            OnFadeEntityVisuals?.Invoke(id);
        }

        public static void InvokeOnVisualFace(GridEntity thisEntity, GridEntity otherEntity) {
            OnVisualFace?.Invoke(thisEntity, otherEntity);
        }
        
        public static Func<int, Transform> GetEntityVisualTransformByID;
    }
}
