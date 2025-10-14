using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DG.Tweening;
using StrategyGame.AI;
using StrategyGame.Core.Delegates;
using UnityEngine;

namespace StrategyGame.Grid {
    public class GridRenderer : MonoBehaviour {
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        private static readonly Dictionary<(Direction, Direction), (int angle, bool flip)> CornerRotationMap = new() {
            { (Direction.North, Direction.East), (0, false) },
            { (Direction.East, Direction.South), (90, false) },
            { (Direction.South, Direction.West), (180, false) },
            { (Direction.West, Direction.North), (270, false) },
            { (Direction.North, Direction.West), (0, true) },
            { (Direction.East, Direction.North), (90, true) },
            { (Direction.South, Direction.East), (180, true) },
            { (Direction.West, Direction.South), (270, true) },
        };
        private static readonly Dictionary<Direction, int> StraightAngles = new() { { Direction.North, 0 }, { Direction.East, 90 }, { Direction.South, 180 }, { Direction.West, 270 } };
        private static readonly Dictionary<Vector2Int, Direction> OffsetToDirection = new() {
            { Vector2Int.up, Direction.North }, { Vector2Int.down, Direction.South }, { Vector2Int.left, Direction.West }, { Vector2Int.right, Direction.East }
        };
        private GameObject[,] _tileVisuals;
        [SerializeField] private GridManager grid;
        private HashSet<GameObject> _walkableTiles;
        private List<GameObject> _pathTiles;

        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        private void OnEnable() {
            _tileVisuals = new GameObject[grid.GetSize().x, grid.GetSize().y];
            _walkableTiles = new HashSet<GameObject>();
            _pathTiles = new List<GameObject>();
            GridDelegates.OnSetSelectedTile += UpdateSelectedTileVisuals;
            GridDelegates.OnUpdatePathPreview += UpdatePathPreview;
            EntityDelegates.OnEntityMoveAlongPath += RenderEntityMovementAlongPath;
        }
        private void OnDisable() {
            GridDelegates.OnSetSelectedTile -= UpdateSelectedTileVisuals;
            GridDelegates.OnUpdatePathPreview -= UpdatePathPreview;
        }

        // ==============================
        // CORE METHODS
        // ==============================
        private void UpdateSelectedTileVisuals(Tile oldTile, Tile newTile) {
            if (oldTile != null) {
                // Hide old tile selection visual
                GameObject oldTileVisual = _tileVisuals[oldTile.Position.x, oldTile.Position.y];
                if (oldTileVisual == null) {
                    throw new Exception("Old tile visual not found!");
                }
                if (oldTileVisual.TryGetComponent(out TileSelectable oldSelectable)) {
                    oldSelectable.SetSelectionVisualVisibility(false);
                }
                ClearWalkableTiles();
            }

            // Show new tile selection visual
            GameObject newTileVisual = _tileVisuals[newTile.Position.x, newTile.Position.y];
            if (newTileVisual == null) {
                throw new Exception("New tile visual not found!");
            }
            if (newTileVisual.TryGetComponent(out TileSelectable newSelectable)) {
                newSelectable.SetSelectionVisualVisibility(true);

                // If selected tile has entity, show entity's walkable tiles
                if (newTile.IsOccupied) {
                    HashSet<Tile> walkableTileObjects = newTile.Occupant.GetWalkableTiles();
                    foreach (Tile tile in walkableTileObjects) {
                        _walkableTiles.Add(_tileVisuals[tile.Position.x, tile.Position.y]);
                    }
                    foreach (GameObject walkableTile in _walkableTiles) {
                        if (walkableTile.TryGetComponent(out TileSelectable tileSelectable)) {
                            tileSelectable.SetWalkableMarkVisualVisibility(true);
                        }
                    }
                }
            }
        }
        private void UpdatePathPreview(Vector2Int startPosition, Vector2Int endPosition) {
            // Clear all path tiles before rendering new ones
            for (int i = 0; i < _pathTiles.Count; i++) {
                GameObject visual = _pathTiles[i];
                if (visual.TryGetComponent(out TileSelectable tileSelectable)) {
                    tileSelectable.ShowRouteSegment(false, CreateRouteSegmentData(new List<Tile>(), i));
                }
            }
            _pathTiles.Clear();
            // If there is a Unit at startPosition tile, render path preview
            Tile startTile = GridDelegates.GetTileFromPosition(startPosition);
            if (startTile == null)
                return;
            if (!startTile.IsOccupied)
                return;
            if (startTile.Occupant is not GridUnit)
                return;
            // Assign new tiles
            List<Tile> newPath = AStar.CalculateBestPath(startPosition, endPosition).path;
            foreach (Tile tile in newPath) {
                _pathTiles.Add(_tileVisuals[tile.Position.x, tile.Position.y]);
            }
            for (int i = 0; i < _pathTiles.Count; i++) {
                GameObject visual = _pathTiles[i];
                if (visual.TryGetComponent(out TileSelectable tileSelectable)) {
                    tileSelectable.ShowRouteSegment(true, CreateRouteSegmentData(newPath, i));
                }
            }
        }
        private void RenderEntityMovementAlongPath(GridEntity entity, List<Tile> path) {
            StartCoroutine(EntityMovementCoroutine(entity, path));
        }
        public void OnGridRedraw() {
            Vector2Int dimensions = grid.GetSize();
            for (int y = 0; y < dimensions.y; y++) {
                for (int x = 0; x < dimensions.x; x++) {
                    Vector3 position = new Vector3(x, 0f, y);
                    GameObject tilePrefab = grid.Tiles[x, y].InitData.TilePrefab;
                    GameObject newTile = Instantiate(tilePrefab, transform);
                    newTile.transform.position = position;
                    _tileVisuals[x, y] = newTile;
                    if (newTile.TryGetComponent(out TileSelectable selectable)) {
                        selectable.Initialize(new Vector2Int(x, y));
                    }
                }
            }
        }
        public void OnTileRedraw(Tile tileToRedraw) {
            GameObject tileVisualToRedraw = _tileVisuals[tileToRedraw.Position.x, tileToRedraw.Position.y];
            if (tileVisualToRedraw == null)
                throw new Exception("Tile to redraw is null");
            if (tileVisualToRedraw.TryGetComponent(out TileSelectable selectable)) {
                selectable.Redraw();
            }
        }
        
        
        
        // ==============================
        // HELPERS
        // ==============================
        private (int angle, bool flip) GetCornerRotationAngleFromIncomingOutcoming(Direction incoming, Direction outcoming) =>
            CornerRotationMap.TryGetValue((incoming, outcoming), out var result) ? result : throw new Exception("Invalid corner directions");
        private int GetStraightRotationAngleFromIncomingOutcoming(Direction incoming, Direction outcoming) {
            if (incoming != outcoming)
                throw new Exception("Not a straight segment");
            return StraightAngles[incoming];
        }
        private RouteSegmentData CreateRouteSegmentData(List<Tile> pathTiles, int i) {
            if (pathTiles == null || pathTiles.Count == 0 || i < 0 || i >= pathTiles.Count) {
                return new RouteSegmentData { IsValid = false };
            }
            Tile currentTile = pathTiles[i];
            Tile previousTile = i > 0 ? pathTiles[i - 1] : null;
            Tile nextTile = i < pathTiles.Count - 1 ? pathTiles[i + 1] : null;
            Direction? incomingDirection = null;
            Direction? outcomingDirection = null;
            if (previousTile != null) {
                Vector2Int incomingOffset = currentTile.Position - previousTile.Position;
                if (!OffsetToDirection.TryGetValue(incomingOffset, out Direction dirIn)) {
                    throw new Exception($"Invalid incoming offset: {incomingOffset}");
                }
                incomingDirection = dirIn;
            }
            if (nextTile != null) {
                Vector2Int outcomingOffset = nextTile.Position - currentTile.Position;
                if (!OffsetToDirection.TryGetValue(outcomingOffset, out Direction dirOut)) {
                    throw new Exception($"Invalid outcoming offset: {outcomingOffset}");
                }
                outcomingDirection = dirOut;
            }
            int angle = 0;
            bool flip = false;
            bool isTurn = false;
            if (incomingDirection.HasValue && outcomingDirection.HasValue) {
                isTurn = incomingDirection.Value != outcomingDirection.Value;
                if (isTurn) {
                    (angle, flip) = GetCornerRotationAngleFromIncomingOutcoming(incomingDirection.Value, outcomingDirection.Value);
                } else {
                    angle = GetStraightRotationAngleFromIncomingOutcoming(incomingDirection.Value, outcomingDirection.Value);
                }
            } else if (incomingDirection.HasValue) {
                // Last tile: use incoming direction to orient route end
                angle = GetStraightRotationAngleFromIncomingOutcoming(incomingDirection.Value, incomingDirection.Value);
            } else if (outcomingDirection.HasValue) {
                // First tile: use outcoming direction to orient route start
                angle = GetStraightRotationAngleFromIncomingOutcoming(outcomingDirection.Value, outcomingDirection.Value);
            }
            return new RouteSegmentData {
                IsValid = true,
                IsDestination = nextTile == null,
                IsTurn = isTurn,
                Angle = angle,
                IsFlipped = flip,
                IsStart = previousTile == null,
            };
        }
        private void ClearWalkableTiles() {
            foreach (GameObject walkableTile in _walkableTiles) {
                if (walkableTile.TryGetComponent(out TileSelectable tileSelectable)) {
                    tileSelectable.SetWalkableMarkVisualVisibility(false);
                }
            }
            _walkableTiles.Clear();
        }
        
        private IEnumerator EntityMovementCoroutine(GridEntity entity, List<Tile> path) {
            // Get entity transform
            Transform entityTransform = EntityDelegates.GetEntityVisualTransformByID(entity.ID);
            foreach (Tile tile in path) {
                entityTransform.DOMove(new Vector3(tile.Position.x, 0f, tile.Position.y), .33f);
                yield return new WaitForSeconds(.33f);
            }
        }
    }
}
