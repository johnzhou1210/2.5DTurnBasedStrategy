using System;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Core.GameState;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StrategyGame.UI.HUD {
    public class TurnIndicatorRenderer : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI turnText;
        [SerializeField] private Image indicatorBackground;

        private void OnEnable() {
            UIDelegates.OnUpdateTurnIndicatorRenderer += UpdateIndicator;
        }

        private void OnDisable() {
            UIDelegates.OnUpdateTurnIndicatorRenderer -= UpdateIndicator;
        }

        private void UpdateIndicator() {
            GameStateData currentState = GameStateDelegates.GetCurrentGameState();
            Debug.Log(currentState.Combat);
            Debug.Log(currentState.Combat.TurnPhaseCycle);
            Debug.Log(currentState.Combat.TurnPhase);
            
            turnText.SetText($"Turn {currentState.Combat.TurnPhaseCycle.ToString()}");
            indicatorBackground.color = currentState.Combat.TurnPhase == GameStateEnums.TurnPhase.Player
                ? new Color(0, 0, 140 / 255f)
                : currentState.Combat.TurnPhase == GameStateEnums.TurnPhase.Enemy
                    ? new Color(150 / 255f, 0, 0)
                    : new Color(50 / 255f, 50 / 255f, 50 / 255f);
        }
    }
}