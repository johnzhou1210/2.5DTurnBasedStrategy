using System;
using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Core.GameState {
    public class GameStateManager : MonoBehaviour
    {
       public enum TurnPhase { Player, Enemy, Event } 
       public TurnPhase CurrentPhase { get; private set; }
       public Dictionary<int, GridEntity> Entities { get; private set; }

       private void OnEnable() {
           EntityDelegates.GetGridEntityByID = GetGridEntityById;
           GameStateDelegates.OnGameStarted += StartGame;
       }

       private void OnDisable() {
           EntityDelegates.GetGridEntityByID = null;
           GameStateDelegates.OnGameStarted -= StartGame;
       }

       private void Awake() {
           Entities = new Dictionary<int, GridEntity>();
       }


       public void AdvancePhase() {
           CurrentPhase = (TurnPhase)(((int)CurrentPhase + 1) % Enum.GetValues(typeof(TurnPhase)).Length);
           GameStateDelegates.InvokeOnPhaseChanged(CurrentPhase);
       }

       private GridEntity GetGridEntityById(int id) {
           return Entities[id];
       }


       private void StartGame() {
           Debug.Log("Starting Game");
           
           GridEntity soldierEntity = SpawnUnit(Resources.Load<GridUnitData>("ScriptableObjects/Units/Soldier"), new(0,0));
           GridEntity orcEntity = SpawnUnit(Resources.Load<GridUnitData>("ScriptableObjects/Units/Orc"), new(1,1));
           GridEntity archerEntity = SpawnUnit(Resources.Load<GridUnitData>("ScriptableObjects/Units/Archer"), new(2,2));

           Debug.Assert(soldierEntity != null, nameof(soldierEntity) + " != null");
           Debug.Assert(orcEntity != null, nameof(orcEntity) + " != null");
           Debug.Assert(archerEntity != null, nameof(archerEntity) + " != null");
           
           Entities[soldierEntity.ID] = soldierEntity;
           Entities[orcEntity.ID] = orcEntity;
           Entities[archerEntity.ID] = archerEntity;
       }
       
       private GridEntity SpawnUnit(GridUnitData unitData, Vector2Int position) {
           GridEntity newEntity = new GridUnit(unitData, unitData);
           GridDelegates.InvokeOnEntitySpawned(newEntity, position);
           return newEntity;
       }
       
    }
}
