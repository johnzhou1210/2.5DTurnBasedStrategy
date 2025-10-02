using System.Collections.Generic;
using StrategyGame.Grid.GridData;
using UnityEngine;
using UnityEngine.Serialization;

namespace StrategyGame.Factions {
    public class Faction : ScriptableObject {
        [FormerlySerializedAs("FactionName")] public string factionName;
        [FormerlySerializedAs("FactionColor")] public Color factionColor;
        public bool isPlayerControlled;
        public List<GridUnitData> allowedUnits;
    }
    
}
