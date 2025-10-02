using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private bool lookAtTarget = false;

    private CinemachineCamera _vCam;
    private CinemachineBrain _cineBrain;

    private void OnEnable() {
        if (Camera.main != null) {
            _cineBrain = Camera.main.GetComponent<CinemachineBrain>();
            _vCam = _cineBrain.ActiveVirtualCamera as CinemachineCamera;
        }
    }


    private void LateUpdate() {
        transform.LookAt(targetTransform);
        if (lookAtTarget) {
            transform.LookAt(_vCam.transform.position);
            transform.Rotate(0f,180f,0f);
        } else {
            transform.forward = Camera.main.transform.forward;
        }
    }

}
