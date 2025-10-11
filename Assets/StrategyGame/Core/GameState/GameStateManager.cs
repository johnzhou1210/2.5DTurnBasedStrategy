using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid;
using StrategyGame.Grid.GridData;
using StrategyGame.UI;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StrategyGame.Core.GameState {
    public class GameStateManager : MonoBehaviour {
        // ==============================
        // ENUMS
        // ==============================
        public enum TurnPhase {
            Player,
            Enemy,
            Event,
            None
        }

        public enum PlayerPhaseState {
            SelectUnitToMove,
            SelectUnitMoveDestination,
            UnitActionMenu,
            UnitSelectTarget,
            UnitAttackCutscene,
            None
        }

        public enum UnitMoveSelectionMode {
            Manual,
            Automatic,
            None
        }
        
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        public TurnPhase CurrentPhase { get; private set; }
        public GridEntity CurrentSelectedEntity { get; private set; }
        public Tile CurrentSelectedTile {get; private set;}
        public PlayerPhaseState CurrentPlayerPhaseState { get; private set; }
        public UnitMoveSelectionMode CurrentUnitMoveSelectionMode { get; private set; }
        private Coroutine _coreGameLoop;
        
        
        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        private void OnEnable() {
            GameStateDelegates.OnGameStarted += StartGame;
            GridDelegates.OnSelectTile += SetSelectedTile;
            GameStateDelegates.OnUnitMoveSelectionChanged += SetCurrentUnitMoveSelectionMode;
           
            
            GridDelegates.GetSelectedTile = () => CurrentSelectedTile;
            GameStateDelegates.GetCurrentSelectedEntity  = () => CurrentSelectedEntity;
            GameStateDelegates.GetCurrentUnitMoveSelectionMode = () => CurrentUnitMoveSelectionMode;
        }
        private void OnDisable() {
            GameStateDelegates.OnGameStarted -= StartGame;
            GridDelegates.OnSelectTile -= SetSelectedTile;
            GameStateDelegates.OnUnitMoveSelectionChanged -= SetCurrentUnitMoveSelectionMode;
           
            
            GridDelegates.GetSelectedTile = null;
            GameStateDelegates.GetCurrentSelectedEntity = null;
            GameStateDelegates.GetCurrentUnitMoveSelectionMode = null;
        }
        
        
        // ==============================
        // CORE METHODS
        // ==============================
        public void AdvancePhase() {
            SetTurnPhaseState((TurnPhase)(((int)CurrentPhase + 1) % Enum.GetValues(typeof(TurnPhase)).Length));
            GameStateDelegates.InvokeOnPhaseChanged(CurrentPhase);
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

           GenerateRandomMountains();
            
            // Start core game loop
            SetSelectedTile(Vector2Int.zero);
            _coreGameLoop = StartCoroutine(CoreGameLoop());
        }
        private void SetTurnPhaseState(TurnPhase phase) {
            if (phase == CurrentPhase) return;
            CurrentPhase = phase;
        }
        
        
        // ==============================
        // CORE GAME LOOP
        // ==============================
        private IEnumerator CoreGameLoop() {
            while (true) {
                switch (CurrentPhase) {
                    case TurnPhase.Player:
                        HandlePlayerPhaseState();
                        break;
                    case TurnPhase.Enemy:
                        HandleEnemyPhaseState();
                        break;
                    case TurnPhase.Event:
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
        private void SetCurrentUnitMoveSelectionMode(UnitMoveSelectionMode mode) {
            if (CurrentUnitMoveSelectionMode == mode) return;
            CurrentUnitMoveSelectionMode = mode;
            switch (CurrentUnitMoveSelectionMode) {
                case UnitMoveSelectionMode.Manual:
                    InputDelegates.InvokeOnSetMouseRaycastEnabled(false);
                    break;
                case UnitMoveSelectionMode.Automatic:
                    InputDelegates.InvokeOnSetMouseRaycastEnabled(true);
                    break;
                case UnitMoveSelectionMode.None:
                    InputDelegates.InvokeOnSetMouseRaycastEnabled(false);
                    break;
                default:
                    throw new InvalidEnumArgumentException("Invalid unit move selection mode!");
            }
        }
        
        // ==============================
        // HELPERS
        // ==============================
        private void SetSelectedTile(Vector2Int coordinates) {
            Tile newTile = GridDelegates.GetTileFromPosition(coordinates);
            Tile oldTile = CurrentSelectedTile;
            if (Equals(oldTile, newTile)) return;
            CurrentSelectedTile = newTile ?? throw new ArgumentException("Tile does not exist at position {coordinates}!");
            GridDelegates.InvokeOnSetSelectedTile(oldTile, newTile);
            GridEntity previousSelectedEntity = CurrentSelectedEntity;
            CurrentSelectedEntity = newTile.IsOccupied ? newTile.Occupant : null;
            Vector2Int startPosition = CurrentSelectedEntity?.GridPosition ?? newTile.Position;
            GridDelegates.InvokeOnUpdatePathPreview(startPosition, startPosition);
            
            
            if (CurrentUnitMoveSelectionMode == UnitMoveSelectionMode.Manual || CurrentSelectedEntity != null) {
                // Focus camera rig onto unit
                CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(CurrentSelectedTile.Position.x, 0, CurrentSelectedTile.Position.y));
                if (CurrentSelectedEntity != null) {
                    UIDelegates.InvokeOnEntityHUDUpdate(CurrentSelectedEntity);
                }
                
            }

            if (previousSelectedEntity == null && CurrentSelectedEntity == null) return;
            if (previousSelectedEntity != null && CurrentSelectedEntity != null) return;
            
            if (CurrentSelectedEntity != null) {
                UIAnimationDelegates.InvokeOnPlayAnimation(AnimatorCategory.EntityHUD, "TweenIn");
            } else if (CurrentSelectedEntity == null) {
                UIAnimationDelegates.InvokeOnPlayAnimation(AnimatorCategory.EntityHUD, "TweenOut");
            }
            
        }

        private void GenerateRandomMountains() {
            int placedMountains = 0;
            Vector2Int gridDimensions = GridDelegates.GetGridDimensions();
            while (placedMountains < 32) {
                Vector2Int randomPosition = new Vector2Int(Random.Range(0, gridDimensions.x), Random.Range(0, gridDimensions.y));
                while (GridDelegates.GetTileFromPosition(randomPosition).IsOccupied) {
                    randomPosition = new Vector2Int(Random.Range(0, gridDimensions.x), Random.Range(0, gridDimensions.y));
                }
                GridDelegates.InvokeOnMountainifyTile(randomPosition);
                placedMountains++;
            }
        }
    }
}
