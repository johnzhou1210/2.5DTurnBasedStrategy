using System;
using System.Linq;
using StrategyGame.Combat;
using StrategyGame.Combat.Cinematics;
using StrategyGame.Core.Data.Databases;
using StrategyGame.Core.Delegates;
using UnityEngine;

namespace StrategyGame.Core.Data {
    public class DataProvider : MonoBehaviour {
        [SerializeField] private AbilityDataDatabase abilityDataDatabase;
        [SerializeField] private ProjectileVisualDatabase projectileVisualDatabase;

        private void OnEnable() {
            DataDelegates.GetAbilityDataByID = (id) => abilityDataDatabase.Abilities.FirstOrDefault(a => a.SkillID == id);
            DataDelegates.GetProjectileVisualDataByID = id => projectileVisualDatabase.ProjectileVisuals.FirstOrDefault(v => v.ProjectileID == id);
        }
        private void OnDisable() {
            DataDelegates.GetAbilityDataByID = null;
            DataDelegates.GetProjectileVisualDataByID = null;
        }
    }
}
