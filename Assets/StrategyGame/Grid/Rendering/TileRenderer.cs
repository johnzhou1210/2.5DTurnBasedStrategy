using System;
using System.Collections.Generic;
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

    public enum TileHighlightType {
        Move,
        AttackRange,
        Attackable,
        Danger,
        Inspected,
        Hover
    }

    public struct TileHighlight : IEquatable<TileHighlight> {
        public TileHighlightType Type;
        public Faction Owner;
        public TileHighlight(TileHighlightType type, Faction owner) {
            Type = type;
            Owner = owner;
        }
        public bool Equals(TileHighlight other) {
            return Type == other.Type && Owner == other.Owner;
        }
        public override bool Equals(object obj) {
            return obj is TileHighlight other && Equals(other);
        }
        public override int GetHashCode() {
            return HashCode.Combine((int)Type, (int)Owner);
        }
    }

    public class TileRenderer : MonoBehaviour {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
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

        [SerializeField] private Renderer highlightRenderer;

        [SerializeField] private new Renderer renderer;
        [SerializeField] private Animator selectionAnimator;

        private readonly Dictionary<(TileHighlightType, Faction), Color> _highlightColors = new Dictionary<(TileHighlightType, Faction), Color> {
            { (TileHighlightType.Move, Faction.Player), new Color(0, 0, 1, .2f) },
            { (TileHighlightType.Attackable, Faction.Player), new Color(0f, 1.5f, 1.5f, .2f) },
            { (TileHighlightType.AttackRange, Faction.Player), new Color(0, 0, 1.5f, .2f) },
            { (TileHighlightType.AttackRange, Faction.Enemy), new Color(1.25f, 0, 0, .2f) },
            { (TileHighlightType.Danger, Faction.Enemy), new Color(.5f, 0, 0, .2f) }
        };


        private HashSet<TileHighlight> _activeHighlights = new HashSet<TileHighlight>();


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
                    _terrainColor = new Color(.3f, .5f, .2f);
                    break;
                case "Forest":
                    _terrainColor = new Color(.2f, .3f, .2f);
                    break;
                case "Mountains":
                    _terrainColor = new Color(.2f, .2f, .2f);
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
            }
            else if (routeSegmentData.IsStart) {
                routeHalfStraightVisual.SetActive(true);
                activeVisual = routeHalfStraightVisual;
            }
            else if (routeSegmentData.IsDestination) {
                routeTipVisual.SetActive(true);
                activeVisual = routeTipVisual;
            }
            else if (routeSegmentData is { IsTurn: true, IsFlipped: false }) {
                routeTurnVisual.SetActive(true);
                activeVisual = routeTurnVisual;
            }
            else if (routeSegmentData is { IsTurn: true, IsFlipped: true }) {
                routeTurnFlippedVisual.SetActive(true);
                activeVisual = routeTurnFlippedVisual;
            }
            else {
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

        public void RedrawHighlights() {
            TileData tileInitData = GridDelegates.GetTileFromPosition(GridCoordinates).InitData;
            if (tileInitData == null) throw new Exception("Redraw: Tile init data is null");
            SetColor(highlightRenderer, Color.clear);
        }

        private void SetColor(Renderer rend, Color color) {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            rend.GetPropertyBlock(block);
            block.SetColor(BaseColor, color);
            rend.SetPropertyBlock(block);
        }

        public void SetHighlight(TileHighlightType type, Faction owner, bool active) {
            TileHighlight hl = new TileHighlight(type, owner);
            if (active) _activeHighlights.Add(hl);
            else _activeHighlights.Remove(hl);
            UpdateHighlightColor();
        }


        private void UpdateHighlightColor() {
            Color finalColor = Color.clear;

            foreach (var highlight in _activeHighlights) {
                if (_highlightColors.TryGetValue((highlight.Type, highlight.Owner), out var color)) {
                    finalColor = AlphaBlend(finalColor, color);
                }
            }

            SetColor(highlightRenderer, finalColor);
        }
        
        private Color AlphaBlend(Color bottom, Color top) {
            float outAlpha = top.a + bottom.a * (1 - top.a);
            if (outAlpha < 0.001f) return Color.clear;

            float r = (top.r * top.a + bottom.r * bottom.a * (1 - top.a)) / outAlpha;
            float g = (top.g * top.a + bottom.g * bottom.a * (1 - top.a)) / outAlpha;
            float b = (top.b * top.a + bottom.b * bottom.a * (1 - top.a)) / outAlpha;

            return new Color(r, g, b, outAlpha);
        }






    }
}
