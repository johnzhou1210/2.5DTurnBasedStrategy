using System.Collections.Generic;
using StrategyGame.Combat.Cinematics;
using StrategyGame.Combat.Targeting;
using StrategyGame.Combat.Weapons;
using UnityEngine;

namespace StrategyGame.Combat {
    public enum ImpactEffectType {
        None,
        GigaImpactMonochrome,
        GigaImpactFury
    }
    
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
        public bool Heal = false;
        public GameObject AuraPrefab;
        public GameObject CollisionEffect;
        public ImpactEffectType ImpactEffectType;
        public bool CanTargetAllies = false;
        public bool CanTargetEnemies = true;
        public bool CanTargetSelf = false;
        public AttackRange AttackRange;
        public bool IgnoreDefensiveStats = false;
        public bool NeverCrit = false;
        public bool NeverMiss = false;
        public bool AttackCutscene = true;
    }
}
