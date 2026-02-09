using System;
using StrategyGame.Core.Delegates;
using Unity.Cinemachine;
using UnityEngine;

namespace StrategyGame.UI.World {
    public class Billboard : MonoBehaviour
    {
        [SerializeField] private Transform targetTransform;
        [SerializeField] private bool lookAtTarget = false;

        private CinemachineCamera _vCam;
        private CinemachineBrain _cineBrain;
        private Transform _cameraRigTransform;

        private void OnEnable() {
            BillboardDelegates.OnSetLookatTargetTransform += SetCameraRigTransform;
            if (Camera.main != null) {
                _cineBrain = Camera.main.GetComponent<CinemachineBrain>();
                _vCam = _cineBrain.ActiveVirtualCamera as CinemachineCamera;
                _cameraRigTransform = _cineBrain.transform;
            }
        }

        private void OnDisable() {
            BillboardDelegates.OnSetLookatTargetTransform -= SetCameraRigTransform;
        }

        private void SetCameraRigTransform(Transform newTransform) {
            _cameraRigTransform = newTransform;
        }

        private void LateUpdate() {
            transform.LookAt(targetTransform);
            if (lookAtTarget) {
                if (_vCam == null) {
                    _vCam = _cineBrain.ActiveVirtualCamera as CinemachineCamera;
                }
                Debug.Log($"Billboard.LateUpdate: Looking at {_cineBrain.ActiveVirtualCamera}, {_cineBrain.ActiveVirtualCamera.Name}, {_cameraRigTransform.name}, {_cameraRigTransform.position}");
                transform.LookAt(_cameraRigTransform.position);
                transform.Rotate(0f,180f,0f);
            } else {
                transform.forward = Camera.main.transform.forward;
            }
        }

    }
}

