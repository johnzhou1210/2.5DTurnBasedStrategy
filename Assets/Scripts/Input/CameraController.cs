using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private float cameraMoveSpeed = 5f;
    
    private CinemachineCamera _vCam;

    private InputAction cameraMoveAction;
    private InputAction cameraLookAction;

    private void Awake() {
        cameraMoveAction = playerInput.actions["Move"];
        cameraLookAction = playerInput.actions["Look"];
    }

    private void OnEnable() {
        _vCam = GetComponent<CinemachineCamera>();
    }

    private void Update() {
        if (_vCam == null) return;
        Vector2 moveInput = cameraMoveAction.ReadValue<Vector2>();
        Vector2 lookInput = cameraLookAction.ReadValue<Vector2>();
        _vCam.transform.Translate(new Vector3(moveInput.x, 0f, moveInput.y) * (Time.deltaTime * cameraMoveSpeed));

        Vector3 raycastPosition = InputDelegates.GetMouseRaycastPosition?.Invoke() ?? transform.position;
        if (raycastPosition == transform.position) return;
        _vCam.transform.LookAt(raycastPosition * Time.deltaTime * 200f);
        
    }


}
