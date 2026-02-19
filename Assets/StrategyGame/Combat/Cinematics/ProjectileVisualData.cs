using System.Collections.Generic;
using UnityEngine;

namespace StrategyGame.Combat.Cinematics {
    public enum ProjectileTrajectoryType {
        Straight,
        Parabola
    }
    
    [CreateAssetMenu(menuName = "Strategy Game/Projectile Visual")]
    public class ProjectileVisualData : ScriptableObject {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject impactVFXPrefab;
        [SerializeField] private GameObject missVFXPrefab;
        [SerializeField] private GameObject missBillboardVFXPrefab;
        [SerializeField] private GameObject impactBillboardVFXPrefab;
        [SerializeField] private AnimationCurve heightCurve;
        [SerializeField] private float maxArchHeight;
        [SerializeField] private float cleanupTimeout;
        [SerializeField] private float travelTime;
        [SerializeField] private bool disintegrateOnMiss;

        [field: SerializeField] public int ProjectileID { get; private set; }
        
        public GameObject ProjectilePrefab { get => projectilePrefab; }
        public GameObject ImpactVFXPrefab { get => impactVFXPrefab; }
        public GameObject ImpactBillboardVFXPrefab { get =>  impactBillboardVFXPrefab; }
        public GameObject MissVFXPrefab { get => missVFXPrefab; }
        public GameObject MissBillboardVFXPrefab { get => missBillboardVFXPrefab; }

        public AnimationCurve HeightCurve { get => heightCurve; }
        public float MaxArchHeight { get => maxArchHeight; }

        public float CleanupTimeout { get => cleanupTimeout; }
        public float  TravelTime { get => travelTime; }
        public bool DisintegrateOnMiss { get => disintegrateOnMiss; }
    }

}
