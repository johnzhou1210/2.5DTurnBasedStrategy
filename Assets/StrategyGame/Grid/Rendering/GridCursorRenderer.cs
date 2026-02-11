using System;
using DG.Tweening;
using StrategyGame.Core.Delegates;
using StrategyGame.Factions;
using StrategyGame.Grid;
using UnityEngine;

public class GridCursorRenderer : MonoBehaviour
{
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    [SerializeField] private GameObject gridCursorInnerPointers;
    [SerializeField] private Renderer downwardArrowRenderer;
    [SerializeField] private float moveTweenDuration = .25f;

    private void OnEnable() {
        InputDelegates.OnSetGridCursorInnerPointerVisibility += SetGridCursorInnerPointerVisibility;
    }

    private void OnDisable() {
        InputDelegates.OnSetGridCursorInnerPointerVisibility -= SetGridCursorInnerPointerVisibility;
    }
    
    private void Start() {
        downwardArrowRenderer.material.EnableKeyword("_EMISSION");
    }

    public void SetDownwardArrowColor(Color c) {
        downwardArrowRenderer.material.color = c;
        downwardArrowRenderer.material.SetColor(EmissionColor, c * 5f);
    }

    public void MoveTo(Vector2Int gridCursorPosition) {
        transform.DOMove(new Vector3(gridCursorPosition.x, .05f, gridCursorPosition.y), moveTweenDuration);
    }
    
    private void SetGridCursorInnerPointerVisibility(int entityID) {
        gridCursorInnerPointers.SetActive(entityID != -1);
        GridEntity inspectedEntity = EntityDelegates.GetGridEntityByID(entityID);
        if (inspectedEntity == null) {
            SetDownwardArrowColor(Color.white);
            return;
        }
        SetDownwardArrowColor(inspectedEntity.Faction == Faction.Player ? Color.blue : inspectedEntity.Faction == Faction.Enemy ? Color.red : Color.yellow);
    }
}
