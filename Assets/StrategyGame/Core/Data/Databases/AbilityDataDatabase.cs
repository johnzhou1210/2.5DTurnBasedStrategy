using System.Collections.Generic;
using StrategyGame.Combat;
using UnityEngine;

namespace StrategyGame.Core.Data.Databases {
    [CreateAssetMenu(menuName = "Strategy Game/Databases/Ability Data Database")]
    public class AbilityDataDatabase : ScriptableObject {
        [SerializeField] private List<AbilityData> abilities;
        public List<AbilityData> Abilities { get => abilities; }
    }
}
