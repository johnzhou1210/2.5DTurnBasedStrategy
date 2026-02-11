using System;
using DG.Tweening;
using StrategyGame.Core.Delegates;
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
    }
    
    private void SetGridCursorInnerPointerVisibility(Vector2Int gridCursorPosition) {
        GameStateData currState = GameStateDelegates.GetCurrentGameState();
        Tile targetTile = GridDelegates.GetTileFromPosition(gridCursorPosition);
        gridCursorInnerPointers.SetActive(targetTile.Occupant != null);
        if (targetTile.Occupant == null) {
            SetDownwardArrowColor(Color.white);
            return;
        }
        SetDownwardArrowColor(targetTile.Occupant.Faction == Faction.Player ? Color.blue : targetTile.Occupant.Faction == Faction.Enemy ? Color.red : Color.yellow);
    }
}
