using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Grid;
using StrategyGame.Grid.GridData;
using StrategyGame.UI;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StrategyGame.Core.GameState {
    public class GameStateManager : MonoBehaviour {
        // ==============================
        // STRUCTS
        // ==============================
        public struct GameStateSnapshot {
            public GameStateEnums.PlayerPhaseState CurrentPlayerPhaseState;
            public GameStateEnums.UnitMoveSelectionMode CurrentUnitMoveSelectionMode;
            public GameStateEnums.ManualMoveSelectionState  CurrentManualMoveSelectionState;
            public GameStateEnums.TurnPhase CurrentTurnPhase;
        }
        
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        public GameStateEnums.TurnPhase CurrentTurnPhase { get; private set; } = GameStateEnums.TurnPhase.Player;
        public GridEntity CurrentInspectedEntity { get; private set; }
        public GridEntity CurrentSelectedEntity { get; private set; }
        public Tile CurrentInspectedTile {get; private set;}
        public GameStateEnums.PlayerPhaseState CurrentPlayerPhaseState { get; private set; } = GameStateEnums.PlayerPhaseState.SelectUnitToMove;
        public GameStateEnums.UnitMoveSelectionMode CurrentUnitMoveSelectionMode { get; private set; } = GameStateEnums.UnitMoveSelectionMode.Manual;
        public GameStateEnums.ManualMoveSelectionState CurrentManualMoveSelectionState { get; private set; } = GameStateEnums.ManualMoveSelectionState.AwaitingUnitSelection;

        public List<Tile> TilesAlongManualPath;
        
        
        private Coroutine _coreGameLoop;
        
        
        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        private void OnEnable() {
            GameStateDelegates.OnGameStarted += StartGame;
            GridDelegates.OnSelectTile += HandleOnSelectTile;
            GameStateDelegates.OnUnitMoveSelectionChanged += SetCurrentUnitMoveSelectionMode;
            GameStateDelegates.OnManualMoveSelectionChanged += SetCurrentManualMoveSelectionState;
           
            
            GridDelegates.GetInspectedTile = () => CurrentInspectedTile;
            GameStateDelegates.GetCurrentInspectedEntity  = () => CurrentInspectedEntity;
            GameStateDelegates.GetCurrentSelectedEntity  = () => CurrentSelectedEntity;
            GameStateDelegates.GetCurrentGameStateSnapshot = GetCurrentGameStateSnapshot;
            
        }
        private void OnDisable() {
            GameStateDelegates.OnGameStarted -= StartGame;
            GridDelegates.OnSelectTile -= HandleOnSelectTile;
            GameStateDelegates.OnUnitMoveSelectionChanged -= SetCurrentUnitMoveSelectionMode;
            GameStateDelegates.OnManualMoveSelectionChanged -= SetCurrentManualMoveSelectionState;
            
            GridDelegates.GetInspectedTile = null;
            GameStateDelegates.GetCurrentInspectedEntity = null;
            GameStateDelegates.GetCurrentSelectedEntity = null;
            GameStateDelegates.GetCurrentGameStateSnapshot = null;
        }
        
        
        // ==============================
        // CORE METHODS
        // ==============================
        public void AdvancePhase() {
            SetTurnPhaseState((GameStateEnums.TurnPhase)(((int)CurrentTurnPhase + 1) % Enum.GetValues(typeof(GameStateEnums.TurnPhase)).Length));
            GameStateDelegates.InvokeOnPhaseChanged(CurrentTurnPhase);
        }
        private void StartGame() {
            Debug.Log("Starting Game");
            List<UnitSpawnQuery> entities = new List<UnitSpawnQuery>();
            entities.Add(new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Soldier"), SpawnPosition = new Vector2Int(0, 0) });
            entities.Add(new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Orc"), SpawnPosition = new Vector2Int(1, 1) });
            entities.Add(new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Archer"), SpawnPosition = new Vector2Int(2, 2) });
            entities.Add(new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Soldier"), SpawnPosition = new Vector2Int(5, 1) });
            entities.Add(new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Orc"), SpawnPosition = new Vector2Int(3, 6) });
            entities.Add(new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Elite Orc"), SpawnPosition = new Vector2Int(4, 4) });    
            entities.Add(new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Elite Orc"), SpawnPosition = new Vector2Int(0, 1) });           
            EntityDelegates.SpawnUnits(entities);

           GenerateRandomBiome(Resources.Load<TileData>("ScriptableObjects/Tiles/Mountains"));
           GenerateRandomBiome(Resources.Load<TileData>("ScriptableObjects/Tiles/Forest"));
            
            // Start core game loop
            SetInspectedTile(Vector2Int.zero);
            _coreGameLoop = StartCoroutine(CoreGameLoop());
        }
        private void SetTurnPhaseState(GameStateEnums.TurnPhase phase) {
            if (phase == CurrentTurnPhase) return;
            CurrentTurnPhase = phase;
        }
        
        
        // ==============================
        // CORE GAME LOOP
        // ==============================
        private IEnumerator CoreGameLoop() {
            while (true) {
                switch (CurrentTurnPhase) {
                    case GameStateEnums.TurnPhase.Player:
                        HandlePlayerPhaseState();
                        break;
                    case GameStateEnums.TurnPhase.Enemy:
                        HandleEnemyPhaseState();
                        break;
                    case GameStateEnums.TurnPhase.Event:
                        HandleEventPhaseState();
                        break;
                    default:
                        throw new InvalidEnumArgumentException("Invalid turn phase!");
                }
                yield return new WaitForEndOfFrame();
            }
        }

        
        
        // ==============================
        // PHASE HANDLERS
        // ==============================
        private void HandlePlayerPhaseState() {
            
        }

        private void HandleEnemyPhaseState() {
            
        }

        private void HandleEventPhaseState() {
            
        }

        
        // ==============================
        // CORE METHODS
        // ==============================
        private void SetCurrentUnitMoveSelectionMode(GameStateEnums.UnitMoveSelectionMode mode) {
            if (CurrentUnitMoveSelectionMode == mode) return;
            CurrentUnitMoveSelectionMode = mode;
            switch (CurrentUnitMoveSelectionMode) {
                case GameStateEnums.UnitMoveSelectionMode.Manual:
                    InputDelegates.InvokeOnSetMouseRaycastEnabled(false);
                    break;
                case GameStateEnums.UnitMoveSelectionMode.Automatic:
                    InputDelegates.InvokeOnSetMouseRaycastEnabled(true);
                    break;
                case GameStateEnums.UnitMoveSelectionMode.None:
                    InputDelegates.InvokeOnSetMouseRaycastEnabled(false);
                    break;
                default:
                    throw new InvalidEnumArgumentException("Invalid unit move selection mode!");
            }
        }

        private void SetCurrentManualMoveSelectionState(GameStateEnums.ManualMoveSelectionState state) {
            if  (CurrentManualMoveSelectionState == state) return;
            CurrentManualMoveSelectionState = state;
            switch (CurrentManualMoveSelectionState) {
                case GameStateEnums.ManualMoveSelectionState.AwaitingUnitSelection:
                    break;
                case GameStateEnums.ManualMoveSelectionState.FormingPath:
                    break;
                case GameStateEnums.ManualMoveSelectionState.None:
                    break;
                default:
                    throw new InvalidEnumArgumentException("Invalid manual move selection state!");
            }
        }
        
        private GameStateSnapshot GetCurrentGameStateSnapshot() {
            return new GameStateSnapshot {
                CurrentPlayerPhaseState = CurrentPlayerPhaseState,
                CurrentUnitMoveSelectionMode = CurrentUnitMoveSelectionMode,
                CurrentManualMoveSelectionState = CurrentManualMoveSelectionState,
                CurrentTurnPhase = CurrentTurnPhase,
            };
        }
        
        // ==============================
        // HELPERS
        // ==============================
        private void HandleOnSelectTile(Vector2Int coordinate) {
            SetInspectedTile(coordinate);
            switch (CurrentManualMoveSelectionState) {
                case GameStateEnums.ManualMoveSelectionState.None:
                    return;
                case GameStateEnums.ManualMoveSelectionState.AwaitingUnitSelection:
                    UpdateAutomaticPathPreview(coordinate);
                    break;
                case GameStateEnums.ManualMoveSelectionState.FormingPath:
                    AddCoordinateToManualPath(coordinate);
                    break;
                default:
                    throw new InvalidEnumArgumentException("Invalid manual move selection state!");
            }
        }
        private void AddCoordinateToManualPath(Vector2Int coordinate) {
            Tile newTile = GridDelegates.GetTileFromPosition(coordinate);
            Tile oldTile = CurrentInspectedTile;
            if (Equals(oldTile, newTile)) {
                Debug.LogWarning("Not adding coordinate to manual path because newTile and oldTile are the same!");
                return;
            }
            TilesAlongManualPath.Add(GridDelegates.GetTileFromPosition(coordinate));
        }

        private void UpdateAutomaticPathPreview(Vector2Int coordinate) {
            Tile newTile = GridDelegates.GetTileFromPosition(coordinate);
            Vector2Int startPosition = CurrentInspectedEntity?.GridPosition ?? newTile.Position;
            GridDelegates.InvokeOnUpdatePathPreview(startPosition, startPosition);
        }
        
        private void SetInspectedTile(Vector2Int coordinate) {
            Tile newTile = GridDelegates.GetTileFromPosition(coordinate);
            Tile oldTile = CurrentInspectedTile;
            if (Equals(oldTile, newTile)) return;
            CurrentInspectedTile = newTile ?? throw new ArgumentException("Tile does not exist at position {coordinates}!");
            GridDelegates.InvokeOnSetInspectedTile(oldTile, newTile);
            GridEntity previousSelectedEntity = CurrentInspectedEntity;
            CurrentInspectedEntity = newTile.IsOccupied ? newTile.Occupant : null;
            Vector2Int startPosition = CurrentInspectedEntity?.GridPosition ?? newTile.Position;
            
            UIDelegates.InvokeOnTerrainUIUpdate(CurrentInspectedTile);
            
            if (CurrentUnitMoveSelectionMode == GameStateEnums.UnitMoveSelectionMode.Manual || CurrentInspectedEntity != null) {
                // Focus camera rig onto position
                CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(CurrentInspectedTile.Position.x, 0, CurrentInspectedTile.Position.y));
                if (CurrentInspectedEntity != null) {
                    UIDelegates.InvokeOnEntityHUDUpdate(CurrentInspectedEntity);
                }
            }

            if (previousSelectedEntity == null && CurrentInspectedEntity == null) return;
            if (previousSelectedEntity != null && CurrentInspectedEntity != null) return;
            
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
                Vector2Int randomPosition = new Vector2Int(Random.Range(0, gridDimensions.x), Random.Range(0, gridDimensions.y));
                Tile randomTile = GridDelegates.GetTileFromPosition(randomPosition);
                if (!overrideNonDefault && randomTile.InitData.name != "Grasslands") continue;
                if (tileData.MovementCost > 99) {
                    while (randomTile.IsOccupied) {
                        randomPosition = new Vector2Int(Random.Range(0, gridDimensions.x), Random.Range(0, gridDimensions.y));
                        randomTile = GridDelegates.GetTileFromPosition(randomPosition);
                    }
                }
                GridDelegates.InvokeOnSetTileTerrainType(randomPosition, tileData);
                placedMountains++;
            }
        }
    }
}
