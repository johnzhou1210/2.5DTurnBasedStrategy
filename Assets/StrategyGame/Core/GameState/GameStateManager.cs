using System;
using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid;
using StrategyGame.Grid.GridData;
using UnityEngine;
using Random = UnityEngine.Random;

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
            GameStateDelegates.GetCurrentSelectedEntity  = () => CurrentSelectedEntity;
        }
        private void OnDisable() {
            GameStateDelegates.OnGameStarted -= StartGame;
            GridDelegates.OnSelectTile -= SetSelectedTile;
            GridDelegates.GetSelectedTile = null;
            GameStateDelegates.GetCurrentSelectedEntity = null;
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

            int placedMontains = 0;
            Vector2Int gridDimensions = GridDelegates.GetGridDimensions();
            while (placedMontains < 32) {
                Vector2Int randomPosition = new Vector2Int(Random.Range(0, gridDimensions.x), Random.Range(0, gridDimensions.y));
                while (GridDelegates.GetTileFromPosition(randomPosition).IsOccupied) {
                    randomPosition = new Vector2Int(Random.Range(0, gridDimensions.x), Random.Range(0, gridDimensions.y));
                }
                GridDelegates.InvokeOnMountainifyTile(randomPosition);
                placedMontains++;
            }


        }

        private void SetSelectedTile(Vector2Int coordinates) {
            Tile newTile = GridDelegates.GetTileFromPosition(coordinates);
            Tile oldTile = CurrentSelectedTile;
            CurrentSelectedTile = newTile ?? throw new ArgumentException("Tile does not exist at position {coordinates}!");
            GridDelegates.InvokeOnSetSelectedTile(oldTile, newTile);
            CurrentSelectedEntity = newTile.IsOccupied ? newTile.Occupant : null;
            Vector2Int startPosition = CurrentSelectedEntity?.GridPosition ?? newTile.Position;
            GridDelegates.InvokeOnUpdatePathPreview(startPosition, startPosition);
        }
    }
}
