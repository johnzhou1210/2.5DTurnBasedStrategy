using System.Collections.Generic;
using StrategyGame.Grid.GridData;
using UnityEngine;
using UnityEngine.Serialization;

namespace StrategyGame.Factions {
    [CreateAssetMenu(menuName = "Strategy Game/Faction")]
    public class FactionData : ScriptableObject {
        public string factionName;
        public Color factionColor;
        public bool isPlayerControlled;
        public List<GridUnitData> allowedUnits;
    }
    
}
