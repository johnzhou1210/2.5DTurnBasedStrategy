using System;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Grid.Rendering {
    public struct RouteSegmentData {
        public bool IsValid;
        public bool IsDestination;
        public bool IsTurn;
        public int Angle;
        public bool IsFlipped;
        public bool IsStart;
    }
    
    public class TileSelectable : MonoBehaviour {
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        [SerializeField] private GameObject selectionVisual;
        [SerializeField] private GameObject routeTipVisual;
        [SerializeField] private GameObject routeStraightVisual;
        [SerializeField] private GameObject routeTurnVisual;
        [SerializeField] private GameObject routeTurnFlippedVisual;
        [SerializeField] private GameObject routeStartVisual;
        [SerializeField] private GameObject routeHalfStraightVisual;

        [SerializeField] private new Renderer renderer;
        [SerializeField] private Animator selectionAnimator;

        private Color _originalColor;
        
        public Vector2Int GridCoordinates { get; private set; }
        
        
        
        // ==============================
        // INITIALIZATION
        // ==============================
        public void Initialize(Vector2Int position) {
            GridCoordinates = position;
            gameObject.name = $"Tile {GridCoordinates}";
            _originalColor = GridDelegates.GetTileFromPosition(position).MovementCost > 100 ? Color.black : Color.gray;
            renderer.material.color = _originalColor;
        }

        
        
        // ==============================
        // CORE METHODS
        // ==============================
        public void SetSelectionVisualVisibility(bool val) {
            if (!val) SetSelectionVisualIsAnimated(false);
            selectionVisual.SetActive(val);
        }

        public void SetSelectionVisualIsAnimated(bool val) {
            selectionAnimator.Play(val ? "Select" : "Unselected");
        }
        
        public void ShowRouteSegment(bool val, RouteSegmentData routeSegmentData) {
            HideAllRouteVisuals();
            
            if (!val) return;
            if (!routeSegmentData.IsValid) return;
            
            GameObject activeVisual;
            if (GameStateDelegates.GetManualPath().Unique.Count <= 1) {
                activeVisual = null;
                HideAllRouteVisuals();
            } else if (routeSegmentData.IsStart) {
                routeHalfStraightVisual.SetActive(true);
                activeVisual = routeHalfStraightVisual;
            } else if (routeSegmentData.IsDestination) {
                routeTipVisual.SetActive(true);
                activeVisual = routeTipVisual;
            } else if (routeSegmentData is { IsTurn: true, IsFlipped: false }) {
                routeTurnVisual.SetActive(true);
                activeVisual = routeTurnVisual;
            } else if (routeSegmentData is { IsTurn: true, IsFlipped: true }) {
                routeTurnFlippedVisual.SetActive(true);
                activeVisual = routeTurnFlippedVisual;
            } else {
                routeStraightVisual.SetActive(true);
                activeVisual = routeStraightVisual;
            }
            // Rotate visual based on RouteSegmentData
            if (activeVisual == null) return;
            activeVisual.transform.localEulerAngles = new Vector3(90, routeSegmentData.Angle, 0);
        }

        private void HideAllRouteVisuals() {
            routeTipVisual.SetActive(false);
            routeStraightVisual.SetActive(false);
            routeTurnVisual.SetActive(false);
            routeTurnFlippedVisual.SetActive(false);
            routeStartVisual.SetActive(false);
            routeHalfStraightVisual.SetActive(false);
        }
        
        // For when unit is selected
        public void SetWalkableMarkVisualVisibility(bool val) {
            if (_originalColor == Color.black) return;
            renderer.material.color = val ? Color.green : Color.gray;
        }

        public void Redraw() {
            TileData tileInitData = GridDelegates.GetTileFromPosition(GridCoordinates).InitData;
            if (tileInitData == null) throw new Exception("Redraw: Tile init data is null");
            _originalColor = tileInitData.MovementCost >=  100 ? Color.black : Color.white;
            renderer.material.color = _originalColor;
        }
        
        
    }
}
