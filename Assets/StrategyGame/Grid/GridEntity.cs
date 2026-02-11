using System;
using System.Collections.Generic;
using System.Linq;
using StrategyGame.Combat.Weapons;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.GameState;
using StrategyGame.Factions;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Grid {
    struct FloodFillQueueEntry {
        public Tile Tile;
        public int RemainingMovementPoints;
    }

    [Serializable]
    public abstract class GridEntity {
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        
        /* CORE */
        public Vector2Int GridPosition { get; private set; }
        public bool IsPassable { get; private set; } = true;
        public GridEntityData GridEntityData { get; protected set; }
        public WeaponData Weapon { get; protected set; }

        /* IDENTITY */
        private static int _nextID = 0;
        private Dictionary<string, int> _entityNameCounts = new Dictionary<string, int>();
        
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public int ID { get; private set; }
        public Faction Faction { get; private set; }

        /* GAMEPLAY */
        public bool CanMove { get; private set; } = false;
        public bool CanAct { get; private set; } = false;
        public bool Selectable { get; private set; } = true;
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int MovementRange { get; private set; }
        public int VisionRange { get; private set; }
        public int Attack { get; private set; }
        public int Accuracy { get; private set; }
        public int Defense { get; private set; }
        public int Resistance { get; private set; }
        public int Agility { get; private set; }
        public int Evasion { get; private set; }

        /* VISUAL */
        public bool IsSelected { get; private set; } = false;
        public bool IsVisible { get; private set; } = true;
        public RuntimeAnimatorController AnimatorController { get; private set; }

        
        // ==============================
        // CONSTRUCTOR
        // ==============================
        protected GridEntity(GridEntityData gridEntityData) {
            ID = _nextID++;
            _entityNameCounts[gridEntityData.name] = _entityNameCounts.TryGetValue(gridEntityData.name, out int count) ? count + 1 : 1;
            GridEntityData = gridEntityData;
            Initialize();
        }
        
        // ==============================
        // INITIALIZATION
        // ==============================
        private void Initialize() {
            Health = GridEntityData.BaseHealth;
            MaxHealth = GridEntityData.BaseHealth;
            Faction = GridEntityData.FactionData.FactionEnum;
            MovementRange = GridEntityData.MovementRange;
            VisionRange = GridEntityData.VisionRange;
            DisplayName = GridEntityData.name;
            Attack = GridEntityData.BaseAttack;
            Accuracy = GridEntityData.BaseAccuracy;
            Defense = GridEntityData.BaseDefense;
            Resistance = GridEntityData.BaseResistance;
            Agility = GridEntityData.BaseAgility;
            Evasion = GridEntityData.BaseEvasion;
            AnimatorController = GridEntityData.AnimatorController;
        }
        
        
        // ==============================
        // METHODS
        // ==============================
        /// <summary>
        /// Call this if you want to move a unit on a path while respecting animations.
        /// </summary>
        /// <param name="path">The path the entity will move along.</param>
        /// <param name="respectAnimations">Boolean indicating whether the movement through each individual tile will be animated</param>
        public virtual void MoveAlongPath(List<Tile> path, bool respectAnimations = true) {
            Tile startTile = GridDelegates.GetTileFromPosition(GridPosition);
            Vector2Int endPosition = path[^1].Position;
            Tile endTile = GridDelegates.GetTileFromPosition(endPosition);
            SetGridPosition(endPosition);
            startTile.RemoveOccupant();
            endTile.AddOccupant(this);
            if (respectAnimations) {
                EntityDelegates.InvokeOnEntityMoveAlongPath(this, path);
            } else {
                // TODO: Implement version that instantly teleport without effects/animation
            }
            
        }
        
        public virtual GameObject GetSpritePrefab() {
            return GridEntityData.VisualPrefab;
        }
        public virtual HashSet<Tile> GetWalkableTiles(bool includeAllies = false) {
            return GetWalkableTilesAtPosition(GridPosition, MovementRange, includeAllies);
        }

        /// <summary>
        /// Gets walkable tiles at position.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <param name="availableMovementCost">How much movement cost remaining.</param>
        /// <param name="includeAllies">Include allies or not. Useful for path previewing.</param>
        /// <returns>Walkable tiles at position.</returns>
        public HashSet<Tile> GetWalkableTilesAtPosition(Vector2Int position, int availableMovementCost, bool includeAllies = false) {
            HashSet<Tile> walkableTiles = new HashSet<Tile>();
            Tile startTile = GridDelegates.GetTileFromPosition(position);
            if (startTile.Occupant != null && startTile.Occupant.Faction != Faction) {
                Debug.LogWarning("GridEntity.GetWalkableTilesAtPosition: Start tile is blocked by an opposing faction!");
                return walkableTiles;
            }
            // BFS / flood fill
            Queue<FloodFillQueueEntry> tilesToVisit = new Queue<FloodFillQueueEntry>();
            Dictionary<Tile, int> bestRemainingMovements = new Dictionary<Tile, int>();
            tilesToVisit.Enqueue(new FloodFillQueueEntry { Tile = startTile, RemainingMovementPoints = availableMovementCost });
            bestRemainingMovements[startTile] = availableMovementCost;
            while (tilesToVisit.Count > 0) {
                FloodFillQueueEntry entry = tilesToVisit.Dequeue();
                Tile currentTile = entry.Tile;
                int remainingMovement = entry.RemainingMovementPoints;
                foreach (var neighborPair in currentTile.Neighbors) {
                    Tile neighbor = neighborPair.Value;
                    if (neighbor == null)
                        continue;
                    bool isNeighborOccupied = neighbor.Occupant != null;
                    bool isEnemy = isNeighborOccupied && neighbor.Occupant.Faction != Faction;
                    bool isAlly = isNeighborOccupied && neighbor.Occupant.Faction == Faction;
                    if (isEnemy) {
                        continue;
                    }
                    if (isAlly && !includeAllies) continue;
                    int newRemainingMovement = remainingMovement - neighbor.MovementCost;
                    if (newRemainingMovement < 0)
                        continue; // Not enough movement points to enter
                    
                    // Update bestRemainingMovements if better
                    if (!bestRemainingMovements.ContainsKey(neighbor) || newRemainingMovement > bestRemainingMovements[neighbor]) {
                        bestRemainingMovements[neighbor] = newRemainingMovement;
                        tilesToVisit.Enqueue(new FloodFillQueueEntry { Tile = neighbor, RemainingMovementPoints = newRemainingMovement });
                    }
                }
            }
            // Collect all reachable tiles
            foreach (var entry in bestRemainingMovements) {
                walkableTiles.Add(entry.Key);
            }
            return walkableTiles;
        }


        public virtual HashSet<Tile> GetAttackableTilesAtPosition(Vector2Int position) {
            HashSet<Tile> tilesWithinRange = GridDelegates.GetTilesInRadius(position, 1, VisionRange).ToHashSet();
            return tilesWithinRange;
        }

        // Assumes the player has not moved yet.
        public HashSet<Tile> GetTilesWithinAttackRange() {
            // Debug.Log("GridEntity.GetTilesWithinAttackRange: Calling base version");
            HashSet<Tile> reachableTiles = GetWalkableTiles(true);
            HashSet<Tile> dangerTiles = new HashSet<Tile>();
            foreach (Tile tile in reachableTiles) {
                dangerTiles.UnionWith(GetAttackableTilesAtPosition(tile.Position));
            }
            return dangerTiles;
        }

        public HashSet<GridEntity> GetEntitiesWithinAttackRange() {
            HashSet<Tile> tilesWithinAttackRange = GetTilesWithinAttackRange();
            HashSet<GridEntity> attackableEntities = new HashSet<GridEntity>();
            foreach (Tile tile in tilesWithinAttackRange) {
                if (tile.Occupant != null) {
                    attackableEntities.Add(tile.Occupant);
                }
            }
            return attackableEntities;
        }

        public virtual HashSet<GridEntity> GetAttackableEntitiesAtPosition(Vector2Int position) {
            HashSet<Tile> attackableTiles = GetAttackableTilesAtPosition(position);
            HashSet<GridEntity> attackableEntities =  new HashSet<GridEntity>();
            foreach (Tile tile in attackableTiles) {
                if (tile.IsOccupied && !IsFriendlyWith(tile.Occupant)) {
                    attackableEntities.Add(tile.Occupant);
                }
            }
            return attackableEntities;
        }
        
        
        public void SetGridPosition(Vector2Int newPosition) {
            GridPosition = newPosition;
        }

        public override string ToString() {
            return DisplayName;
        }
        
        public bool IsFriendlyWith(GridEntity other) {
            return Faction == other.Faction;
        }

        public void TakeDamage(int amount) {
            Health = Math.Max(Health - amount, 0);
            FireHealthChangedEvents();
            if (Health == 0) {
                Die();
            }
        }

        private void FireHealthChangedEvents() {
            BillboardDelegates.InvokeOnHealthChanged(ID, Health, MaxHealth);
            UIDelegates.InvokeOnEntityHUDUpdate(this);
        }

        private void FireDeathEvents() {
            
        }

        public void Die() {
            // Free up tile since they died
            GridDelegates.GetTileFromPosition(GridPosition).RemoveOccupant();
            GameStateData currentState = GameStateDelegates.GetCurrentGameState();
            currentState.Combat.DeadEntityIDs.Add(ID);
            if (currentState.Combat.ActorIDsRemaining.Contains(ID)) {
                currentState.Combat.ActorIDsRemaining.Remove(ID);
            }
            FireDeathEvents();
        }


    }
}
