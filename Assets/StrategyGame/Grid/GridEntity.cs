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
    
    public abstract class GridEntity {
            // Core
            public Vector2Int GridPosition { get; private set; }
            public bool IsPassable { get; private set; } = true;
            public GridEntityData GridEntityData { get; private set; }

            // Identity
            private static int _nextID = 0;
            public string Name { get; private set; }
            public int ID { get; private set; }
            public FactionData FactionData { get; private set; }

            // Gameplay
            public bool CanMove { get; private set; } = false;
            public bool CanAct { get; private set; } = false;
            public bool Selectable { get; private set; } = true;
            
            // Visual
            public bool IsSelected { get; private set; } = false;
            public bool IsVisible { get; private set; } = true;

            // Optional
            public int Health { get; private set; }
            public int MaxHealth { get; private set; }
            public int MovementRange { get; private set; }
            public int VisionRange { get; private set; }

            // Events
            public event Action<GridEntity> OnSelected;
            public event Action<GridEntity, Vector2Int> OnMoved;

            
            // Constructor
            public GridEntity(GridEntityData gridEntityData) {
                ID = _nextID++;
                GridEntityData = gridEntityData;
                Initialize();
            }

            private void Initialize() {
                Health = GridEntityData.Health;
                MaxHealth = GridEntityData.MaxHealth;
                FactionData = GridEntityData.FactionData;
                MovementRange = GridEntityData.MovementRange;
                VisionRange = GridEntityData.VisionRange;
            }
            
            public virtual void Select() => OnSelected?.Invoke(this);

            public virtual void MoveTo(Vector2Int newPos) {
                GridPosition = newPos;
                OnMoved?.Invoke(this, newPos);
            }

            public virtual GameObject GetSpritePrefab() {
                return GridEntityData.VisualPrefab;
            }

            public virtual HashSet<Tile> GetWalkableTiles() {
                HashSet<Tile> result = new HashSet<Tile>();
                
                // Start Tile
                Debug.Log(GridPosition);
                Tile startTile = GridDelegates.GetTileFromPosition(GridPosition);
                
                // BFS
                Queue<FloodFillQueueEntry> tilesToVisit = new Queue<FloodFillQueueEntry>();
                Dictionary<Tile, int> bestRemainingMovements = new Dictionary<Tile, int>();
                
                tilesToVisit.Enqueue(new FloodFillQueueEntry {
                    Tile = startTile,
                    RemainingMovementPoints = MovementRange,
                });
                bestRemainingMovements[startTile] = MovementRange;

                while (tilesToVisit.Count > 0) {
                    // take out next tile and its remaining points
                    FloodFillQueueEntry poppedEntry = tilesToVisit.Dequeue();
                    Tile poppedTile = poppedEntry.Tile;
                    int poppedRemainingMovementPoints = poppedEntry.RemainingMovementPoints;
                    
                    foreach (var neighbor in poppedTile.Neighbors) {
                        if (neighbor.Value == null) continue;
                        int newRemainingMovementPoints = poppedRemainingMovementPoints - neighbor.Value.MovementCost;
                        if (newRemainingMovementPoints < 0) continue; // Skip (not enough movement points to enter)
                        
                        // If not seen, or if reached with more remaining movement points than before, update dictionary
                        if (!bestRemainingMovements.ContainsKey(neighbor.Value) || newRemainingMovementPoints > bestRemainingMovements[neighbor.Value]) {
                            bestRemainingMovements[neighbor.Value] = newRemainingMovementPoints;
                            // Enqueue the neighbor with the newRemainingMovementPoints
                            tilesToVisit.Enqueue(new FloodFillQueueEntry {
                                Tile = neighbor.Value,
                                RemainingMovementPoints = newRemainingMovementPoints
                            } );
                        }
                    }
                    
                }

                foreach (var entry in bestRemainingMovements) {
                    if (entry.Key == startTile) continue;
                    result.Add(entry.Key);
                }
                
                return result;
            }
            
            public void SetGridPosition(Vector2Int newPosition) {
                GridPosition = newPosition;
            }

        
    }
    
   
    
}

