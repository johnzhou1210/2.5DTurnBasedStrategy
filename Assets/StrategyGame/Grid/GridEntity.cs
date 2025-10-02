using System;
using StrategyGame.Factions;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Grid {
    public abstract class GridEntity {
            // Core
            public Vector2Int GridPosition { get; set; }
            public Vector3 WorldPosition => new Vector3(GridPosition.x, 0, GridPosition.y);
            public bool IsPassable { get; set; } = true;
            public GridEntityData GridEntityData { get; set; }

            // Identity
            private static int _nextID = 0;
            public string Name { get; set; }
            public int ID { get; set; }
            public Faction Faction { get; set; }

            // Gameplay
            public bool CanMove { get; set; } = false;
            public bool CanAct { get; set; } = false;
            public bool Selectable { get; set; } = true;
            
            // Visual
            public bool IsSelected { get; set; } = false;
            public bool IsVisible { get; set; } = true;

            // Optional
            public int Health { get; set; }
            public int MaxHealth { get; set; }
            public int MovementRange { get; set; }
            public int VisionRange { get; set; }

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
                
            }
            
            public virtual void Select() => OnSelected?.Invoke(this);

            public virtual void MoveTo(Vector2Int newPos) {
                GridPosition = newPos;
                OnMoved?.Invoke(this, newPos);
            }

            public virtual GameObject GetSpritePrefab() {
                return GridEntityData.VisualPrefab;
            }

        
    }
}

