using System.Collections.Generic;
using StrategyGame.Combat.Cinematics;
using StrategyGame.Combat.Weapons;
using UnityEngine;

namespace StrategyGame.Combat {
    [CreateAssetMenu(menuName = "Strategy Game/Consumable Item")]
    public class ConsumableItemData : ScriptableObject {
        public int HealingPower = 0;
        public int ConsumableID = -1;
    }
}
