using System.Collections.Generic;
using StrategyGame.Grid.GridData;
using UnityEngine;
using UnityEngine.Serialization;

namespace StrategyGame.Factions {
    [CreateAssetMenu(menuName = "Strategy Game/Faction")]
    public class FactionData : ScriptableObject {
        [SerializeField] private Faction factionEnum;
        [SerializeField] private string factionName;
        [SerializeField] private Color factionColor;
        [SerializeField] private bool isPlayerControlled;
       
        
        public Faction FactionEnum {get => factionEnum; }
        public string FactionName { get => factionName; }
        public Color FactionColor {get => factionColor; }
        public bool IsPlayerControlled {get => isPlayerControlled; }
    
    }
    
}
