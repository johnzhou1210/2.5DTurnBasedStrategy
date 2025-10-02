using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRigController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private float rigMoveSpeed = 5f;
    [SerializeField] private float rigZoomSpeed = 5f;
    [SerializeField] private Vector2 rigZoomMinMax = new Vector2(0f, 10f);
    [SerializeField] private float zoomSmoothTime = 0.2f;
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;

    private InputAction _rigMoveAction;
    private InputAction _rigZoomAction;

    private float _targetZoom;
    private float _zoomVelocity;

    private void Awake() {
        if (orbitalFollow == null) return;
        
        _rigMoveAction = playerInput.actions["Move"];
        _rigZoomAction = playerInput.actions["Zoom"];

        _targetZoom = orbitalFollow.Radius;
    }
    

    private void Update() {
        if (orbitalFollow == null) return;
        
        // Camera rig movement
        Vector2 moveInput = _rigMoveAction.ReadValue<Vector2>();
        transform.Translate(new Vector3(moveInput.x, 0f, moveInput.y) * (Time.deltaTime * rigMoveSpeed));
        
        // Cinemachine camera zoom
        Vector2  zoomInput = _rigZoomAction.ReadValue<Vector2>();
        float scrollY = zoomInput.y;
        _targetZoom = Mathf.Clamp(
                _targetZoom - scrollY * rigZoomSpeed * Time.deltaTime,
                rigZoomMinMax.x, rigZoomMinMax.y
            );
        
        orbitalFollow.Radius = Mathf.SmoothDamp(
                orbitalFollow.Radius,
                _targetZoom,
                ref _zoomVelocity,
                zoomSmoothTime
            );
        
    }


}
