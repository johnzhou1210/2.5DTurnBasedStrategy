using System.Collections.Generic;
using StrategyGame.Combat.Cinematics;
using UnityEngine;

namespace StrategyGame.Core.Data.Databases {
    [CreateAssetMenu(menuName = "Strategy Game/Databases/Projectile Visual Database")]
    public class ProjectileVisualDatabase : ScriptableObject {
        [SerializeField] private List<ProjectileVisualData> projectileVisuals;
        public List<ProjectileVisualData> ProjectileVisuals { get => projectileVisuals; }
    }
}
