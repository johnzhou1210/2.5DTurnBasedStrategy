using System;
using UnityEngine;

namespace StrategyGame.Core.Delegates {
    public static class CameraDelegates {
        #region Events

        public static event Action<Vector3> OnSetCameraRigPosition;

        public static void InvokeOnSetCameraRigPosition(Vector3 position) {
            OnSetCameraRigPosition?.Invoke(position);
        }

        #endregion

        #region Funcs

        #endregion

    }
}
