using System;
using System.Collections.Generic;
using StrategyGame.Core.Delegates;
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
        public GridEntityData GridEntityData { get; private set; }

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

        public HashSet<Tile> GetWalkableTilesAtPosition(Vector2Int position, int availableMovementCost, bool includeAllies = false) {
            HashSet<Tile> result = new HashSet<Tile>();
            Tile startTile = GridDelegates.GetTileFromPosition(position);

            if (startTile.Occupant != null && startTile.Occupant.Faction != Faction) {
                Debug.LogWarning("GridEntity.GetWalkableTilesAtPosition: Start tile is blocked by an opposing faction!");
                return result;
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
                    bool isOccupied = neighbor.Occupant != null;
                    bool isEnemy = isOccupied && neighbor.Occupant.Faction != Faction;
                    bool isAlly = isOccupied && neighbor.Occupant.Faction == Faction;
                    if (isEnemy) continue;
                    if (isAlly && !includeAllies) continue;
                    int newRemainingMovement = remainingMovement - neighbor.MovementCost;
                    if (newRemainingMovement < 0)
                        continue; // Not enough movement points to enter
                    
                    // Update bestRemainingMovements if better
                    if (!bestRemainingMovements.ContainsKey(neighbor) || newRemainingMovement > bestRemainingMovements[neighbor]) {
                        bestRemainingMovements[neighbor] = newRemainingMovement;
                        // Enemy tiles are reachable but NOT pass-through
                        tilesToVisit.Enqueue(new FloodFillQueueEntry { Tile = neighbor, RemainingMovementPoints = newRemainingMovement });
                    }
                }
            }

            // Collect all reachable tiles
            foreach (var entry in bestRemainingMovements) {
                result.Add(entry.Key);
            }
            
            return result;
        }
        
        public void SetGridPosition(Vector2Int newPosition) {
            GridPosition = newPosition;
        }

        public override string ToString() {
            return DisplayName;
        }


    }
}
