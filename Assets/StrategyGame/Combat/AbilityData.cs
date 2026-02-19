using System.Collections.Generic;
using StrategyGame.Combat.Cinematics;
using StrategyGame.Combat.Weapons;
using UnityEngine;

namespace StrategyGame.Combat {
    [CreateAssetMenu(menuName = "Strategy Game/Ability")]
    public class AbilityData : ScriptableObject {
        public List<StatModifier> StatModifiers;
        public bool OverrideDamageType = false;
        public DamageType DamageTypeOverride;
        public ProjectileVisualData ProjectileVisualData;
        public int SkillID = -1;
        public string Description;
        public bool CooldownAtStart = false;
        public int MaxCooldown = 0;
    }
}
