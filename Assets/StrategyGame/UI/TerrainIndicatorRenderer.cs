using System;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid;
using TMPro;
using UnityEngine;

public class TerrainIndicatorRenderer : MonoBehaviour
{
    // ==============================
    // FIELDS & PROPERTIES
    // ==============================
    [SerializeField] private TextMeshProUGUI terrainText;
    
    // ==============================
    // MONOBEHAVIOUR LIFECYCLE
    // ==============================
    private void OnEnable() {
        UIDelegates.OnTerrainUIUpdate += UpdateTerrainIndicatorUI;
    }
    private void OnDisable() {
        UIDelegates.OnTerrainUIUpdate -= UpdateTerrainIndicatorUI;
    }
    
    // ==============================
    // CORE METHODS
    // ==============================
    private void UpdateTerrainIndicatorUI(Tile tileObj) {
        terrainText.text = tileObj.InitData.name;
    }
}
