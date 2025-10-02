using System;
using StrategyGame.Core.Delegates;
using UnityEngine;

namespace StrategyGame.Core.GameState {
    public class GameStateManager : MonoBehaviour
    {
       public enum TurnPhase { Player, Enemy, Event } 
       public TurnPhase CurrentPhase { get; private set; }

       public void AdvancePhase() {
           CurrentPhase = (TurnPhase)(((int)CurrentPhase + 1) % Enum.GetValues(typeof(TurnPhase)).Length);
           GameStateDelegates.InvokeOnPhaseChanged(CurrentPhase);
       }
    }
}
