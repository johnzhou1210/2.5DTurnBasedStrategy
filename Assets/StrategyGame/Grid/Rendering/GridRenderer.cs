using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using StrategyGame.AI;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Core.GameState;
using StrategyGame.Factions;
using UnityEngine;

namespace StrategyGame.Grid.Rendering {
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
        private HashSet<GameObject> _tilesWithAttackableEntities;
        private HashSet<GameObject> _tilesWithinAttackRange;
        private HashSet<GameObject> _tilesWithinDangerZone;
        private List<GameObject> _pathTiles;

        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        private void OnEnable() {
            _tileVisuals = new GameObject[grid.GetSize().x, grid.GetSize().y];
            _walkableTiles = new HashSet<GameObject>();
            _tilesWithAttackableEntities = new HashSet<GameObject>();
            _tilesWithinAttackRange = new HashSet<GameObject>();
            _tilesWithinDangerZone = new HashSet<GameObject>();
            _pathTiles = new List<GameObject>();
            GridDelegates.OnAStarPathPreview += PreviewPathAStar;
            GridDelegates.OnManualPathPreview += PreviewManualPath;
            EntityDelegates.OnEntityMoveAlongPath += RenderEntityMovementAlongPath;
            GridDelegates.OnInspectedTileChanged += UpdateInspectedTileVisuals;
            GridDelegates.OnSetTileVisualSelectionAnim += UpdateTileVisualSelection;
            GridDelegates.OnClearPath += ClearAllPathTileVisuals;
            GridDelegates.OnGridRedraw += OnGridRedraw;
            GridDelegates.OnSetDangerZoneVisibility += SetDangerZoneVisibility;
            GridDelegates.OnManualMarkTilesWithAttackableEntities += HandleManualMarkTilesWithAttackableEntities;
        }
        private void OnDisable() {
            GridDelegates.OnAStarPathPreview -= PreviewPathAStar;
            GridDelegates.OnManualPathPreview -= PreviewManualPath;
            GridDelegates.OnInspectedTileChanged -= UpdateInspectedTileVisuals;
            GridDelegates.OnClearPath -= ClearAllPathTileVisuals;
            GridDelegates.OnGridRedraw -= OnGridRedraw;
            GridDelegates.OnSetDangerZoneVisibility -= SetDangerZoneVisibility;
            GridDelegates.OnManualMarkTilesWithAttackableEntities -= HandleManualMarkTilesWithAttackableEntities;
        }

        // ==============================
        // CORE METHODS
        // ==============================
        private void UpdateInspectedTileVisuals(Tile oldTile, Tile newTile) {
            if (oldTile != null) {
                // Hide old tile selection visual
                GameObject oldTileVisual = _tileVisuals[oldTile.Position.x, oldTile.Position.y];
                if (oldTileVisual == null) {
                    throw new Exception("GridRenderer.UpdateInspectedTileVisuals: Old tile visual not found!");
                }
                if (oldTileVisual.TryGetComponent(out TileRenderer oldRenderer)) {
                    oldRenderer.SetSelectionVisualIsAnimated(false);
                    oldRenderer.SetSelectionVisualVisibility(false);
                }
                // ClearWalkableTiles();
            }

            if (newTile == null) {
                Debug.Log("GridRenderer.UpdateInspectedTileVisuals: New tile is null, returning early.");
                return;
            }

            // Show new tile selection visual
            GameObject newTileVisual = _tileVisuals[newTile.Position.x, newTile.Position.y];
            if (newTileVisual == null) {
                throw new Exception("GridRenderer.UpdateInspectedTileVisuals: New tile visual not found!");
            }
            if (newTileVisual.TryGetComponent(out TileRenderer newRenderer)) {
                newRenderer.SetSelectionVisualVisibility(true);

                // If currently selecting an entity or hovering over an entity outside of path selection mode, show entity's walkable tiles
                // Do not update walkable tiles if currently selecting a unit's movement path
                GameStateData currentGameState = GameStateDelegates.GetCurrentGameState();

                ClearWalkableTiles();
                if (currentGameState.Combat.PlayerPhase != GameStateEnums.PlayerPhaseState.UnitSelectTarget) ClearAttackableTiles();

                HashSet<Tile> walkableTiles = new HashSet<Tile>();
                HashSet<GridEntity> attackableEntities = new HashSet<GridEntity>();
                GridEntity currSelectedEntity = EntityDelegates.GetGridEntityByID(currentGameState.Combat.SelectedEntityID);

                if (currSelectedEntity != null) {
                    attackableEntities = currSelectedEntity.GetAttackableEntitiesAtPosition(currentGameState.Combat.InspectedTilePosition);
                }

                switch (currentGameState.Combat.TurnPhase) {
                    case GameStateEnums.TurnPhase.Player:
                        switch (currentGameState.Combat.PlayerPhase) {
                            case GameStateEnums.PlayerPhaseState.SelectUnitToControl:
                                ClearTilesWithinAttackRange();
                                if (newTile.IsOccupied && newTile.Occupant.Faction == Faction.Enemy) {
                                    MarkAttackRange(Faction.Enemy, newTile.Occupant);
                                }
                                break;
                            case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination:
                                // To determine how many steps to look, take a look at GameStateManager's manual path
                                int movementCostRemaining = currSelectedEntity.MovementRange - GameStateDelegates.ManualPathSelectionGetSpentMovementCost();
                                Debug.Log($"GridRenderer.UpdateInspectedTileVisuals: Movement cost remaining: {movementCostRemaining}");
                                walkableTiles = currSelectedEntity.GetWalkableTilesAtPosition(newTile.Position, movementCostRemaining, true);
                                walkableTiles.UnionWith(GameStateDelegates.GetManualPath().Unique);
                                MarkWalkableTiles(walkableTiles);

                                if (newTile.Occupant is { Faction: Faction.Enemy }) {
                                    // Highlight enemy
                                    MarkTilesWithAttackableEntities(new HashSet<GridEntity> { newTile.Occupant });
                                } else {
                                    MarkTilesWithAttackableEntities(attackableEntities);
                                }


                                break;
                            case GameStateEnums.PlayerPhaseState.UnitMovingToDestination:
                                break;
                            case GameStateEnums.PlayerPhaseState.UnitActionMenu:
                                MarkTilesWithinAttackRange(Faction.Player, currSelectedEntity.GetAttackableTilesAtPosition(currSelectedEntity.GridPosition));
                                break;
                            case GameStateEnums.PlayerPhaseState.UnitSelectTarget:
                                break;
                            case GameStateEnums.PlayerPhaseState.UnitAttackCutscene:
                                break;
                            case GameStateEnums.PlayerPhaseState.None:
                                break;
                            default:
                                throw new Exception("GridRenderer.UpdateInspectedTileVisuals: Invalid PlayerPhaseState!");
                        }
                        break;
                    case GameStateEnums.TurnPhase.Enemy:
                        ClearTilesWithinAttackRange();
                        break;
                }

            }
        }


        /// <summary>
        /// Visually marks walkable tiles.
        /// </summary>
        /// <param name="walkableTileObjects">The Tiles to mark as walkable.</param>
        private void MarkWalkableTiles(HashSet<Tile> walkableTileObjects) {
            foreach (Tile tile in walkableTileObjects) {
                _walkableTiles.Add(_tileVisuals[tile.Position.x, tile.Position.y]);
            }
            foreach (GameObject walkableTile in _walkableTiles) {
                if (walkableTile.TryGetComponent(out TileRenderer tileRenderer)) {
                    tileRenderer.SetHighlight(TileHighlightType.Move, Faction.Player, true);
                    // tileRenderer.SetWalkableMarkVisualVisibility(true);
                }
            }
        }

        private void MarkTilesWithAttackableEntities(HashSet<GridEntity> attackableEntities) {
            foreach (GridEntity attackableEntity in attackableEntities) {
                Debug.Log($"GridRenderer.MarkTilesWithAttackableEntities: {string.Join(",", attackableEntities)}");
                _tilesWithAttackableEntities.Add(_tileVisuals[attackableEntity.GridPosition.x, attackableEntity.GridPosition.y]);
            }

            foreach (GameObject attackableTile in _tilesWithAttackableEntities) {
                if (attackableTile.TryGetComponent(out TileRenderer tileRenderer)) {
                    tileRenderer.SetHighlight(TileHighlightType.Attackable, Faction.Player, true);
                    // tileRenderer.SetAttackableHighlightVisualVisibility(true);
                }
            }
        }

        private void MarkTilesWithinAttackRange(Faction faction, HashSet<Tile> tilesWithinAttackRange) {
            foreach (Tile tile in tilesWithinAttackRange) {
                _tilesWithinAttackRange.Add(_tileVisuals[tile.Position.x, tile.Position.y]);
            }
            foreach (GameObject tileWithinRange in _tilesWithinAttackRange) {
                if (tileWithinRange.TryGetComponent(out TileRenderer tileRenderer)) {
                    tileRenderer.SetHighlight(TileHighlightType.AttackRange, faction, true);
                    // tileRenderer.SetOppositeReactionHighlightVisualVisibility(true);
                }
            }
        }


        /// <summary>
        /// Renders a visual route of a path.
        /// This method is ONLY for Automatic Move Selection Mode (It uses A*).
        /// </summary>
        /// <param name="startPosition">The start position of the path (usually the selected unit)</param>
        /// <param name="endPosition">The end position of the path (target destination)</param>
        private void PreviewPathAStar(Vector2Int startPosition, Vector2Int endPosition) {
            ClearAllPathTileVisuals();
            // If there is a Unit at startPosition tile, render path preview
            Tile startTile = GridDelegates.GetTileFromPosition(startPosition);
            if (startTile == null)
                return;
            if (!startTile.IsOccupied)
                return;
            // Assign new tiles
            List<Tile> newPath = AStar.CalculateBestPath(startPosition, endPosition).path;
            foreach (Tile tile in newPath) {
                _pathTiles.Add(_tileVisuals[tile.Position.x, tile.Position.y]);
            }
            for (int i = 0; i < _pathTiles.Count; i++) {
                GameObject visual = _pathTiles[i];
                if (visual.TryGetComponent(out TileRenderer tileRenderer)) {
                    tileRenderer.ShowRouteSegment(true, CreateRouteSegmentData(newPath, i));
                }
            }
        }

        private void PreviewManualPath(ManualPath manualPath) {
            ClearAllPathTileVisuals();
            foreach (Tile tile in manualPath.Tiles) {
                _pathTiles.Add(_tileVisuals[tile.Position.x, tile.Position.y]);
            }
            for (int i = 0; i < _pathTiles.Count; i++) {
                GameObject visual = _pathTiles[i];
                if (visual.TryGetComponent(out TileRenderer tileRenderer)) {
                    tileRenderer.ShowRouteSegment(true, CreateRouteSegmentData(manualPath.Tiles, i));
                }
            }
        }

        private void ClearAllPathTileVisuals() {
            // Clear all path tiles before rendering new ones
            for (int i = 0; i < _pathTiles.Count; i++) {
                GameObject visual = _pathTiles[i];
                if (visual.TryGetComponent(out TileRenderer tileRenderer)) {
                    tileRenderer.ShowRouteSegment(false, CreateRouteSegmentData(new List<Tile>(), i));
                }
            }
            _pathTiles.Clear();
        }

        private void UpdateTileVisualSelection(Vector2Int newPosition, bool animated) {
            GameObject tileToUpdate = _tileVisuals[newPosition.x, newPosition.y];
            if (!tileToUpdate.activeInHierarchy) return;
            if (tileToUpdate.TryGetComponent(out TileRenderer tileRenderer)) {
                tileRenderer.SetSelectionVisualIsAnimated(animated);
            }
        }

        private void RenderEntityMovementAlongPath(GridEntity entity, List<Tile> path) {
            StartCoroutine(EntityPathMovementCoroutine(entity, path));
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
                    if (newTile.TryGetComponent(out TileRenderer selectable)) {
                        selectable.Initialize(new Vector2Int(x, y));
                    }
                }
            }
        }
        public void OnTileRedraw(Tile tileToRedraw) {
            GameObject tileVisualToRedraw = _tileVisuals[tileToRedraw.Position.x, tileToRedraw.Position.y];
            if (tileVisualToRedraw == null)
                throw new Exception("GridRenderer.OnTileRedraw: Tile to redraw is null");
            if (tileVisualToRedraw.TryGetComponent(out TileRenderer selectable)) {
                selectable.RedrawHighlights();
            }
        }



        // ==============================
        // HELPERS
        // ==============================
        private (int angle, bool flip) GetCornerRotationAngleFromIncomingOutcoming(Direction incoming, Direction outcoming) =>
            CornerRotationMap.TryGetValue((incoming, outcoming), out var result) ? result : throw new Exception("GridRenderer.GetCornerRotationAngleFromIncomingOutcoming: Invalid corner directions");
        private int GetStraightRotationAngleFromIncomingOutcoming(Direction incoming, Direction outcoming) {
            if (incoming != outcoming)
                throw new Exception("GridRenderer.GetStraightRotationAngleFromIncomingOutcoming: Not a straight segment");
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
                    throw new Exception($"GridRenderer.CreateRouteSegmentData: Invalid incoming offset: {incomingOffset}");
                }
                incomingDirection = dirIn;
            }
            if (nextTile != null) {
                Vector2Int outcomingOffset = nextTile.Position - currentTile.Position;
                if (!OffsetToDirection.TryGetValue(outcomingOffset, out Direction dirOut)) {
                    throw new Exception($"GridRenderer.CreateRouteSegmentData: Invalid outcoming offset: {outcomingOffset}");
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
                }
                else {
                    angle = GetStraightRotationAngleFromIncomingOutcoming(incomingDirection.Value, outcomingDirection.Value);
                }
            }
            else if (incomingDirection.HasValue) {
                // Last tile: use incoming direction to orient route end
                angle = GetStraightRotationAngleFromIncomingOutcoming(incomingDirection.Value, incomingDirection.Value);
            }
            else if (outcomingDirection.HasValue) {
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
                if (walkableTile.TryGetComponent(out TileRenderer tileRenderer)) {
                    tileRenderer.SetHighlight(TileHighlightType.Move, Faction.Player, false);
                    // tileRenderer.SetWalkableMarkVisualVisibility(false);
                }
            }
            _walkableTiles.Clear();
        }

        private void ClearAttackableTiles() {
            foreach (GameObject attackableTile in _tilesWithAttackableEntities) {
                if (attackableTile.TryGetComponent(out TileRenderer tileRenderer)) {
                    tileRenderer.SetHighlight(TileHighlightType.Attackable, Faction.Player, false);
                    // tileRenderer.SetAttackableHighlightVisualVisibility(false);
                }
            }
            _tilesWithAttackableEntities.Clear();
        }

        private void ClearTilesWithinAttackRange() {
            foreach (GameObject tile in _tilesWithinAttackRange) {
                if (tile.TryGetComponent(out TileRenderer tileRenderer)) {
                    tileRenderer.SetHighlight(TileHighlightType.AttackRange, Faction.Player, false);
                    tileRenderer.SetHighlight(TileHighlightType.AttackRange, Faction.Enemy, false);
                    // tileRenderer.SetOppositeReactionHighlightVisualVisibility(false);
                }
            }
            _tilesWithinAttackRange.Clear();
        }

        private void ClearTilesWithinDangerZone() {
            foreach (GameObject tile in _tilesWithinDangerZone) {
                if (tile.TryGetComponent(out TileRenderer tileRenderer)) {
                    tileRenderer.SetHighlight(TileHighlightType.Danger, Faction.Enemy, false);
                    // tileRenderer.SetOppositeReactionHighlightVisualVisibility(false);
                }
            }
            _tilesWithinDangerZone.Clear();
        }

        private IEnumerator EntityPathMovementCoroutine(GridEntity entity, List<Tile> path) {
            // Get entity transform
            Transform entityTransform = EntityVisualDelegates.GetEntityVisualTransformByID(entity.ID);
            SpriteRenderer spriteRenderer = entityTransform.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null) throw new Exception("GridRenderer.EntityPathMovementCoroutine: No SpriteRenderer found!");
            List<Tile> pathCopy = new List<Tile>(path);
            Debug.Log(string.Join(", ", pathCopy));
            foreach (Tile tile in pathCopy) {
                Tween tween = entityTransform.DOMove(new Vector3(tile.Position.x, 0f, tile.Position.y), 0.33f).SetEase(Ease.Linear);
                Debug.Log($"Current x: {entityTransform.position.x}, Tile x: {tile.Position.x}");
                if (!Mathf.Approximately(entityTransform.position.x, tile.Position.x)) spriteRenderer.flipX = entityTransform.transform.position.x > tile.Position.x;
                
                yield return tween.WaitForCompletion();
            }
            GameStateData currentState = GameStateDelegates.GetCurrentGameState();
            switch (currentState.Combat.TurnPhase) {
                case GameStateEnums.TurnPhase.Player:
                    // Notify game state to change immediately to unit action menu
                    GameStateDelegates.InvokeOnPlayerPhaseStateChanged(GameStateEnums.PlayerPhaseState.UnitActionMenu);
                    break;
                case GameStateEnums.TurnPhase.Enemy:
                    // Notify game state to continue to next enemy actor.
                    currentState.Combat.NextActorReady = true;
                    break;
                case GameStateEnums.TurnPhase.Event:
                    break;
                case GameStateEnums.TurnPhase.None:
                    break;
                default:
                    throw new Exception("GridRenderer.EntityPathMovementCoroutine: Invalid turn phase!");
            }
        }

        private void SetDangerZoneVisibility(bool val) {
            if (val) {
                // add tiles to danger zone
                List<int> allEnemyIDs = EntityDelegates.GetAllGridEntityIDsByFaction(Faction.Enemy, false);
                foreach (int enemyID in allEnemyIDs) {
                    GridEntity currentEnemy = EntityDelegates.GetGridEntityByID(enemyID);
                    HashSet<Tile> dangerTiles = currentEnemy.GetTilesWithinAttackRange();

                    foreach (Tile tile in dangerTiles) {
                        _tilesWithinDangerZone.Add(_tileVisuals[tile.Position.x, tile.Position.y]);
                    }
                }
                foreach (GameObject tile in _tilesWithinDangerZone) {
                    if (tile.TryGetComponent(out TileRenderer tileRenderer)) {
                        tileRenderer.SetHighlight(TileHighlightType.Danger, Faction.Enemy, true);
                        // tileRenderer.SetOppositeReactionHighlightVisualVisibility(true);
                    }
                }
            }
            else {
                // clear tiles from danger zone
                ClearTilesWithinDangerZone();
                GameStateData currState = GameStateDelegates.GetCurrentGameState();
                Tile currInspectedTile = GridDelegates.GetTileFromPosition(currState.Combat.InspectedTilePosition);
                if (currInspectedTile.IsOccupied && currInspectedTile.Occupant.Faction == Faction.Enemy) {
                    MarkAttackRange(Faction.Enemy, currInspectedTile.Occupant);
                }
            }
        }

        private void MarkAttackRange(Faction faction, GridEntity entity) {
            HashSet<Tile> tilesWithinAttackRange = entity.GetTilesWithinAttackRange();
            MarkTilesWithinAttackRange(faction, tilesWithinAttackRange);
        }

        private void HandleManualMarkTilesWithAttackableEntities() {
            GameStateData currentState = GameStateDelegates.GetCurrentGameState();
            GridEntity selectedEntity = EntityDelegates.GetGridEntityByID(currentState.Combat.SelectedEntityID);
            MarkTilesWithAttackableEntities(selectedEntity.GetAttackableEntitiesAtPosition(selectedEntity.GridPosition));
        }

    }
}
