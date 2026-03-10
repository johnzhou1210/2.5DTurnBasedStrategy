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
            DataDelegates.GetAbilityDataByID = (id) => {
                AbilityData result = abilityDataDatabase.Abilities.FirstOrDefault(a => a.SkillID == id);
                if (result == null) return result;
                if (result.AttackRange == null) throw new Exception($"DataProvider.OnEnable: The ability of id={id} is missing an AttackRange SO!");
                return result;
            };
            DataDelegates.GetProjectileVisualDataByID = (id) => {
                return projectileVisualDatabase.ProjectileVisuals.FirstOrDefault(v => v.ProjectileID == id);
            };
        }
        private void OnDisable() {
            DataDelegates.GetAbilityDataByID = null;
            DataDelegates.GetProjectileVisualDataByID = null;
        }
    }
}
