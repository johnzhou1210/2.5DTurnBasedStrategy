using System;
using UnityEngine;

namespace Grid {
    public class GridHighlighter : MonoBehaviour {
        [SerializeField] private LayerMask tileLayerMask;
        private GameObject _currHighlight;
        
        private void Update() {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, 100, tileLayerMask)) {
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
    }

}
