using System;
using UnityEngine;

namespace Grid {
    public class GridHighlighter : MonoBehaviour {
        [SerializeField] private LayerMask tileLayerMask;
        private GameObject _currHighlight;
        private Vector3 _lastMouseRaycastPosition;
        private Camera _camera;

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
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100, tileLayerMask)) {
                _lastMouseRaycastPosition = hit.point;
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.green);
                GameObject hitTile = hit.collider.gameObject;
                
                if (_currHighlight != hitTile) {
                    ClearHighlight();
                    SetCurrentHighlight(hitTile);
                }
            } else {
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
                ClearHighlight();
            }
        }

        private void SetCurrentHighlight(GameObject tile) {
            _currHighlight = tile;
            if (_currHighlight.TryGetComponent(out Renderer rend)) {
                rend.material.color = Color.yellow;
                Debug.Log($"Highlighting: {tile}");
            }
        }

        private void ClearHighlight() {
            if (_currHighlight != null) {
                if (_currHighlight.TryGetComponent(out Renderer rend)) {
                    rend.material.color = Color.white;
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
