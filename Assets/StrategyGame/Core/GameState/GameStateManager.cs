using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Grid;
using StrategyGame.Grid.GridData;
using StrategyGame.UI;
using Unity.Android.Gradle.Manifest;
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

    public override string ToString() { return string.Join(", ", Tiles); }
}

public class GameStateManager : MonoBehaviour {
    // ==============================
    // STRUCTS
    // ==============================
    public struct GameStateSnapshot {
        public GameStateEnums.PlayerPhaseState CurrentPlayerPhaseState;
        public GameStateEnums.UnitMoveSelectionMode CurrentUnitMoveSelectionMode;
        public GameStateEnums.TurnPhase CurrentTurnPhase;
    }

    // ==============================
    // FIELDS & PROPERTIES
    // ==============================
    public GameStateEnums.TurnPhase CurrentTurnPhase { get; private set; } = GameStateEnums.TurnPhase.Player;
    public GridEntity CurrentInspectedEntity { get; private set; }
    public GridEntity CurrentSelectedEntity { get; private set; }
    public Tile CurrentInspectedTile { get; private set; }

    public GameStateEnums.PlayerPhaseState CurrentPlayerPhaseState { get; private set; } =
        GameStateEnums.PlayerPhaseState.SelectUnitToControl;

    public GameStateEnums.UnitMoveSelectionMode CurrentUnitMoveSelectionMode { get; private set; } =
        GameStateEnums.UnitMoveSelectionMode.Manual;

    public ManualPath ManualPath { get; private set; }
    private Coroutine _coreGameLoop;

    // ==============================
    // MONOBEHAVIOUR LIFECYCLE
    // ==============================
    private void OnEnable() {
        ManualPath = new ManualPath();
        GameStateDelegates.OnGameStarted += StartGame;
        GameStateDelegates.OnUnitMoveSelectionChanged += SetCurrentUnitMoveSelectionMode;
        GameStateDelegates.OnPlayerPhaseStateChanged += SetCurrentPlayerPhaseState;
        GridDelegates.GetInspectedTile = () => CurrentInspectedTile;
        GameStateDelegates.GetCurrentInspectedEntity = () => CurrentInspectedEntity;
        GameStateDelegates.GetCurrentSelectedEntity = () => CurrentSelectedEntity;
        GameStateDelegates.GetManualPath = () => ManualPath;
        GameStateDelegates.GetCurrentGameStateSnapshot = GetCurrentGameStateSnapshot;
        GridDelegates.SetInspectedTile = HandleSetInspectedTile;
        GameStateDelegates.ManualPathSelectionGetSpentMovementCost = GetManualPathUsedMovementCost;
    }

    private void OnDisable() {
        GameStateDelegates.OnGameStarted -= StartGame;
        GameStateDelegates.OnUnitMoveSelectionChanged -= SetCurrentUnitMoveSelectionMode;
        GameStateDelegates.OnPlayerPhaseStateChanged -= SetCurrentPlayerPhaseState;
        GridDelegates.GetInspectedTile = null;
        GameStateDelegates.GetCurrentInspectedEntity = null;
        GameStateDelegates.GetCurrentSelectedEntity = null;
        GameStateDelegates.GetManualPath = null;
        GameStateDelegates.GetCurrentGameStateSnapshot = null;
        GridDelegates.SetInspectedTile = null;
        GameStateDelegates.ManualPathSelectionGetSpentMovementCost = null;
    }

    // ==============================
    // CORE METHODS
    // ==============================
    public void AdvancePhase() {
        SetTurnPhaseState((GameStateEnums.TurnPhase)(((int)CurrentTurnPhase + 1) %
                                                     Enum.GetValues(typeof(GameStateEnums.TurnPhase)).Length));
        GameStateDelegates.InvokeOnPhaseChanged(CurrentTurnPhase);
    }

    private void StartGame() {
        Debug.Log("Starting Game");
        List<UnitSpawnQuery> entities = new List<UnitSpawnQuery>();
        entities.Add(new UnitSpawnQuery {
            UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Soldier"),
            SpawnPosition = new Vector2Int(0, 0)
        });
        entities.Add(new UnitSpawnQuery {
            UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Orc"), SpawnPosition = new Vector2Int(1, 1)
        });
        entities.Add(new UnitSpawnQuery {
            UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Archer"),
            SpawnPosition = new Vector2Int(2, 2)
        });
        entities.Add(new UnitSpawnQuery {
            UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Soldier"),
            SpawnPosition = new Vector2Int(5, 1)
        });
        entities.Add(new UnitSpawnQuery {
            UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Orc"), SpawnPosition = new Vector2Int(3, 6)
        });
        entities.Add(new UnitSpawnQuery {
            UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Elite Orc"),
            SpawnPosition = new Vector2Int(4, 4)
        });
        entities.Add(new UnitSpawnQuery {
            UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Elite Orc"),
            SpawnPosition = new Vector2Int(0, 1)
        });
        EntityDelegates.SpawnUnits(entities);
        GenerateRandomBiome(Resources.Load<TileData>("ScriptableObjects/Tiles/Mountains"));
        GenerateRandomBiome(Resources.Load<TileData>("ScriptableObjects/Tiles/Forest"));

        // Start core game loop
        SetInspectedTile(Vector2Int.zero);
        _coreGameLoop = StartCoroutine(CoreGameLoop());
    }

    private void SetTurnPhaseState(GameStateEnums.TurnPhase phase) {
        if (phase == CurrentTurnPhase)
            return;
        CurrentTurnPhase = phase;
    }

    // ==============================
    // CORE GAME LOOP
    // ==============================
    private IEnumerator CoreGameLoop() {
        while (true) {
            switch (CurrentTurnPhase) {
                case GameStateEnums.TurnPhase.Player: HandlePlayerPhaseState(); break;
                case GameStateEnums.TurnPhase.Enemy: HandleEnemyPhaseState(); break;
                case GameStateEnums.TurnPhase.Event: HandleEventPhaseState(); break;
                default: throw new InvalidEnumArgumentException("GameStateManager.CoreGameLoop: Invalid turn phase!");
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
        switch (CurrentPlayerPhaseState) {
            case GameStateEnums.PlayerPhaseState.SelectUnitToControl:
            break;
            case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination:
                GridDelegates.InvokeOnSetTileVisualSelectionAnim(CurrentInspectedTile.Position, true); 
            break;
            case GameStateEnums.PlayerPhaseState.UnitMovingToDestination:
                if (CurrentSelectedEntity == null) {
                    Debug.LogWarning("GameStateManager.HandlePlayerPhaseState: CurrentSelectedEntity is null");
                    return;
                }
                // Remove selection
                
                // Focus camera rig onto position
                // Debug.Log("Current selected entity: " + CurrentSelectedEntity);
                Vector3 visualPosition =
                    EntityDelegates.GetEntityVisualTransformByID(CurrentSelectedEntity.ID).position;
                CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(visualPosition.x, visualPosition.y,
                    visualPosition.z));
            break;
            case GameStateEnums.PlayerPhaseState.UnitActionMenu:
            break;
            case GameStateEnums.PlayerPhaseState.UnitSelectTarget:
                GridDelegates.InvokeOnSetTileVisualSelectionAnim(CurrentInspectedTile.Position, true); 
            break;
            case GameStateEnums.PlayerPhaseState.UnitAttackCutscene:
            break;
            case GameStateEnums.PlayerPhaseState.None:
            break;
            default:
                throw new InvalidEnumArgumentException(
                    "GameStateManager.HandlePlayerPhaseState: Invalid CurrentPlayerPhaseState!");
        }
    }

    private void HandleEnemyPhaseState() { }

    private void HandleEventPhaseState() { }

    // ==============================
    // CORE METHODS
    // ==============================
    private void SetCurrentUnitMoveSelectionMode(GameStateEnums.UnitMoveSelectionMode mode) {
        if (CurrentUnitMoveSelectionMode == mode)
            return;
        CurrentUnitMoveSelectionMode = mode;
        switch (CurrentUnitMoveSelectionMode) {
            case GameStateEnums.UnitMoveSelectionMode.Manual:
                InputDelegates.InvokeOnSetMouseRaycastEnabled(false); break;
            case GameStateEnums.UnitMoveSelectionMode.Automatic:
                InputDelegates.InvokeOnSetMouseRaycastEnabled(true); break;
            case GameStateEnums.UnitMoveSelectionMode.None: InputDelegates.InvokeOnSetMouseRaycastEnabled(false); break;
            default:
                throw new InvalidEnumArgumentException(
                    "GameStateManager.SetCurrentUnitMoveSelectionMode: Invalid unit move selection mode!");
        }
    }

    private void SetCurrentPlayerPhaseState(GameStateEnums.PlayerPhaseState phase) {
        if (CurrentPlayerPhaseState == phase)
            return;
        CurrentPlayerPhaseState = phase;
        
        switch (CurrentPlayerPhaseState) {
            case GameStateEnums.PlayerPhaseState.SelectUnitToControl: 
                ManualPath.Clear();
                GridDelegates.InvokeOnSetTileVisualSelectionAnim(CurrentInspectedTile.Position, false);
            break;
            case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination:
                // Selected current inspected entity
                CurrentSelectedEntity = CurrentInspectedEntity;
                bool stepSuccess = ManualPath.StepToTile(CurrentInspectedTile);
                if (!stepSuccess) {
                    Debug.LogError(
                        $"GameStateManager.SetCurrentPlayerPhaseState: Failed to step to {CurrentInspectedTile.Position}");
                }
                Debug.Log($"GameStateManager.SetCurrentPlayerPhaseState: Manual path is now: {ManualPath}");
                GridDelegates.InvokeOnManualPathPreview(ManualPath);
            break;
            case GameStateEnums.PlayerPhaseState.UnitMovingToDestination: 
                GridDelegates.InvokeOnSetTileVisualSelectionAnim(CurrentInspectedTile.Position, false);
            break;
            case GameStateEnums.PlayerPhaseState.UnitActionMenu: 
                ManualPath.Clear();
                UIDelegates.InvokeOnSetCombatActionMenuVisibility(true);
                Debug.Log($"GameStateManager.SetCurrentPlayerPhaseState: CurrentSelectedEntity is {CurrentSelectedEntity}");
                SetInspectedTile(CurrentInspectedTile.Position);
                GridDelegates.InvokeOnSetTileVisualSelectionAnim(CurrentInspectedTile.Position, true);
            break;
            case GameStateEnums.PlayerPhaseState.UnitSelectTarget: break;
            case GameStateEnums.PlayerPhaseState.UnitAttackCutscene: break;
            case GameStateEnums.PlayerPhaseState.None: break;
        }
    }

    private GameStateSnapshot GetCurrentGameStateSnapshot() {
        return new GameStateSnapshot {
            CurrentPlayerPhaseState = CurrentPlayerPhaseState,
            CurrentUnitMoveSelectionMode = CurrentUnitMoveSelectionMode, CurrentTurnPhase = CurrentTurnPhase,
        };
    }

    // ==============================
    // HELPERS
    // ==============================
    private bool HandleSetInspectedTile(Vector2Int coordinate) {
        switch (CurrentPlayerPhaseState) {
            case GameStateEnums.PlayerPhaseState.SelectUnitToControl:
                SetInspectedTile(coordinate);
                UpdateAutomaticPathPreview(coordinate);
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
        if (ManualPath.Tiles.FirstOrDefault(tile => tile.Position == coordinate) == null) {
            if (CurrentInspectedEntity != null && CurrentInspectedEntity.Faction != CurrentSelectedEntity.Faction) {
                Debug.LogWarning("GameStateManager.AddCoordinateToManualPath: An entity of an opposing faction is blocking movement to this tile.");
                return false;
            }
            int movementCostUsed = GetManualPathUsedMovementCost();
            if (CurrentSelectedEntity.MovementRange - movementCostUsed - tileAtCoordinate.MovementCost < 0) {
                Debug.LogWarning($"GameStateManager.AddCoordinateToManualPath: Not enough movement cost (need {tileAtCoordinate.MovementCost} but have {CurrentSelectedEntity.MovementRange - movementCostUsed} left: used {movementCostUsed}). Not adding coordinate {coordinate} to manual path. {string.Join(",", ManualPath.Tiles)}");
                return false;
            }
        }

        bool stepSuccess = ManualPath.StepToTile(GridDelegates.GetTileFromPosition(coordinate));
        Debug.Log($"GameStateManager.AddCoordinateToManualPath: Manual path is now: {ManualPath}");
        if (stepSuccess) {
            SetInspectedTile(coordinate);
            GridDelegates.InvokeOnManualPathPreview(ManualPath);
        } else {
            Debug.LogWarning(
                $"GameStateManager.AddCoordinateToManualPath: Illegal path. Restricting cursor movement. Cursor position according to GameState: {CurrentInspectedTile.Position}");
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
        Vector2Int startPosition = CurrentInspectedEntity?.GridPosition ?? newTile.Position;
        GridDelegates.InvokeOnAStarPathPreview(startPosition, startPosition);
    }

    private void SetInspectedTile(Vector2Int coordinate) {
        Tile newTile = GridDelegates.GetTileFromPosition(coordinate);
        Tile oldTile = CurrentInspectedTile;

        // Forbid the change if player is currently in manual path selection mode and the new tile is not walkable from the old tile
        if (CurrentPlayerPhaseState == GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination &&
            CurrentUnitMoveSelectionMode == GameStateEnums.UnitMoveSelectionMode.Manual) {
            int movementCostUsed = GetManualPathUsedMovementCost();
            Debug.Log($"GameStateManager.SetInspectedTile: MovementCostUsed: {movementCostUsed}");
        }


        // if (Equals(oldTile, newTile))
        //     return;
        
        // Clear any visual selection on old tile
        // GridDelegates.InvokeOnSetTileVisualSelectionAnim(CurrentInspectedTile.Position, false); 
        
        CurrentInspectedTile = newTile ??
                               throw new ArgumentException(
                                   "GameStateManager.SetInspectedTile: Tile does not exist at position {coordinates}!");
        GridDelegates.InvokeOnInspectedTileChanged(oldTile, newTile);
        GridEntity previousSelectedEntity = CurrentInspectedEntity;
        CurrentInspectedEntity = newTile.IsOccupied ? newTile.Occupant : null;
        Debug.Log($"GameStateManager.SetInspectedTile: Set CurrentInspectedEntity to {CurrentInspectedEntity}");
        UIDelegates.InvokeOnTerrainUIUpdate(CurrentInspectedTile);
        if (CurrentUnitMoveSelectionMode == GameStateEnums.UnitMoveSelectionMode.Manual ||
            CurrentInspectedEntity != null) {
            // Focus camera rig onto position
            CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(CurrentInspectedTile.Position.x, 0,
                CurrentInspectedTile.Position.y));
            if (CurrentInspectedEntity != null) {
                UIDelegates.InvokeOnEntityHUDUpdate(CurrentInspectedEntity);
            }
        }

        if (previousSelectedEntity == null && CurrentInspectedEntity == null)
            return;
        if (previousSelectedEntity != null && CurrentInspectedEntity != null)
            return;
        if (CurrentInspectedEntity != null) {
            UIAnimationDelegates.InvokeOnPlayAnimation(AnimatorCategory.EntityHUD, "TweenIn");
        } else if (CurrentInspectedEntity == null) {
            UIAnimationDelegates.InvokeOnPlayAnimation(AnimatorCategory.EntityHUD, "TweenOut");
        }
    }

    private void GenerateRandomBiome(TileData tileData, bool overrideNonDefault = false) {
        int placedMountains = 0;
        int numTries = 32;
        Vector2Int gridDimensions = GridDelegates.GetGridDimensions();
        while (placedMountains < numTries) {
            Vector2Int randomPosition =
                new Vector2Int(Random.Range(0, gridDimensions.x), Random.Range(0, gridDimensions.y));
            Tile randomTile = GridDelegates.GetTileFromPosition(randomPosition);
            if (!overrideNonDefault && randomTile.InitData.name != "Grasslands")
                continue;
            if (tileData.MovementCost > 99) {
                while (randomTile.IsOccupied) {
                    randomPosition = new Vector2Int(Random.Range(0, gridDimensions.x),
                        Random.Range(0, gridDimensions.y));
                    randomTile = GridDelegates.GetTileFromPosition(randomPosition);
                }
            }

            GridDelegates.InvokeOnSetTileTerrainType(randomPosition, tileData);
            placedMountains++;
        }
    }
}
}