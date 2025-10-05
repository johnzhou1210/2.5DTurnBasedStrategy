using System;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StrategyGame.Core.Input {
    public class GridMouseInputRaycaster : MonoBehaviour {
        [SerializeField] private LayerMask tileLayerMask;
        [SerializeField] private PlayerInput playerInput;
        private GameObject _currHighlight;
        private Vector3 _lastMouseRaycastPosition;
        private Camera _camera;
        private InputAction _selectAction;

        private void Awake() {
            _selectAction = playerInput.actions["Select"];
        }

        private void Start() {
            _camera = Camera.main;
        }

        private void OnEnable() {
            InputDelegates.GetMouseRaycastPosition = GetMouseRaycastPosition;
        }

        private void OnDisable() {
            InputDelegates.GetMouseRaycastPosition = null;
        }

        private void Update() {
            if (_camera == null) return;
            Ray ray = _camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100, tileLayerMask)) {
                _lastMouseRaycastPosition = hit.point;
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.green);
                GameObject hitTile = hit.collider.gameObject;
                
                if (_currHighlight != hitTile) {
                    ClearHighlight();
                    SetCurrentHighlight(hitTile);
                }
                
                // Check if player clicked while hovering over a tile
                if (_selectAction.WasPressedThisFrame()) {
                    if (hitTile.TryGetComponent<TileSelectable>(out TileSelectable selectable)) {
                        GridDelegates.InvokeOnSelectTile(selectable.GridCoordinates);
                    }
                }
                
            } else {
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
                ClearHighlight();
            }
        }

        private void SetCurrentHighlight(GameObject tile) {
            _currHighlight = tile;
            if (_currHighlight.TryGetComponent(out Renderer rend)) {
                // rend.material.color = Color.yellow;
                Debug.Log($"Highlighting: {tile}");
            }
        }

        private void ClearHighlight() {
            if (_currHighlight != null) {
                if (_currHighlight.TryGetComponent(out Renderer rend)) {
                    // rend.material.color = Color.white;
                    Debug.Log($"Un-highlighting: {_currHighlight}");
                }
                _currHighlight = null;
            }
        }

        private Vector3 GetMouseRaycastPosition() {
            return _lastMouseRaycastPosition;
        }
        
    }

}
