using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using StrategyGame.AI;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Factions;
using StrategyGame.Grid;
using StrategyGame.Grid.GridData;
using StrategyGame.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StrategyGame.Core.GameState {
    public class ManualPath {
        public List<Tile> Tiles;
        public HashSet<Tile> Unique;

        public ManualPath() {
            Tiles = new List<Tile>();
            Unique = new HashSet<Tile>();
        }

        /// <summary>
        /// Attempts to step to the given tile.
        /// </summary>
        /// <param name="tile">The tile to step to.</param>
        /// <returns>If the step was successful.</returns>
        public bool StepToTile(Tile tile) {
            if (Tiles.Count >= 2 && Equals(Tiles[^2], tile)) {
                // Simulate "stepback"
                Tile tileToRemove = Tiles[^1];
                Unique.Remove(tileToRemove);
                Tiles.RemoveAt(Tiles.Count - 1);
                return true;
            }

            if (Unique.Contains(tile)) {
                return false;
            }

            Tiles.Add(tile);
            Debug.Log($"GameStateManager.StepToTile: Added {tile} to Tiles list");
            Unique.Add(tile);
            Debug.Log($"GameStateManager.StepToTile: Added {tile} to Unique list");
            return true;
        }

        public void Clear() {
            Tiles.Clear();
            Unique.Clear();
            GridDelegates.InvokeOnClearPath();
        }

        public override string ToString() {
            return string.Join(", ", Tiles);
        }
    }

    public class GameStateManager : MonoBehaviour {
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        [SerializeField] private GameStateData currentState;

        public GameStateData CurrentState => currentState;

        public ManualPath ManualPath { get; private set; }
        private Coroutine _coreGameLoop;
        private Coroutine _enemyPhaseCoroutine;
        private Coroutine _eventPhaseCoroutine;

        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        private void OnEnable() {
            ManualPath = new ManualPath();
            currentState = new GameStateData {
                Combat = new CombatStateData {
                    TurnPhase = GameStateEnums.TurnPhase.None,
                    PlayerPhase = GameStateEnums.PlayerPhaseState.SelectUnitToControl,
                    UnitMoveSelectionMode = GameStateEnums.UnitMoveSelectionMode.Manual,
                },
                MasterState = GameStateEnums.MasterState.Combat
            };
            GameStateDelegates.OnGameStarted += StartGame;
            GameStateDelegates.OnUnitMoveSelectionChanged += SetCurrentUnitMoveSelectionMode;
            GameStateDelegates.OnPlayerPhaseStateChanged += SetCurrentPlayerPhaseState;
            GameStateDelegates.OnAdvanceTurnPhase += AdvancePhase;
            GameStateDelegates.GetManualPath = () => ManualPath;
            GameStateDelegates.GetCurrentGameState = GetCurrentGameState;
            GridDelegates.SetInspectedTile = HandleSetInspectedTile;
            GameStateDelegates.ManualPathSelectionGetSpentMovementCost = GetManualPathUsedMovementCost;
        }

        private void OnDisable() {
            GameStateDelegates.OnGameStarted -= StartGame;
            GameStateDelegates.OnUnitMoveSelectionChanged -= SetCurrentUnitMoveSelectionMode;
            GameStateDelegates.OnPlayerPhaseStateChanged -= SetCurrentPlayerPhaseState;
            GameStateDelegates.OnAdvanceTurnPhase -= AdvancePhase;
            GameStateDelegates.GetManualPath = null;
            GameStateDelegates.GetCurrentGameState = null;
            GridDelegates.SetInspectedTile = null;
            GameStateDelegates.ManualPathSelectionGetSpentMovementCost = null;
            if (_coreGameLoop != null) {
                StopCoroutine(_coreGameLoop);
                _coreGameLoop = null;
            }
            if (_enemyPhaseCoroutine != null) {
                StopCoroutine(_enemyPhaseCoroutine);
                _enemyPhaseCoroutine = null;
            }
            if (_eventPhaseCoroutine != null) {
                StopCoroutine(_eventPhaseCoroutine);
                _eventPhaseCoroutine = null;
            }
        }

        // ==============================
        // CORE METHODS
        // ==============================
        private void AdvancePhase() {
            GameStateEnums.TurnPhase nextPhaseState = (GameStateEnums.TurnPhase)(((int)CurrentState.Combat.TurnPhase + 1) % Enum.GetValues(typeof(GameStateEnums.TurnPhase)).Length);
            if (nextPhaseState == 0) {
                CurrentState.Combat.TurnPhaseCycle += 1;
            }
            SetTurnPhaseState(nextPhaseState);
            Debug.Log($"GameStateManager.AdvancePhase: Setting turn phase state to {CurrentState.Combat.TurnPhase}");
            GameStateDelegates.InvokeOnTurnPhaseChanged(CurrentState.Combat.TurnPhase);
        }

        private void StartGame() {
            Debug.Log("Starting Game");
            List<UnitSpawnQuery> units = new List<UnitSpawnQuery>();
            units.Add(new UnitSpawnQuery {
                UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Soldier"),
                SpawnPosition = new Vector2Int(0, 0)
            });
            units.Add(new UnitSpawnQuery {
                UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Orc"),
                SpawnPosition = new Vector2Int(1, 1)
            });
            units.Add(new UnitSpawnQuery {
                UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Archer"),
                SpawnPosition = new Vector2Int(2, 2)
            });
            units.Add(new UnitSpawnQuery {
                UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Soldier"),
                SpawnPosition = new Vector2Int(5, 1)
            });
            units.Add(new UnitSpawnQuery {
                UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Orc"),
                SpawnPosition = new Vector2Int(3, 6)
            });
            units.Add(new UnitSpawnQuery {
                UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Elite Orc"),
                SpawnPosition = new Vector2Int(4, 4)
            });
            units.Add(new UnitSpawnQuery {
                UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Elite Orc"),
                SpawnPosition = new Vector2Int(0, 1)
            });
            EntityDelegates.SpawnUnits(units);
            GenerateRandomBiome(Resources.Load<TileData>("ScriptableObjects/Tiles/Mountains"));
            GenerateRandomBiome(Resources.Load<TileData>("ScriptableObjects/Tiles/Forest"));
            
            GridDelegates.InvokeOnGridRedraw();

            // Start core game loop
            SetInspectedTile(Vector2Int.zero);
            _coreGameLoop = StartCoroutine(CoreGameLoop());
        }

        private void SetTurnPhaseState(GameStateEnums.TurnPhase phase) {
            if (phase == CurrentState.Combat.TurnPhase)
                return;
            CurrentState.Combat.TurnPhase = phase;
            UIDelegates.InvokeOnUpdateTurnIndicatorRenderer();
            if (CurrentState.Combat.TurnPhase is GameStateEnums.TurnPhase.Player or GameStateEnums.TurnPhase.Enemy) UIDelegates.InvokeOnPlayPhaseBannerAnimationSequence();

            // Play ui animations
            // Depending on turn phase, fill ActorsRemaining with the entities from the current phase.
            switch (CurrentState.Combat.TurnPhase) {
                case GameStateEnums.TurnPhase.Player:
                    // Clear danger zone highlights
                    if (InputDelegates.GetDangerZoneVisible()) GridDelegates.InvokeOnSetDangerZoneVisibility(true); // doesn't change input state
                    CurrentState.Combat.ActorIDsRemaining =
                        EntityDelegates.GetAllGridEntityIDsByFaction(Faction.PlayerFaction); 
                    CurrentState.Combat.PlayerPhase = GameStateEnums.PlayerPhaseState.SelectUnitToControl;
                    InputDelegates.InvokeOnReinstateGridCursorPosition();
                    break;
                case GameStateEnums.TurnPhase.Enemy:
                    // Clear danger zone highlights
                    GridDelegates.InvokeOnSetDangerZoneVisibility(false); // doesn't change input state
                    CurrentState.Combat.ActorIDsRemaining =
                        EntityDelegates.GetAllGridEntityIDsByFaction(Faction.EnemyFaction);
                    // Automate enemy actions
                    _enemyPhaseCoroutine = StartCoroutine(RunEnemyPhaseCoroutine());
                    break;
                case GameStateEnums.TurnPhase.Event:
                    // Run any events this phase cycle
                    _eventPhaseCoroutine = StartCoroutine(RunEventPhaseCoroutine());
                    break;
                case GameStateEnums.TurnPhase.None: break;
                default: throw new Exception("GameStateManager.HandleOnTurnPhaseChange: Invalid turn phase!");
            }
        }

        // ==============================
        // CORE GAME LOOP
        // ==============================
        private IEnumerator CoreGameLoop() {
            while (true) {
                switch (CurrentState.Combat.TurnPhase) {
                    case GameStateEnums.TurnPhase.Player: HandlePlayerPhaseState(); break;
                    case GameStateEnums.TurnPhase.Enemy: HandleEnemyPhaseState(); break;
                    case GameStateEnums.TurnPhase.Event: HandleEventPhaseState(); break;
                    case GameStateEnums.TurnPhase.None: 
                        AdvancePhase();
                        break;
                    default:
                        throw new InvalidEnumArgumentException("GameStateManager.CoreGameLoop: Invalid turn phase!");
                }

                yield return new WaitForEndOfFrame();
            }
        }

        // ==============================
        // PHASE HANDLERS
        // ==============================
        /// <summary>
        /// This method is called every frame if the current phase is the Player's phase.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException">Occurs if the current player phase state is an invalid one.</exception>
        private void HandlePlayerPhaseState() {
            switch (CurrentState.Combat.PlayerPhase) {
                case GameStateEnums.PlayerPhaseState.SelectUnitToControl: break;
                case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination:
                    GridDelegates.InvokeOnSetTileVisualSelectionAnim(CurrentState.Combat.InspectedTilePosition, true);
                    break;
                case GameStateEnums.PlayerPhaseState.UnitMovingToDestination:
                    if (CurrentState.Combat.SelectedEntityID == -1) {
                        Debug.LogWarning("GameStateManager.HandlePlayerPhaseState: CurrentSelectedEntity is null");
                        return;
                    }
                    // Remove selection

                    // Focus camera rig onto position
                    // Debug.Log("Current selected entity: " + CurrentSelectedEntity);
                    Vector3 visualPosition = EntityDelegates
                        .GetEntityVisualTransformByID(CurrentState.Combat.SelectedEntityID).position;
                    CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(visualPosition.x, visualPosition.y,
                        visualPosition.z));
                    break;
                case GameStateEnums.PlayerPhaseState.UnitActionMenu: break;
                case GameStateEnums.PlayerPhaseState.UnitSelectTarget:
                    GridDelegates.InvokeOnSetTileVisualSelectionAnim(CurrentState.Combat.InspectedTilePosition, true);
                    break;
                case GameStateEnums.PlayerPhaseState.UnitAttackCutscene: break;
                case GameStateEnums.PlayerPhaseState.None: break;
                default: throw new Exception("GameStateManager.HandlePlayerPhaseState: Invalid PlayerPhase state!");
            }
        }

        private void HandleEnemyPhaseState() {
            Transform entityVisualTransform =
                EntityDelegates.GetEntityVisualTransformByID(CurrentState.Combat.SelectedEntityID);
            if (entityVisualTransform != null) {
                Vector3 visualPosition = entityVisualTransform.position;
                switch (CurrentState.Combat.EnemyPhase) {
                    case GameStateEnums.EnemyPhaseState.SelectUnitToControl:
                        CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(visualPosition.x, visualPosition.y, visualPosition.z));
                        break;
                    case GameStateEnums.EnemyPhaseState.SelectUnitMoveDestination:
                        CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(visualPosition.x, visualPosition.y, visualPosition.z));
                        break;
                    case GameStateEnums.EnemyPhaseState.UnitMovingToDestination:
                        CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(visualPosition.x, visualPosition.y, visualPosition.z));
                        break;
                    case GameStateEnums.EnemyPhaseState.UnitContemplateAction:
                        CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(visualPosition.x, visualPosition.y, visualPosition.z));
                        break;
                    case GameStateEnums.EnemyPhaseState.UnitSelectTarget:
                        CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(visualPosition.x, visualPosition.y, visualPosition.z));
                        break;
                    case GameStateEnums.EnemyPhaseState.UnitAttackCutscene: break;
                    case GameStateEnums.EnemyPhaseState.None: break;
                    default: throw new Exception("GameStateManager.HandleEnemyPhaseState: Invalid EnemyPhase state!");
                }
            }
            else {
                Debug.LogWarning($"GameStateManager.HandleEnemyPhaseState: Current frame doesn't have selected entity (id is {CurrentState.Combat.SelectedEntityID}).");
                switch (CurrentState.Combat.EnemyPhase) {
                    case GameStateEnums.EnemyPhaseState.SelectUnitToControl:
                        break;
                    case GameStateEnums.EnemyPhaseState.SelectUnitMoveDestination:
                        break;
                    case GameStateEnums.EnemyPhaseState.UnitMovingToDestination:
                        break;
                    case GameStateEnums.EnemyPhaseState.UnitContemplateAction:
                        break;
                    case GameStateEnums.EnemyPhaseState.UnitSelectTarget:
                        break;
                    case GameStateEnums.EnemyPhaseState.UnitAttackCutscene: break;
                    case GameStateEnums.EnemyPhaseState.None: break;
                    default: throw new Exception("GameStateManager.HandleEnemyPhaseState: Invalid EnemyPhase state!");
                }
            }
        }

        private void HandleEventPhaseState() {
        }

        // ==============================
        // CORE METHODS
        // ==============================
        private void SetCurrentUnitMoveSelectionMode(GameStateEnums.UnitMoveSelectionMode mode) {
            if (CurrentState.Combat.UnitMoveSelectionMode == mode)
                return;
            CurrentState.Combat.UnitMoveSelectionMode = mode;
            switch (CurrentState.Combat.UnitMoveSelectionMode) {
                case GameStateEnums.UnitMoveSelectionMode.Manual:
                    InputDelegates.InvokeOnSetMouseRaycastEnabled(false); break;
                case GameStateEnums.UnitMoveSelectionMode.Automatic:
                    InputDelegates.InvokeOnSetMouseRaycastEnabled(true); break;
                case GameStateEnums.UnitMoveSelectionMode.None:
                    InputDelegates.InvokeOnSetMouseRaycastEnabled(false); break;
                default:
                    throw new InvalidEnumArgumentException(
                        "GameStateManager.SetCurrentUnitMoveSelectionMode: Invalid unit move selection mode!");
            }
        }

        private void SetCurrentPlayerPhaseState(GameStateEnums.PlayerPhaseState phase) {
            if (CurrentState.Combat.PlayerPhase == phase)
                return;
            CurrentState.Combat.PlayerPhase = phase;
            switch (CurrentState.Combat.PlayerPhase) {
                case GameStateEnums.PlayerPhaseState.SelectUnitToControl:
                    ManualPath.Clear();
                    CurrentState.Combat.SelectedEntityID = -1;
                    GridDelegates.InvokeOnSetTileVisualSelectionAnim(CurrentState.Combat.InspectedTilePosition, false);
                    Tile currInspectedTile = GridDelegates.GetTileFromPosition(CurrentState.Combat.InspectedTilePosition);
                    GridDelegates.InvokeOnInspectedTileChanged(currInspectedTile, currInspectedTile);
                    break;
                case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination:
                    // Selected current inspected entity
                    CurrentState.Combat.SelectedEntityID = CurrentState.Combat.InspectedEntityID;
                    bool stepSuccess =
                        ManualPath.StepToTile(
                            GridDelegates.GetTileFromPosition(CurrentState.Combat.InspectedTilePosition));
                    if (!stepSuccess) {
                        Debug.LogError(
                            $"GameStateManager.SetCurrentPlayerPhaseState: Failed to step to {CurrentState.Combat.InspectedTilePosition}");
                    }

                    Debug.Log($"GameStateManager.SetCurrentPlayerPhaseState: Manual path is now: {ManualPath}");
                    GridDelegates.InvokeOnManualPathPreview(ManualPath);
                    Tile currentTile = GridDelegates.GetTileFromPosition(CurrentState.Combat.InspectedTilePosition);
                    GridDelegates.InvokeOnInspectedTileChanged(currentTile, currentTile); // Force update to show walkable tiles
                    break;
                case GameStateEnums.PlayerPhaseState.UnitMovingToDestination:
                    GridDelegates.InvokeOnSetTileVisualSelectionAnim(CurrentState.Combat.InspectedTilePosition, false);
                    break;
                case GameStateEnums.PlayerPhaseState.UnitActionMenu:
                    ManualPath.Clear();
                    UIDelegates.InvokeOnSetCombatActionMenuVisibility(true);
                    Debug.Log(
                        $"GameStateManager.SetCurrentPlayerPhaseState: SelectedEntityID is {CurrentState.Combat.SelectedEntityID}");
                    SetInspectedTile(CurrentState.Combat.InspectedTilePosition);
                    GridDelegates.InvokeOnSetTileVisualSelectionAnim(CurrentState.Combat.InspectedTilePosition, true);
                    break;
                case GameStateEnums.PlayerPhaseState.UnitSelectTarget: break;
                case GameStateEnums.PlayerPhaseState.UnitAttackCutscene: break;
                case GameStateEnums.PlayerPhaseState.None: break;
            }
        }

        private GameStateData GetCurrentGameState() {
            return CurrentState;
        }

        // ==============================
        // HELPERS
        // ==============================
        private bool HandleSetInspectedTile(Vector2Int coordinate) {
            switch (CurrentState.Combat.PlayerPhase) {
                case GameStateEnums.PlayerPhaseState.SelectUnitToControl:
                    SetInspectedTile(coordinate);
                    // UpdateAutomaticPathPreview(coordinate);
                    return true;
                case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination:
                    return AddCoordinateToManualPath(coordinate);
                case GameStateEnums.PlayerPhaseState.UnitMovingToDestination: return false;
                case GameStateEnums.PlayerPhaseState.UnitActionMenu: return false;
                case GameStateEnums.PlayerPhaseState.UnitSelectTarget: return false;
                case GameStateEnums.PlayerPhaseState.UnitAttackCutscene: return false;
                case GameStateEnums.PlayerPhaseState.None: return false;
                default:
                    throw new InvalidEnumArgumentException(
                        "GameStateManager.HandleSetInspectedTile: Invalid manual move selection state!");
            }
        }

        private bool AddCoordinateToManualPath(Vector2Int coordinate) {
            // Get terrain data at coordinate
            Tile tileAtCoordinate = GridDelegates.GetTileFromPosition(coordinate);
            // Forbid adding coordinate to manual path if out of movement range
            // Forbid adding coordinate to manual path if currently inspecting an entity of an enemy faction
            GridEntity inspectedEntity = EntityDelegates.GetGridEntityByID(CurrentState.Combat.InspectedEntityID);
            GridEntity selectedEntity = EntityDelegates.GetGridEntityByID(CurrentState.Combat.SelectedEntityID);
            if (ManualPath.Tiles.FirstOrDefault(tile => tile.Position == coordinate) == null) {
                if (CurrentState.Combat.InspectedEntityID != -1 && inspectedEntity.Faction != selectedEntity.Faction) {
                    Debug.LogWarning(
                        "GameStateManager.AddCoordinateToManualPath: An entity of an opposing faction is blocking movement to this tile.");
                    return false;
                }

                int movementCostUsed = GetManualPathUsedMovementCost();
                if (selectedEntity.MovementRange - movementCostUsed - tileAtCoordinate.MovementCost < 0) {
                    Debug.LogWarning(
                        $"GameStateManager.AddCoordinateToManualPath: Not enough movement cost (need {tileAtCoordinate.MovementCost} but have {selectedEntity.MovementRange - movementCostUsed} left: used {movementCostUsed}). Not adding coordinate {coordinate} to manual path. {string.Join(",", ManualPath.Tiles)}");
                    return false;
                }
            }

            bool stepSuccess = ManualPath.StepToTile(GridDelegates.GetTileFromPosition(coordinate));
            Debug.Log($"GameStateManager.AddCoordinateToManualPath: Manual path is now: {ManualPath}");
            if (stepSuccess) {
                SetInspectedTile(coordinate);
                GridDelegates.InvokeOnManualPathPreview(ManualPath);
            }
            else {
                Debug.LogWarning(
                    $"GameStateManager.AddCoordinateToManualPath: Illegal path. Restricting cursor movement. Cursor position according to GameState: {CurrentState.Combat.InspectedTilePosition}");
            }

            return stepSuccess;
        }

        private int GetManualPathUsedMovementCost() {
            int totalCost = 0;
            // Skip the starting tile
            for (int i = 1; i < ManualPath.Tiles.Count; i++) {
                totalCost += ManualPath.Tiles[i].MovementCost;
            }

            return totalCost;
        }

        private void UpdateAutomaticPathPreview(Vector2Int coordinate) {
            Tile newTile = GridDelegates.GetTileFromPosition(coordinate);
            GridEntity inspectedEntity = EntityDelegates.GetGridEntityByID(CurrentState.Combat.InspectedEntityID);
            Vector2Int startPosition = inspectedEntity?.GridPosition ?? newTile.Position;
            GridDelegates.InvokeOnAStarPathPreview(startPosition, startPosition);
        }

        private void SetInspectedTile(Vector2Int coordinate, bool focusCameraRig = true) {
            Tile newTile = GridDelegates.GetTileFromPosition(coordinate);
            Tile oldTile = GridDelegates.GetTileFromPosition(CurrentState.Combat.InspectedTilePosition);

            // Forbid the change if player is currently in manual path selection mode and the new tile is not walkable from the old tile
            if (CurrentState.Combat.PlayerPhase == GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination &&
                CurrentState.Combat.UnitMoveSelectionMode == GameStateEnums.UnitMoveSelectionMode.Manual) {
                int movementCostUsed = GetManualPathUsedMovementCost();
                Debug.Log($"GameStateManager.SetInspectedTile: MovementCostUsed: {movementCostUsed}");
            }
            
            CurrentState.Combat.InspectedTilePosition = newTile?.Position ??
                                                        throw new ArgumentException(
                                                            "GameStateManager.SetInspectedTile: Tile does not exist at position {coordinates}!");
            GridDelegates.InvokeOnInspectedTileChanged(oldTile, newTile);
            CurrentState.Combat.InspectedEntityID = newTile.IsOccupied ? newTile.Occupant.ID : -1;
            UIDelegates.InvokeOnTerrainUIUpdate(GridDelegates.GetTileFromPosition(CurrentState.Combat.InspectedTilePosition));
            if (CurrentState.Combat.UnitMoveSelectionMode == GameStateEnums.UnitMoveSelectionMode.Manual ||
                CurrentState.Combat.InspectedEntityID != -1) {
                // Focus camera rig onto position
                if (focusCameraRig) CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(CurrentState.Combat.InspectedTilePosition.x, 0, CurrentState.Combat.InspectedTilePosition.y));
                if (CurrentState.Combat.InspectedEntityID != -1) {
                    UIDelegates.InvokeOnEntityHUDUpdate(
                        EntityDelegates.GetGridEntityByID(CurrentState.Combat.InspectedEntityID));
                }
            }

           
        }

        private void GenerateRandomBiome(TileData tileData) {
            int replacedTiles = 0;
            int numTries = 32;
            Vector2Int gridDimensions = GridDelegates.GetGridDimensions();
            while (replacedTiles < numTries) {
                Vector2Int randomPosition =
                    new Vector2Int(Random.Range(0, gridDimensions.x), Random.Range(0, gridDimensions.y));
                Tile randomTile = GridDelegates.GetTileFromPosition(randomPosition);
                
                while (randomTile.IsOccupied) {
                    randomPosition = new Vector2Int(Random.Range(0, gridDimensions.x), Random.Range(0, gridDimensions.y));
                    randomTile = GridDelegates.GetTileFromPosition(randomPosition);
                }
                

                GridDelegates.InvokeOnSetTileTerrainType(randomPosition, tileData);
                replacedTiles++;
            }
        }

        private IEnumerator RunEnemyPhaseCoroutine() {
            while (CurrentState.Combat.ActorIDsRemaining.Count > 0) {
                // Select enemy to control
                CurrentState.Combat.EnemyPhase = GameStateEnums.EnemyPhaseState.SelectUnitToControl;
                int entityID = CurrentState.Combat.ActorIDsRemaining[0];
                GridEntity currentEntity = EntityDelegates.GetGridEntityByID(entityID);
                if (currentEntity == null) {
                    throw new Exception(
                        $"GameStateManager.RunEnemyPhaseCoroutine: Tried to search for enemy of ID {entityID} but could not find it!");
                }
                Tile oldTile = GridDelegates.GetTileFromPosition(CurrentState.Combat.InspectedTilePosition);
                CurrentState.Combat.InspectedEntityID = entityID;
                // Wait a frame for entityID to update correctly
                yield return new WaitForEndOfFrame();
                CurrentState.Combat.SelectedEntityID = entityID;
                SetInspectedTile(currentEntity.GridPosition, false);
                yield return new WaitForSeconds(2f);
                
                // Choose tile to move to
                CurrentState.Combat.EnemyPhase = GameStateEnums.EnemyPhaseState.SelectUnitMoveDestination;
                (HashSet<Tile> walkableTiles, HashSet<GridEntity> attackableEnemies) = currentEntity.GetWalkableTiles();
                Tile chosenRandomTile = walkableTiles.ElementAt(Random.Range(0, walkableTiles.Count));
                (bool reachable, List<Tile> path) =
                    AStar.CalculateBestPath(currentEntity.GridPosition, chosenRandomTile.Position);
                if (!reachable) {
                    throw new Exception(
                        $"GameStateManager.RunEnemyPhaseCoroutine: AStar could not find a path for the chosen random tile at position {chosenRandomTile.Position}!");
                }

                oldTile = GridDelegates.GetTileFromPosition(CurrentState.Combat.InspectedTilePosition); 
                SetInspectedTile(path[^1].Position, false);
                yield return new WaitForSeconds(.1f);

                // Move to destination
                CurrentState.Combat.EnemyPhase = GameStateEnums.EnemyPhaseState.UnitMovingToDestination;
                CurrentState.Combat.NextActorReady = false;
                currentEntity.MoveAlongPath(path);
                yield return new WaitUntil(() => CurrentState.Combat.NextActorReady);
                yield return new WaitForSeconds(path.Count > 0 ? 2f : 0f);
                
                // Contemplate action
                CurrentState.Combat.EnemyPhase = GameStateEnums.EnemyPhaseState.UnitContemplateAction;
                
                yield return new WaitForSeconds(1f);
                CurrentState.Combat.ActorIDsRemaining.RemoveAt(0);
                CurrentState.Combat.SelectedEntityID = -1;
                CurrentState.Combat.InspectedEntityID = -1;
            }

         
            GridDelegates.InvokeOnInspectedTileChanged(GridDelegates.GetTileFromPosition(CurrentState.Combat.InspectedTilePosition), null);
            
            CurrentState.Combat.EnemyPhase = GameStateEnums.EnemyPhaseState.None;
            AdvancePhase();
        }

        private IEnumerator RunEventPhaseCoroutine() {
            // Do something
            yield return new WaitForSeconds(2f);
            AdvancePhase();
        }
    }
}