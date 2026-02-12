using System;
using DG.Tweening;
using StrategyGame.Core.Delegates;
using StrategyGame.Core.Enums;
using StrategyGame.Core.GameState;
using StrategyGame.Factions;
using StrategyGame.Grid;
using UnityEngine;

public class GridCursorRenderer : MonoBehaviour
{
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    [SerializeField] private GameObject gridCursorInnerPointers;
    [SerializeField] private Renderer downwardArrowRenderer;
    [SerializeField] private float moveTweenDuration = .25f;
    [SerializeField] private GameObject attackIcon;

    private Vector2Int _targetPosition;
   

    private void Start() {
        downwardArrowRenderer.material.EnableKeyword("_EMISSION");
    }

    public void SetDownwardArrowColor(Color c) {
        downwardArrowRenderer.material.color = c;
        downwardArrowRenderer.material.SetColor(EmissionColor, c * 5f);
    }

    public void MoveTo(Vector2Int gridCursorPosition) {
        transform.DOMove(new Vector3(gridCursorPosition.x, .05f, gridCursorPosition.y), moveTweenDuration);
        SetGridCursorInnerPointerVisibility(gridCursorPosition);
        SetMiscIcon();
    }
    
    private void SetGridCursorInnerPointerVisibility(Vector2Int gridCursorPosition) {
        _targetPosition = gridCursorPosition;
        Tile targetTile = GridDelegates.GetTileFromPosition(gridCursorPosition);
        gridCursorInnerPointers.SetActive(targetTile.Occupant != null);
        if (targetTile.Occupant == null) {
            SetDownwardArrowColor(Color.white);
            return;
        }
        SetDownwardArrowColor(targetTile.Occupant.Faction == Faction.Player ? Color.blue : targetTile.Occupant.Faction == Faction.Enemy ? Color.red : Color.yellow);
    }

    private void SetMiscIcon() {
        GameStateData currState = GameStateDelegates.GetCurrentGameState();
        switch (currState.Combat.PlayerPhase) {
            case GameStateEnums.PlayerPhaseState.UnitSelectTarget:
                GridEntity occupant = GridDelegates.GetTileFromPosition(_targetPosition).Occupant;
                GridEntity attacker = EntityDelegates.GetGridEntityByID(currState.Combat.SelectedEntityID);
                attackIcon.SetActive(occupant != null && !attacker.IsFriendlyWith(occupant));
                break;
            default:
                attackIcon.SetActive(false);
                break;
        }
    }
}
