using System;
using StrategyGame.Core.Delegates;
using StrategyGame.Factions;
using StrategyGame.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StrategyGame.UI.HUD {
    public class EntityHUDRenderer : MonoBehaviour
    {
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        [SerializeField] private TextMeshProUGUI entityNameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI moveText;
        [SerializeField] private TextMeshProUGUI attackText;
        [SerializeField] private TextMeshProUGUI accuracyText;
        [SerializeField] private TextMeshProUGUI defenseText;
        [SerializeField] private TextMeshProUGUI resistanceText;
        [SerializeField] private TextMeshProUGUI agilityText;
        [SerializeField] private TextMeshProUGUI evasionText;
        [SerializeField] private Image entityTitleBackground;
        [SerializeField] private Image moveSpeedBackground;
        
        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        private void OnEnable() {
            UIDelegates.OnEntityHUDUpdate += UpdateHUD;
        }

        private void OnDisable() {
            UIDelegates.OnEntityHUDUpdate -= UpdateHUD;
        }

        // ==============================
        // CORE METHODS
        // ==============================
        private void UpdateHUD(GridEntity entity) {
            entityTitleBackground.color = entity.Faction == Faction.PlayerFaction ? Color.blue :   Color.red;
            moveSpeedBackground.color = entity.Faction == Faction.PlayerFaction ? new(0,0,140/255f): new Color(150/255f,0,0);
            entityNameText.SetText(entity.DisplayName);
            hpText.SetText($"<size=80>{entity.Health}</size>/{entity.MaxHealth}");
            moveText.SetText(entity.MovementRange.ToString());
            attackText.SetText(entity.Attack.ToString());
            accuracyText.SetText(entity.Accuracy.ToString());
            defenseText.SetText(entity.Defense.ToString());
            resistanceText.SetText(entity.Resistance.ToString());
            agilityText.SetText(entity.Agility.ToString());
            evasionText.SetText(entity.Evasion.ToString());
        }
    }
}
