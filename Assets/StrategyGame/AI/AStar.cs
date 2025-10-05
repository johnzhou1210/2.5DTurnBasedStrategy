using System;
using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid;
using StrategyGame.Utils;
using UnityEngine;

namespace StrategyGame.AI {
    public class AStar {
        private static int ManhattanDistance(Vector2Int coord1, Vector2Int coord2) {
            return Math.Abs(coord1.x - coord2.x) + Math.Abs(coord1.y - coord2.y);
        }

        public struct PathItem {
            public Tile Tile;
            public bool Reachable;
        }
        
        public static (bool reachable, List<Tile> path) CalculateBestPath(Vector2Int startPosition, Vector2Int targetPosition) {
            // 1. Initialization
            Dictionary<Tile, int> gCosts = new Dictionary<Tile, int>();
            Dictionary<Tile, int> hCosts = new Dictionary<Tile, int>();
            Dictionary<Tile, int> fCosts = new Dictionary<Tile, int>();
            Dictionary<Tile, Tile> parents = new Dictionary<Tile, Tile>();

            Tile startTile = GridDelegates.GetTileFromPosition(startPosition);
            int unitMovementRange = startTile.Occupant.MovementRange;
            PriorityQueue<Tile, int> openSet = new PriorityQueue<Tile, int>();
            HashSet<Tile> tilesInOpenSet = new HashSet<Tile>();

            gCosts[startTile] = 0;
            hCosts[startTile] = ManhattanDistance(startPosition, targetPosition);
            fCosts[startTile] = gCosts[startTile] + hCosts[startTile];
            
            openSet.Enqueue(startTile, fCosts[startTile]);
            tilesInOpenSet.Add(startTile);
            

            while (openSet.Count > 0) {
                Tile dequeuedTile = openSet.Dequeue();
                tilesInOpenSet.Remove(dequeuedTile);
                // Goal check
                if (dequeuedTile.Position == targetPosition) {
                    // Stop and reconstruct path using parents dictionary
                    Stack<Tile> reversedPath = new Stack<Tile>();
                    Tile currentTile = dequeuedTile;

                    while (currentTile != null) {
                        reversedPath.Push(currentTile);
                        parents.TryGetValue(currentTile, out currentTile);
                    }
                    
                    List<Tile> bestPath = new List<Tile>();
                    while (reversedPath.Count > 0) {
                        bestPath.Add(reversedPath.Pop());
                    }
                    return (true, bestPath);
                }
                // Evaluate neighbors
                foreach (var neighbor in dequeuedTile.Neighbors) {
                    if (neighbor.Value == null) continue;
                    int neighborCost = neighbor.Value.MovementCost;
                    int gCostAfterMovement = gCosts[dequeuedTile] + neighborCost;
                    if (!gCosts.ContainsKey(neighbor.Value) || gCostAfterMovement < gCosts[neighbor.Value]) {
                        gCosts[neighbor.Value] = gCostAfterMovement;
                        hCosts[neighbor.Value] = ManhattanDistance(neighbor.Value.Position, targetPosition);
                        fCosts[neighbor.Value] = gCosts[neighbor.Value] + hCosts[neighbor.Value];
                        parents[neighbor.Value] = dequeuedTile;
                        if (!tilesInOpenSet.Contains(neighbor.Value) && unitMovementRange - gCostAfterMovement >= 0) {
                            openSet.Enqueue(neighbor.Value, fCosts[neighbor.Value]);
                            tilesInOpenSet.Add(neighbor.Value);
                        }
                    }
                }
            }

            // If here, then no path found.
            Debug.Log("A*: No path found!");
            return (false, new List<Tile>());
        }


        

    }
}
