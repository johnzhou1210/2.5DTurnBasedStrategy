using System;
using StrategyGame.Factions;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Grid {
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

        
    }
}

