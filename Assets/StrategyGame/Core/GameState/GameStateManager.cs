using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using StrategyGame.AI;
using StrategyGame.Combat;
using StrategyGame.Combat.Cinematics;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Factions;
using StrategyGame.Grid;
using StrategyGame.Grid.GridData;
using StrategyGame.Grid.Rendering;
using StrategyGame.UI;
using StrategyGame.UI.Menus;
using StrategyGame.Utils;
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
        private static readonly int Death = Animator.StringToHash("Death");
        private static readonly int Hurt = Animator.StringToHash("Hurt");
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        [SerializeField] private GameStateData currentState;
        public GameStateData CurrentState => currentState;
        public ManualPath ManualPath { get; private set; }
        private Coroutine _coreGameLoop;
        private Coroutine _enemyPhaseCoroutine;
        private Coroutine _eventPhaseCoroutine;
        private Coroutine _combatCinematicsCoroutine;

        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        private void OnEnable() {
            ManualPath = new ManualPath();
            currentState = new GameStateData {
                Combat = new CombatStateData { TurnPhase = GameStateEnums.TurnPhase.None, PlayerPhase = GameStateEnums.PlayerPhaseState.SelectUnitToControl, UnitMoveSelectionMode = GameStateEnums.UnitMoveSelectionMode.Manual, },
                MasterState = GameStateEnums.MasterState.Combat
            };
            GameStateDelegates.OnGameStarted += StartGame;
            GameStateDelegates.OnUnitMoveSelectionChanged += SetCurrentUnitMoveSelectionMode;
            GameStateDelegates.OnPlayerPhaseStateChanged += SetCurrentPlayerPhaseState;
            GameStateDelegates.OnAdvanceTurnPhase += AdvancePhase;
            GameStateDelegates.OnApplyAttackOutcome += ApplyAttackOutcome;
            GameStateDelegates.OnFinalizePlayerAction += FinalizePlayerAction;
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
            GameStateDelegates.OnApplyAttackOutcome -= ApplyAttackOutcome;
            GameStateDelegates.OnFinalizePlayerAction -= FinalizePlayerAction;
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
            if (_combatCinematicsCoroutine != null) {
                StopCoroutine(_combatCinematicsCoroutine);
                _combatCinematicsCoroutine = null;
            }
        }

        // ==============================
        // CORE METHODS
        // ==============================
        private void AdvancePhase() {
            GameStateEnums.TurnPhase nextPhaseState = (GameStateEnums.TurnPhase)(((int)CurrentState.Combat.TurnPhase + 1) % Enum.GetValues(typeof(GameStateEnums.TurnPhase)).Length);
            List<int> enemyIDs = EntityDelegates.GetAllGridEntityIDsByFaction(Faction.Enemy, true);
            List<int> playerIDs = EntityDelegates.GetAllGridEntityIDsByFaction(Faction.Player, true);
            if (nextPhaseState == 0) {
                CurrentState.Combat.TurnPhaseCycle += 1;
                if (CurrentState.Combat.TurnPhaseCycle > 1) {
                    // Reduce cooldowns
                    foreach (int playerID in playerIDs) {
                        GridEntity currEntity = EntityDelegates.GetGridEntityByID(playerID);
                        foreach (KeyValuePair<int, int> kvp in currEntity.AbilityMap.ToList()) {
                            currEntity.AbilityMap[kvp.Key] = Math.Max(0, kvp.Value - 1);
                        }
                    }
                    // Also reduce for enemies
                }
            }
            SetTurnPhaseState(nextPhaseState);
            Debug.Log($"GameStateManager.AdvancePhase: Setting turn phase state to {CurrentState.Combat.TurnPhase}");
            GameStateDelegates.InvokeOnTurnPhaseChanged(CurrentState.Combat.TurnPhase);
            // Refresh broken flags on entities
            
            foreach (int enemyID in enemyIDs) {
                GridEntity currEntity = EntityDelegates.GetGridEntityByID(enemyID);
                currEntity.IsBroken = false;
            }
         
            foreach (int playerID in playerIDs) {
                GridEntity currEntity = EntityDelegates.GetGridEntityByID(playerID);
                currEntity.IsBroken = false;
            }
        }
        private void StartGame() {
            Debug.Log("Starting Game");
            List<UnitSpawnQuery> units = new List<UnitSpawnQuery> {
                new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Player/Soldier"), SpawnPosition = new Vector2Int(4, 4) },
                new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Player/Archer"), SpawnPosition = new Vector2Int(4, 5) },
                new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Player/Soldier"), SpawnPosition = new Vector2Int(4, 6) },
                new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Player/Calvary"), SpawnPosition = new Vector2Int(5, 4) },
                new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Player/Calvary"), SpawnPosition = new Vector2Int(5, 5) },
                new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Player/Priest"), SpawnPosition = new Vector2Int(5, 6) },
                new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Enemy/Orc"), SpawnPosition = new Vector2Int(6, 7) },
                new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Enemy/Orc"), SpawnPosition = new Vector2Int(6, 8) },
                new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Enemy/Elite Orc"), SpawnPosition = new Vector2Int(6, 9) },
                new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Enemy/Elite Orc"), SpawnPosition = new Vector2Int(7, 7) },
                new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Enemy/Skeleton Archer"), SpawnPosition = new Vector2Int(7, 8) },
                new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Enemy/Skeleton Archer"), SpawnPosition = new Vector2Int(7, 9) },
                new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Enemy/Skeleton Archer"), SpawnPosition = new Vector2Int(8, 8) }
            };

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
            if (CurrentState.Combat.TurnPhase is GameStateEnums.TurnPhase.Player or GameStateEnums.TurnPhase.Enemy)
                UIDelegates.InvokeOnPlayPhaseBannerAnimationSequence();

            // Play ui animations
            // Depending on turn phase, fill ActorsRemaining with the entities from the current phase.
            switch (CurrentState.Combat.TurnPhase) {
                case GameStateEnums.TurnPhase.Player:
                    InputDelegates.InvokeOnSetGridCursorVisibility(true);
                    CurrentState.Combat.HighestPriorityTargetEntityID = -1;
                    // Clear danger zone highlights
                    if (InputDelegates.GetDangerZoneVisible())
                        GridDelegates.InvokeOnRefreshDangerZoneVisibility();
                    CurrentState.Combat.ActorIDsRemaining = EntityDelegates.GetAllGridEntityIDsByFaction(Faction.Player, false);
                    CurrentState.Combat.PlayersCycleDeque = new LinkedList<int>(CurrentState.Combat.ActorIDsRemaining);
                    CurrentState.Combat.PlayerPhase = GameStateEnums.PlayerPhaseState.SelectUnitToControl;
                    InputDelegates.InvokeOnReinstateGridCursorPosition(null);
                    break;
                case GameStateEnums.TurnPhase.Enemy:
                    InputDelegates.InvokeOnSetGridCursorVisibility(false);
                    // Clear danger zone highlights
                    GridDelegates.InvokeOnSetDangerZoneVisibility(false); // doesn't change input state
                    CurrentState.Combat.ActorIDsRemaining = EntityDelegates.GetAllGridEntityIDsByFaction(Faction.Enemy, false);
                    // Sort by decreasing agility
                    CurrentState.Combat.ActorIDsRemaining = CurrentState.Combat.ActorIDsRemaining.OrderByDescending(id => EntityDelegates.GetGridEntityByID(id).Agility).ToList();
                    // Automate enemy actions
                    _enemyPhaseCoroutine = StartCoroutine(RunEnemyPhaseCoroutine());
                    break;
                case GameStateEnums.TurnPhase.Event:
                    // Run any events this phase cycle
                    _eventPhaseCoroutine = StartCoroutine(RunEventPhaseCoroutine()); break;
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
                    case GameStateEnums.TurnPhase.None: AdvancePhase(); break;
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
            switch (CurrentState.Combat.PlayerPhase) {
                case GameStateEnums.PlayerPhaseState.SelectUnitToControl: break;
                case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination: break;
                case GameStateEnums.PlayerPhaseState.UnitMovingToDestination:
                    if (CurrentState.Combat.SelectedEntityID == -1) {
                        Debug.LogWarning("GameStateManager.HandlePlayerPhaseState: CurrentSelectedEntity is null");
                        return;
                    }
                    // Remove selection

                    // Focus camera rig onto position
                    // Debug.Log("Current selected entity: " + CurrentSelectedEntity);
                    Vector3 visualPosition = EntityVisualDelegates.GetEntityVisualTransformByID(CurrentState.Combat.SelectedEntityID).position;
                    CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(visualPosition.x, visualPosition.y, visualPosition.z));
                    break;
                case GameStateEnums.PlayerPhaseState.UnitActionMenu: break;
                case GameStateEnums.PlayerPhaseState.UnitSelectTarget:
                    // GridDelegates.InvokeOnSetTileVisualSelectionAnim(CurrentState.Combat.InspectedTilePosition, true);
                    break;
                case GameStateEnums.PlayerPhaseState.UnitAttackCutscene: break;
                case GameStateEnums.PlayerPhaseState.None: break;
                default: throw new Exception("GameStateManager.HandlePlayerPhaseState: Invalid PlayerPhase state!");
            }
        }
        private void HandleEnemyPhaseState() {
            Transform entityVisualTransform = EntityVisualDelegates.GetEntityVisualTransformByID(CurrentState.Combat.SelectedEntityID);
            if (entityVisualTransform != null) {
                Vector3 visualPosition = entityVisualTransform.position;
                switch (CurrentState.Combat.EnemyPhase) {
                    case GameStateEnums.EnemyPhaseState.SelectUnitToControl: CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(visualPosition.x, visualPosition.y, visualPosition.z)); break;
                    case GameStateEnums.EnemyPhaseState.SelectUnitMoveDestination: CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(visualPosition.x, visualPosition.y, visualPosition.z)); break;
                    case GameStateEnums.EnemyPhaseState.UnitMovingToDestination: CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(visualPosition.x, visualPosition.y, visualPosition.z)); break;
                    case GameStateEnums.EnemyPhaseState.UnitContemplateAction: CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(visualPosition.x, visualPosition.y, visualPosition.z)); break;
                    case GameStateEnums.EnemyPhaseState.UnitSelectTarget: CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(visualPosition.x, visualPosition.y, visualPosition.z)); break;
                    case GameStateEnums.EnemyPhaseState.UnitAttackCutscene: break;
                    case GameStateEnums.EnemyPhaseState.None: break;
                    default: throw new Exception("GameStateManager.HandleEnemyPhaseState: Invalid EnemyPhase state!");
                }
            } else {
                Debug.LogWarning($"GameStateManager.HandleEnemyPhaseState: Current frame doesn't have selected entity (id is {CurrentState.Combat.SelectedEntityID}).");
                switch (CurrentState.Combat.EnemyPhase) {
                    case GameStateEnums.EnemyPhaseState.SelectUnitToControl: break;
                    case GameStateEnums.EnemyPhaseState.SelectUnitMoveDestination: break;
                    case GameStateEnums.EnemyPhaseState.UnitMovingToDestination: break;
                    case GameStateEnums.EnemyPhaseState.UnitContemplateAction: break;
                    case GameStateEnums.EnemyPhaseState.UnitSelectTarget: break;
                    case GameStateEnums.EnemyPhaseState.UnitAttackCutscene: break;
                    case GameStateEnums.EnemyPhaseState.None: break;
                    default: throw new Exception("GameStateManager.HandleEnemyPhaseState: Invalid EnemyPhase state!");
                }
            }
        }

        private void HandleEventPhaseState() { }

        // ==============================
        // CORE METHODS
        // ==============================
        private void SetCurrentUnitMoveSelectionMode(GameStateEnums.UnitMoveSelectionMode mode) {
            if (CurrentState.Combat.UnitMoveSelectionMode == mode)
                return;
            CurrentState.Combat.UnitMoveSelectionMode = mode;
            switch (CurrentState.Combat.UnitMoveSelectionMode) {
                case GameStateEnums.UnitMoveSelectionMode.Manual: InputDelegates.InvokeOnSetMouseRaycastEnabled(false); break;
                case GameStateEnums.UnitMoveSelectionMode.Automatic: InputDelegates.InvokeOnSetMouseRaycastEnabled(true); break;
                case GameStateEnums.UnitMoveSelectionMode.None: InputDelegates.InvokeOnSetMouseRaycastEnabled(false); break;
                default: throw new InvalidEnumArgumentException("GameStateManager.SetCurrentUnitMoveSelectionMode: Invalid unit move selection mode!");
            }
        }
        private void SetCurrentPlayerPhaseState(GameStateEnums.PlayerPhaseState phase) {
            if (CurrentState.Combat.PlayerPhase == phase)
                return;
            GameStateEnums.PlayerPhaseState previousState = CurrentState.Combat.PlayerPhase;
            CurrentState.Combat.PlayerPhase = phase;
            Tile currInspectedTile = GridDelegates.GetTileFromPosition(CurrentState.Combat.InspectedTilePosition);
            switch (CurrentState.Combat.PlayerPhase) {
                case GameStateEnums.PlayerPhaseState.SelectUnitToControl:
                    ManualPath.Clear();
                    CurrentState.Combat.HighestPriorityTargetEntityID = -1;
                    CurrentState.Combat.SelectedEntityID = -1;
                    
                    GridDelegates.InvokeOnInspectedTileChanged(currInspectedTile, currInspectedTile);
                    break;
                case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination:
                    
                    if (previousState == GameStateEnums.PlayerPhaseState.UnitActionMenu) { // Handle going back from action menu
                        // Clear attackable tiles
                        GridDelegates.InvokeOnClearAttackRangePreview();
                        // Teleport entity back to where they were before movement
                        ManualPath.Clear();
                        GridEntity currSelectedEntity = EntityDelegates.GetGridEntityByID(CurrentState.Combat.SelectedEntityID);
                        (Vector2Int playerPosBeforeMovement, bool _) = CurrentState.Combat.PlayerPositionBeforeMovementAndFlipX;
                        Tile previousTile = GridDelegates.GetTileFromPosition(playerPosBeforeMovement);
                        List<Tile> pseudoPath = new List<Tile> { previousTile };
                        currSelectedEntity.MoveAlongPath(pseudoPath, true);
                        
                        // Hide action menu
                        UIDelegates.InvokeOnSetCombatActionMenuVisibility(false, ActionMenuPage.Main);
                        // Reset cursor position back to where it was before
                        InputDelegates.InvokeOnReinstateGridCursorPosition(playerPosBeforeMovement);
                    } else if (previousState == GameStateEnums.PlayerPhaseState.SelectUnitToControl) {
                        Transform entityTransform = EntityVisualDelegates.GetEntityVisualTransformByID(CurrentState.Combat.InspectedEntityID);
                        SpriteRenderer spriteRenderer = entityTransform.GetComponentInChildren<SpriteRenderer>();
                        CurrentState.Combat.PlayerPositionBeforeMovementAndFlipX = (CurrentState.Combat.InspectedTilePosition, spriteRenderer.flipX);
                    }
                    // Selected current inspected entity
                    CurrentState.Combat.SelectedEntityID = CurrentState.Combat.InspectedEntityID;
                    bool stepSuccess = ManualPath.StepToTile(GridDelegates.GetTileFromPosition(CurrentState.Combat.InspectedTilePosition));
                    if (!stepSuccess) {
                        Debug.LogWarning($"GameStateManager.SetCurrentPlayerPhaseState: Failed to step to {CurrentState.Combat.InspectedTilePosition}. Ignore if player tried to undo move.");
                    }
                    Debug.Log($"GameStateManager.SetCurrentPlayerPhaseState: Manual path is now: {ManualPath}");
                    GridDelegates.InvokeOnManualPathPreview(ManualPath);
                    SetInspectedTile(CurrentState.Combat.InspectedTilePosition); // Force update to show walkable tiles
                    break;
                case GameStateEnums.PlayerPhaseState.UnitMovingToDestination: break;
                case GameStateEnums.PlayerPhaseState.UnitActionMenu:
                    ManualPath.Clear();
                    InputDelegates.InvokeOnSetGridCursorVisibility(true);
                    UIDelegates.InvokeOnSetCombatActionMenuVisibility(true, CurrentState.Combat.CurrentSelectedSkillID != -1 || CurrentState.Combat.CurrentSelectedItemID != -1 ? ActionMenuPage.Current : ActionMenuPage.Main);
                    Debug.Log($"GameStateManager.SetCurrentPlayerPhaseState: SelectedEntityID is {CurrentState.Combat.SelectedEntityID}");
                    SetInspectedTile(CurrentState.Combat.InspectedTilePosition); // Force update to show walkable tiles
                    InputDelegates.InvokeOnReinstateGridCursorPosition(EntityDelegates.GetGridEntityByID(CurrentState.Combat.SelectedEntityID).GridPosition);
                    break;
                case GameStateEnums.PlayerPhaseState.UnitSelectTarget:
                    ManualPath.Clear();
                    // Populate the EnemiesCycleDeque
                    GridEntity attackingEntity = EntityDelegates.GetGridEntityByID(CurrentState.Combat.SelectedEntityID);
                    HashSet<GridEntity> attackableEntities = attackingEntity.GetAttackableEntitiesAtPosition(attackingEntity.GridPosition);
                    // Sort attackableEntities by increasing distance from player
                    List<GridEntity> sortedAttackableEntities = attackableEntities.OrderBy(e => Manhattan.Distance(e.GridPosition, attackingEntity.GridPosition)).ToList();
                    // If there is one enemy with increased priority, cycle to that one first
                    if (CurrentState.Combat.HighestPriorityTargetEntityID != -1) {
                        // swap elements at index 0 and index of the highest priority target
                        int indexOfHighestPriorityTarget = sortedAttackableEntities.FindIndex(e => e.ID == CurrentState.Combat.HighestPriorityTargetEntityID);
                        if (indexOfHighestPriorityTarget == -1)
                            throw new Exception("GameStateManager.SetCurrentPlayerPhaseState: Index of highest priority target not found!");
                        ListUtils.Swap(sortedAttackableEntities, 0, indexOfHighestPriorityTarget);
                    }
                    CurrentState.Combat.EnemiesCycleDeque = new LinkedList<int>(sortedAttackableEntities.Select(e => e.ID));
                    Vector2Int firstTargetPosition = EntityDelegates.GetGridEntityByID(CurrentState.Combat.EnemiesCycleDeque.First.Value).GridPosition;
                    InputDelegates.InvokeOnReinstateGridCursorPosition(firstTargetPosition);
                    SetInspectedTile(firstTargetPosition);
                    GridDelegates.InvokeOnManualMarkTilesWithAttackableEntities();
                    break;
                case GameStateEnums.PlayerPhaseState.UnitAttackCutscene: 
                    UIDelegates.InvokeOnResetCombatActionMenuIndices();
                    _combatCinematicsCoroutine = StartCoroutine(CombatCinematicsDelegates.GetDirector().PlayCombat());
                    break;
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
                case GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination: return AddCoordinateToManualPath(coordinate);
                case GameStateEnums.PlayerPhaseState.UnitMovingToDestination: return false;
                case GameStateEnums.PlayerPhaseState.UnitActionMenu: return false;
                case GameStateEnums.PlayerPhaseState.UnitSelectTarget:
                    SetInspectedTile(coordinate);
                    return true;
                case GameStateEnums.PlayerPhaseState.UnitAttackCutscene: return false;
                case GameStateEnums.PlayerPhaseState.None: return false;
                default: throw new InvalidEnumArgumentException("GameStateManager.HandleSetInspectedTile: Invalid manual move selection state!");
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

                if (CurrentState.Combat.InspectedEntityID != -1) {
                    if (inspectedEntity.Faction != selectedEntity.Faction) {
                        Debug.LogWarning("GameStateManager.AddCoordinateToManualPath: An entity of an opposing faction is blocking movement to this tile.");
                        return false;
                    }
                    // Still disallow if the coordinate to step to has an entity of an unfriendly faction
                    if (CurrentState.Combat.InspectedEntityID != CurrentState.Combat.SelectedEntityID && tileAtCoordinate.Occupant != null && !selectedEntity.IsFriendlyWith(tileAtCoordinate.Occupant)) {
                        Debug.LogWarning("GameStateManager.AddCoordinateToManualPath: Cannot immediately attack target because the attack spot is occupied by an ally.");
                        return false;
                    }
                }

                // Allow melee units that can reach the target to immediately attack
                int movementCostUsed = GetManualPathUsedMovementCost();
                if (selectedEntity.MovementRange - movementCostUsed == 0) {
                    if (tileAtCoordinate.Occupant == null || tileAtCoordinate.Occupant.Faction == selectedEntity.Faction) {
                        return false;
                    }
                }
                if (selectedEntity.MovementRange - movementCostUsed - tileAtCoordinate.MovementCost < 0) {
                    if (tileAtCoordinate.Occupant == null || tileAtCoordinate.Occupant.Faction == selectedEntity.Faction) {
                        Debug.LogWarning(
                            $"GameStateManager.AddCoordinateToManualPath: Not enough movement cost (need {tileAtCoordinate.MovementCost} but have {selectedEntity.MovementRange - movementCostUsed} left: used {movementCostUsed}). Not adding coordinate {coordinate} to manual path. {string.Join(",", ManualPath.Tiles)}");
                        return false;
                    }
                    Debug.Log($"GameStateManager.AddCoordinateToManualPath: Special case, enabling quick attack selection.");
                }
            }

            // Also prevent the feature of immediately attacking target if min and max range are not 1
            if (tileAtCoordinate.Occupant != null && tileAtCoordinate.Occupant.Faction != selectedEntity.Faction && (selectedEntity.Weapon.MinAttackRange != 1 || selectedEntity.Weapon.MaxAttackRange != 1)) {
                Debug.LogWarning("GameStateManager.AddCoordinateToManualPath: Cannot add tile to manual path because immediate attacking is not supported for ranged units!");
                return false;
            }
            bool stepSuccess = ManualPath.StepToTile(GridDelegates.GetTileFromPosition(coordinate));
            Debug.Log($"GameStateManager.AddCoordinateToManualPath: Manual path is now: {ManualPath}");
            if (stepSuccess) {
                SetInspectedTile(coordinate);
                GridDelegates.InvokeOnManualPathPreview(ManualPath);
            } else {
                Debug.LogWarning($"GameStateManager.AddCoordinateToManualPath: Illegal path. Restricting cursor movement. Cursor position according to GameState: {CurrentState.Combat.InspectedTilePosition}");
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
            if (CurrentState.Combat.PlayerPhase == GameStateEnums.PlayerPhaseState.SelectUnitMoveDestination && CurrentState.Combat.UnitMoveSelectionMode == GameStateEnums.UnitMoveSelectionMode.Manual) {
                int movementCostUsed = GetManualPathUsedMovementCost();
                Debug.Log($"GameStateManager.SetInspectedTile: MovementCostUsed: {movementCostUsed}");
            }
            CurrentState.Combat.InspectedTilePosition = newTile?.Position ?? throw new ArgumentException("GameStateManager.SetInspectedTile: Tile does not exist at position {coordinates}!");
            GridDelegates.InvokeOnInspectedTileChanged(oldTile, newTile);
            CurrentState.Combat.InspectedEntityID = newTile.Occupant?.ID ?? -1;
            UIDelegates.InvokeOnTerrainUIUpdate(GridDelegates.GetTileFromPosition(CurrentState.Combat.InspectedTilePosition));
            if (CurrentState.Combat.UnitMoveSelectionMode == GameStateEnums.UnitMoveSelectionMode.Manual || CurrentState.Combat.InspectedEntityID != -1) {
                // Focus camera rig onto position
                if (focusCameraRig)
                    CameraDelegates.InvokeOnSetCameraRigPosition(new Vector3(CurrentState.Combat.InspectedTilePosition.x, 0, CurrentState.Combat.InspectedTilePosition.y));
                if (CurrentState.Combat.InspectedEntityID != -1) {
                    UIDelegates.InvokeOnEntityHUDUpdate(EntityDelegates.GetGridEntityByID(CurrentState.Combat.InspectedEntityID));
                }
            }
        }
        private void GenerateRandomBiome(TileData tileData) {
            int replacedTiles = 0;
            int numTries = 32;
            Vector2Int gridDimensions = GridDelegates.GetGridDimensions();
            while (replacedTiles < numTries) {
                Vector2Int randomPosition = new Vector2Int(Random.Range(0, gridDimensions.x), Random.Range(0, gridDimensions.y));
                Tile randomTile = GridDelegates.GetTileFromPosition(randomPosition);
                while (randomTile.Occupant != null) {
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
                    throw new Exception($"GameStateManager.RunEnemyPhaseCoroutine: Tried to search for enemy of ID {entityID} but could not find it!");
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
                HashSet<Tile> walkableTiles = currentEntity.GetWalkableTiles(true);
                HashSet<Tile> tilesWhereAttackingIsPossible = currentEntity.GetTilesWhereAttackingIsPossible();
                HashSet<Tile> hashSetToPickFrom = tilesWhereAttackingIsPossible.Count > 0 ? tilesWhereAttackingIsPossible : walkableTiles;
                if (tilesWhereAttackingIsPossible.Count > 0) {
                    Debug.Log($"GameStateManager.RunEnemyPhaseCoroutine: {currentEntity.DisplayName} has {tilesWhereAttackingIsPossible.Count} tiles to choose from to attack.");
                    if (tilesWhereAttackingIsPossible.Count == 1) {
                        Debug.Log($"GameStateManager.RunEnemyPhaseCoroutine: That one tile's position is {tilesWhereAttackingIsPossible.First()}.");
                    }
                } else {
                    Debug.Log($"GameStateManager.RunEnemyPhaseCoroutine: {currentEntity.DisplayName} has no tiles to choose from to attack. Walkable tiles count is {hashSetToPickFrom.Count}");
                    // Narrow down to the one tile closest to any player
                    List<int> playerActorIDs = EntityDelegates.GetAllGridEntityIDsByFaction(Faction.Player, false);
                    if (playerActorIDs.Count > 0) {
                        // Get closest player entity
                        GridEntity closestPlayer = EntityDelegates.GetGridEntityByID(playerActorIDs.OrderBy(id => (EntityDelegates.GetGridEntityByID(id).GridPosition - currentEntity.GridPosition).magnitude ).First());
                        Debug.Log($"GameStateManager.RunEnemyPhaseCoroutine: Closest player is {closestPlayer.DisplayName}");
                        hashSetToPickFrom = new HashSet<Tile>{hashSetToPickFrom.Where(t => t.Occupant == null).OrderBy(t => (t.Position - closestPlayer.GridPosition).magnitude).First()};
                        Debug.Log($"GameStateManager.RunEnemyPhaseCoroutine: {currentEntity.DisplayName}@{currentEntity.GridPosition}'s only position can be {hashSetToPickFrom.First()}");
                    }
                }

                // Remove allies to get all truly walkable tiles
                hashSetToPickFrom = hashSetToPickFrom.Where(t => t.Occupant == null).ToHashSet();
                Debug.Log($"GameStateManager.RunEnemyPhaseCoroutine: hashSetToPickFrom's size after filter: {hashSetToPickFrom.Count}");
                if (hashSetToPickFrom.Count > 0) {
                    Tile chosenRandomTile = hashSetToPickFrom.ElementAt(Random.Range(0, hashSetToPickFrom.Count));
                    (bool reachable, List<Tile> path) = AStar.CalculateBestPath(currentEntity.GridPosition, chosenRandomTile.Position);
                    if (!reachable) {
                        throw new Exception($"GameStateManager.RunEnemyPhaseCoroutine: AStar could not find a path for the chosen random tile at position {chosenRandomTile.Position}!");
                    }
                    SetInspectedTile(path[^1].Position, false);
                    yield return new WaitForSeconds(.1f);

                    // Move to destination
                    CurrentState.Combat.EnemyPhase = GameStateEnums.EnemyPhaseState.UnitMovingToDestination;
                    CurrentState.Combat.NextActorReady = false;
                    currentEntity.MoveAlongPath(path);
                    yield return new WaitUntil(() => CurrentState.Combat.NextActorReady);
                    yield return new WaitForSeconds(path.Count > 0 ? 1f : 0f);
                    // Attack weakest target in range
                    HashSet<GridEntity> targetsInRange = currentEntity.GetAttackableEntitiesAtPosition(currentEntity.GridPosition);
                    if (targetsInRange.Count > 0) {
                        GridEntity chosenTargetEntity = targetsInRange.OrderBy(e => e.Health).First();
                        // Make them face the target
                        currentEntity.VisualFace(chosenTargetEntity);
                        CombatPreview combatPreview = CombatResolver.SimulateAttackPreview(currentEntity.GetCombatStats(),
                            chosenTargetEntity.GetCombatStats(),
                            currentEntity.BasicAttack,
                            chosenTargetEntity.GetAttackableEntitiesAtPosition(chosenTargetEntity.GridPosition).FirstOrDefault(e => e.ID == currentEntity.ID) != null);
                        CurrentState.Combat.CombatPreview = combatPreview;
                        CombatOutcome attackOutcome = CombatResolver.ResolveCombatFromPreview(combatPreview);
                        CurrentState.Combat.EnemyActorFinishedCombatCinematic = false;
                        CombatDirector combatDirector = CombatCinematicsDelegates.GetDirector();
                        combatDirector.InitializeCinematicData(currentEntity, chosenTargetEntity, attackOutcome);
                        StartCoroutine(combatDirector.PlayCombat());
                        CurrentState.Combat.EnemyPhase = GameStateEnums.EnemyPhaseState.UnitAttackCutscene;
                        yield return new WaitUntil(() => CurrentState.Combat.EnemyActorFinishedCombatCinematic);
                    }
                }

                // Contemplate action
                CurrentState.Combat.EnemyPhase = GameStateEnums.EnemyPhaseState.UnitContemplateAction;
                yield return new WaitForSeconds(1f);

                if (CurrentState.Combat.ActorIDsRemaining.Contains(currentEntity.ID)) {
                    CurrentState.Combat.ActorIDsRemaining.RemoveAt(0);
                }
                
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
        private void ApplyAttackOutcome(CombatOutcome outcome) {
            // Reset selected item/ability states
            CurrentState.Combat.CurrentSelectedSkillID = -1;
            CurrentState.Combat.CurrentSelectedItemID = -1;
            
            GridEntity attackerEntity = EntityDelegates.GetGridEntityByID(outcome.AttackerID);
            GridEntity defenderEntity = EntityDelegates.GetGridEntityByID(outcome.DefenderID);
            defenderEntity.TakeDamage(outcome.DamageDealt);
            attackerEntity.TakeDamage(outcome.CounterDamageDealt);
            if (!attackerEntity.IsBroken) attackerEntity.IsBroken = outcome.AttackerBrokenThisSimulation;
            if (!defenderEntity.IsBroken) defenderEntity.IsBroken = outcome.DefenderBrokenThisSimulation;
            // Animate entity visuals
            EntityVisual attackerVisual = EntityVisualDelegates.GetEntityVisualTransformByID(attackerEntity.ID).GetComponent<EntityVisual>();
            EntityVisual defenderVisual = EntityVisualDelegates.GetEntityVisualTransformByID(defenderEntity.ID).GetComponent<EntityVisual>();
            if (outcome.CounterDamageDealt > 0) attackerVisual.Animator.SetTrigger(Hurt);
            if (outcome.DamageDealt > 0) defenderVisual.Animator.SetTrigger(Hurt);
            if (attackerEntity.Health == 0) attackerVisual.Animator.SetTrigger(Death);
            if (defenderEntity.Health == 0) defenderVisual.Animator.SetTrigger(Death);
        }
        private void FinalizePlayerAction() {
            CurrentState.Combat.ActorIDsRemaining.Remove(CurrentState.Combat.SelectedEntityID);
            CurrentState.Combat.PlayersCycleDeque.Remove(CurrentState.Combat.SelectedEntityID);
            GameStateDelegates.InvokeOnPlayerPhaseStateChanged(CurrentState.Combat.ActorIDsRemaining.Count == 0 ? GameStateEnums.PlayerPhaseState.None : GameStateEnums.PlayerPhaseState.SelectUnitToControl);
            if (CurrentState.Combat.ActorIDsRemaining.Count == 0) {
                GameStateDelegates.InvokeOnAdvanceTurnPhase();
            }
            Debug.Log("GameStateManager.FinalizePlayerAction: Finalized player action.");
        }
    }
}
