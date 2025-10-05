using System;
using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Core.GameState {
    public class GameStateManager : MonoBehaviour {
        public enum TurnPhase {
            Player,
            Enemy,
            Event
        }

        public TurnPhase CurrentPhase { get; private set; }
        public GridEntity CurrentSelectedEntity { get; private set; }
        public Tile CurrentSelectedTile {get; private set;}
        
        private void OnEnable() {
            GameStateDelegates.OnGameStarted += StartGame;
            GridDelegates.OnSelectTile += SetSelectedTile;
            GridDelegates.GetSelectedTile = () => CurrentSelectedTile;
        }
        private void OnDisable() {
            GameStateDelegates.OnGameStarted -= StartGame;
            GridDelegates.OnSelectTile -= SetSelectedTile;
            GridDelegates.GetSelectedTile = null;
        }
        public void AdvancePhase() {
            CurrentPhase = (TurnPhase)(((int)CurrentPhase + 1) % Enum.GetValues(typeof(TurnPhase)).Length);
            GameStateDelegates.InvokeOnPhaseChanged(CurrentPhase);
        }
        private void StartGame() {
            Debug.Log("Starting Game");
            List<UnitSpawnQuery> entities = new List<UnitSpawnQuery>();
            entities.Add(new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Soldier"), SpawnPosition = new Vector2Int(0, 0) });
            entities.Add(new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Orc"), SpawnPosition = new Vector2Int(1, 1) });
            entities.Add(new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Archer"), SpawnPosition = new Vector2Int(2, 2) });
            entities.Add(new UnitSpawnQuery { UnitData = Resources.Load<GridUnitData>("ScriptableObjects/Units/Archer"), SpawnPosition = new Vector2Int(2, 2) });
            EntityDelegates.SpawnUnits(entities);
        }

        private void SetSelectedTile(Vector2Int coordinates) {
            Tile newTile = GridDelegates.GetTileFromPosition(coordinates);
            Tile oldTile = CurrentSelectedTile;
            CurrentSelectedTile = newTile ?? throw new ArgumentException("Tile does not exist at position {coordinates}!");
            GridDelegates.InvokeOnSetSelectedTile(oldTile, newTile);

            if (newTile.Occupant != null) {
                Debug.Log("Tile has entity!");
            }
        }
    }
}
