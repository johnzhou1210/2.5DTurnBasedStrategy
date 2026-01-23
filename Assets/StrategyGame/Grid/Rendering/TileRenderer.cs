using System;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Core.GameState;
using StrategyGame.Factions;
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
    
    public class TileRenderer : MonoBehaviour {
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

        [SerializeField] private Renderer walkableHighlightRenderer; // Physically in middle
        [SerializeField] private Renderer oppositeReactionHighlightRenderer; // Physically on bottom
        [SerializeField] private Renderer attackableHighlightRenderer; // Physically on top

        [SerializeField] private new Renderer renderer;
        [SerializeField] private Animator selectionAnimator;

        private Color _terrainColor;
        
        public Vector2Int GridCoordinates { get; private set; }
        
        
        
        // ==============================
        // INITIALIZATION
        // ==============================
        public void Initialize(Vector2Int position) {
            GridCoordinates = position;
            gameObject.name = $"Tile {GridCoordinates}";
            Tile tile = GridDelegates.GetTileFromPosition(position);
            switch (tile.InitData.name) {
                case "Grasslands":
                    _terrainColor = new Color(.3f,.5f,.2f);
                    break;
                case "Forest":
                    _terrainColor = new Color(.2f,.3f,.2f);
                    break;
                case "Mountains":
                    _terrainColor = new Color(.2f,.2f,.2f);
                    break;
                default:
                    _terrainColor = Color.black;
                    break;
            }
            Debug.Log($"TileRenderer.Initialize: Setting tile {name} renderer material color to {_terrainColor} when data name is {tile.InitData.name}");
            SetColor(renderer, _terrainColor);
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
            SetColor(walkableHighlightRenderer, val ? new Color(0,0,1,.2f) : Color.clear);
        }

        public void SetOppositeReactionHighlightVisualVisibility(bool val) {
            SetColor(oppositeReactionHighlightRenderer, val ? new Color(1,0,0,.2f) : Color.clear);
        }

        public void SetAttackableHighlightVisualVisibility(bool val) {
            SetColor(attackableHighlightRenderer, val ? new Color(0,0,1,.2f) : Color.clear);
        }

        public void RedrawHighlights() {
            TileData tileInitData = GridDelegates.GetTileFromPosition(GridCoordinates).InitData;
            if (tileInitData == null) throw new Exception("Redraw: Tile init data is null");
            SetColor(walkableHighlightRenderer, Color.clear);
            SetColor(attackableHighlightRenderer, Color.clear);
            SetColor(oppositeReactionHighlightRenderer, Color.clear);
        }

        private void SetColor(Renderer renderer, Color color) {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(block);
        }
        
        
    }
}
