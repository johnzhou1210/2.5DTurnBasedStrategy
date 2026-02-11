using System;
using System.Collections.Generic;
using StrategyGame.Combat;
using StrategyGame.Combat.Cinematics;
using StrategyGame.Combat.Weapons;
using StrategyGame.Core.Delegates;
using StrategyGame.Factions;
using StrategyGame.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StrategyGame.UI.HUD {
    public class BattleOutcomePreviewRenderer : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI attackerName;
        [SerializeField] private Image attackerWeaponIcon;
        [SerializeField] private TextMeshProUGUI attackerWeaponName;
        [SerializeField] private TextMeshProUGUI attackerCurrentHPNumber;
        [SerializeField] private TextMeshProUGUI attackerHitChance;
        [SerializeField] private TextMeshProUGUI attackerCritChance;
        [SerializeField] private GameObject attackerDeathRisksContainer;
        [SerializeField] private Slider attackerNewHPSlider;
        [SerializeField] private Slider attackerHPLossSlider;
        [SerializeField] private TextMeshProUGUI attackerHPLossNumber;
        [SerializeField] private Image attackerNonBlurColored;
        [SerializeField] private Image attackerGlassColored;
        [SerializeField] private Image attackerDarkGlassColored;
        [SerializeField] private GameObject attackerAdvantage;
        [SerializeField] private GameObject attackerDisadvantage;
        [SerializeField] private Image attackerHPBarFrontFill;
        [SerializeField] private Image attackerHPBarLossFill;

        [SerializeField] private GameObject blowExchangeContainer;
        [SerializeField] private GameObject blowExchangeArrowPrefab;
        
        [SerializeField] private TextMeshProUGUI defenderName;
        [SerializeField] private Image defenderWeaponIcon;
        [SerializeField] private TextMeshProUGUI defenderWeaponName;
        [SerializeField] private TextMeshProUGUI defenderCurrentHPNumber;
        [SerializeField] private TextMeshProUGUI defenderHitChance;
        [SerializeField] private TextMeshProUGUI defenderCritChance;
        [SerializeField] private GameObject defenderDeathRisksContainer;
        [SerializeField] private Slider defenderNewHPSlider;
        [SerializeField] private Slider defenderHPLossSlider;
        [SerializeField] private TextMeshProUGUI defenderHPLossNumber;
        [SerializeField] private Image defenderNonBlurColored;
        [SerializeField] private Image defenderGlassColored;
        [SerializeField] private Image defenderDarkGlassColored;
        [SerializeField] private GameObject defenderAdvantage;
        [SerializeField] private GameObject defenderDisadvantage;
        [SerializeField] private Image defenderHPBarFrontFill;
        [SerializeField] private Image defenderHPBarLossFill;

        private void OnEnable() {
            UIDelegates.OnBattleOutcomePreviewUpdate += UpdateBattleOutcomePreview;
        }

        private void OnDisable() {
            UIDelegates.OnBattleOutcomePreviewUpdate -= UpdateBattleOutcomePreview;
        }

        private void UpdateBattleOutcomePreview(CombatPreview preview) {
            GridEntity attackerEntity = EntityDelegates.GetGridEntityByID(preview.AttackerID);
            GridEntity defenderEntity = EntityDelegates.GetGridEntityByID(preview.DefenderID);
            WeaponMatchupResult attackerWeaponMatchupStatus = WeaponData.EvaluateWeaponMatchup(preview.AttackerWeapon.WeaponType, preview.DefenderWeapon.WeaponType);
            WeaponMatchupResult defenderWeaponMatchupStatus = WeaponData.EvaluateWeaponMatchup(preview.DefenderWeapon.WeaponType, preview.AttackerWeapon.WeaponType);
            
            // Attacker
            attackerName.SetText(preview.AttackerDisplayName);
            attackerWeaponIcon.sprite = WeaponData.GetSpriteFromWeaponType(preview.AttackerWeapon.WeaponType);
            attackerWeaponName.SetText(preview.AttackerWeapon.name);
            attackerCurrentHPNumber.SetText(preview.AttackerCurrentHP.ToString());
            attackerHitChance.SetText($"{Mathf.CeilToInt(preview.AttackerHitChance*100f).ToString()}%");
            attackerCritChance.SetText($"{Mathf.CeilToInt(preview.AttackerCritChance*100f).ToString()}%");
            CanvasGroup attackerVeryLowRisk = attackerDeathRisksContainer.transform.Find("VeryLowRisk").GetComponent<CanvasGroup>();
            CanvasGroup attackerLowRisk = attackerDeathRisksContainer.transform.Find("LowRisk").GetComponent<CanvasGroup>();
            CanvasGroup attackerMediumRisk = attackerDeathRisksContainer.transform.Find("MediumRisk").GetComponent<CanvasGroup>();
            CanvasGroup attackerHighRisk = attackerDeathRisksContainer.transform.Find("HighRisk").GetComponent<CanvasGroup>();
            CanvasGroup attackerGuaranteedDeath = attackerDeathRisksContainer.transform.Find("GuaranteedDeath").GetComponent<CanvasGroup>();
            attackerVeryLowRisk.alpha = 0;
            attackerLowRisk.alpha = 0;
            attackerMediumRisk.alpha = 0;
            attackerHighRisk.alpha = 0;
            attackerGuaranteedDeath.alpha = 0;
            Debug.Log($"BattleOutcomePreviewRenderer.UpdateBattleOutcomePreview: Chance to kill attacker is {preview.DefenderChanceToKillAttacker}");
            switch (preview.DefenderChanceToKillAttacker) {
                case <= 0f:
                    // Don't show risk
                    break;
                case < .1f:
                    attackerVeryLowRisk.alpha = 1f;
                    attackerVeryLowRisk.transform.GetComponentInChildren<TextMeshProUGUI>().SetText($"{Mathf.CeilToInt(preview.DefenderChanceToKillAttacker*100f)}%");
                    break;
                case < .33f:
                    attackerLowRisk.alpha = 1f;
                    attackerLowRisk.transform.GetComponentInChildren<TextMeshProUGUI>().SetText($"{Mathf.CeilToInt(preview.DefenderChanceToKillAttacker*100f)}%");
                    break;
                case < .67f:
                    attackerMediumRisk.alpha = 1f;
                    attackerMediumRisk.transform.GetComponentInChildren<TextMeshProUGUI>().SetText($"{Math.Clamp(Mathf.CeilToInt(preview.DefenderChanceToKillAttacker*100f), 0, 99)}%");
                    break;
                case < .9995f:
                    attackerHighRisk.alpha = 1f;
                    attackerHighRisk.transform.GetComponentInChildren<TextMeshProUGUI>().SetText($"{Mathf.CeilToInt(preview.DefenderChanceToKillAttacker*100f)}%");
                    break;
                default:
                    attackerGuaranteedDeath.alpha = 1f;
                    break;
            }
            int attackerForecastedHP = preview.AttackerCurrentHP - (preview.DefenderNumCounters * preview.DefenderNonCritDamagePerHit);
            attackerForecastedHP = Math.Clamp(attackerForecastedHP, 0, preview.AttackerMaxHP);
            attackerNewHPSlider.value = attackerForecastedHP * 1f / preview.AttackerMaxHP;
            attackerHPLossSlider.value =  preview.AttackerCurrentHP * 1f / preview.AttackerMaxHP;
            attackerHPLossNumber.text = attackerForecastedHP.ToString();
            attackerHPLossNumber.color = attackerForecastedHP == 0 ? Color.red : Color.white;
            attackerNonBlurColored.color = attackerEntity.Faction == Faction.Player ? new Color(0, 0, 1, 240 / 255f) : new Color(1, 0, 0, 240 / 255f);
            attackerGlassColored.color = attackerEntity.Faction == Faction.Player ? new Color(0, 0, 1, 240 / 255f) : new Color(1, 0, 0, 240 / 255f);
            attackerDarkGlassColored.color = attackerEntity.Faction == Faction.Player ? new Color(0, 0, .5f, 240 / 255f) : new Color(.5f, 0, 0, 240 / 255f);

            attackerHPBarFrontFill.color = attackerEntity.Faction == Faction.Player ? new Color(32 / 255f, 32 / 255f, 159 / 255f, 1f) :  new Color(159 / 255f, 32 / 255f, 32 / 255f, 1);
            attackerHPBarLossFill.color = attackerEntity.Faction == Faction.Player ? new Color(80 / 255f, 44 / 255f, 44 / 255f, 1f) : new Color(70 / 255f, 30 / 255f, 30 / 255f, 1);
            
            attackerAdvantage.SetActive(false); attackerDisadvantage.SetActive(false);
            switch (attackerWeaponMatchupStatus) {
                case WeaponMatchupResult.Advantage:
                    attackerAdvantage.SetActive(true);
                    break;
                case WeaponMatchupResult.Disadvantage:
                    attackerDisadvantage.SetActive(true);
                    break;
            }
            
            
            // Defender
            defenderName.SetText(preview.DefenderDisplayName);
            defenderWeaponIcon.sprite = WeaponData.GetSpriteFromWeaponType(preview.DefenderWeapon.WeaponType);
            defenderWeaponName.SetText(preview.DefenderWeapon.name);
            defenderCurrentHPNumber.SetText(preview.DefenderCurrentHP.ToString());
            defenderHitChance.SetText($"{Mathf.CeilToInt(preview.DefenderHitChance*100f).ToString()}%");
            defenderCritChance.SetText($"{Mathf.CeilToInt(preview.DefenderCritChance*100f).ToString()}%");
            CanvasGroup defenderVeryLowRisk = defenderDeathRisksContainer.transform.Find("VeryLowRisk").GetComponent<CanvasGroup>();
            CanvasGroup defenderLowRisk = defenderDeathRisksContainer.transform.Find("LowRisk").GetComponent<CanvasGroup>();
            CanvasGroup defenderMediumRisk = defenderDeathRisksContainer.transform.Find("MediumRisk").GetComponent<CanvasGroup>();
            CanvasGroup defenderHighRisk = defenderDeathRisksContainer.transform.Find("HighRisk").GetComponent<CanvasGroup>();
            CanvasGroup defenderGuaranteedDeath = defenderDeathRisksContainer.transform.Find("GuaranteedDeath").GetComponent<CanvasGroup>();
            defenderVeryLowRisk.alpha = 0f;
            defenderLowRisk.alpha = 0;
           defenderMediumRisk.alpha = 0;
           defenderHighRisk.alpha = 0;
           defenderGuaranteedDeath.alpha = 0;
           Debug.Log($"BattleOutcomePreviewRenderer.UpdateBattleOutcomePreview: Chance to kill defender is {preview.AttackerChanceToKillDefender}");
            switch (preview.AttackerChanceToKillDefender) {
                case <= 0f:
                    // Don't show risk
                    break;
                case < .1f:
                    defenderVeryLowRisk.alpha = 1f;
                    defenderVeryLowRisk.transform.GetComponentInChildren<TextMeshProUGUI>().SetText($"{Mathf.CeilToInt(preview.AttackerChanceToKillDefender*100f)}%");
                    break;
                case < .33f:
                    defenderLowRisk.alpha = 1f;
                    defenderLowRisk.transform.GetComponentInChildren<TextMeshProUGUI>().SetText($"{Mathf.CeilToInt(preview.AttackerChanceToKillDefender*100f)}%");
                    break;
                case < .67f:
                    defenderMediumRisk.alpha = 1f;
                    defenderMediumRisk.transform.GetComponentInChildren<TextMeshProUGUI>().SetText($"{Math.Clamp(Mathf.CeilToInt(preview.AttackerChanceToKillDefender*100f), 0, 99)}%");
                    break;
                case < .9995f:
                    defenderHighRisk.alpha = 1f;
                    defenderHighRisk.transform.GetComponentInChildren<TextMeshProUGUI>().SetText($"{Mathf.CeilToInt(preview.AttackerChanceToKillDefender*100f)}%");
                    break;
                default:
                    defenderGuaranteedDeath.alpha = 1f;
                    break;
            }
            int defenderForecastedHP = preview.DefenderCurrentHP - (preview.AttackerNumAttacks * preview.AttackerNonCritDamagePerHit);
            defenderForecastedHP = Math.Clamp(defenderForecastedHP, 0, preview.DefenderMaxHP);
            defenderNewHPSlider.value = defenderForecastedHP * 1f / preview.DefenderMaxHP;
            defenderHPLossSlider.value =  preview.DefenderCurrentHP * 1f / preview.DefenderMaxHP;
            defenderHPLossNumber.text = defenderForecastedHP.ToString();
            defenderHPLossNumber.color = defenderForecastedHP == 0 ? Color.red : Color.white;
            defenderNonBlurColored.color = defenderEntity.Faction == Faction.Player ? new Color(0, 0, 1, 240 / 255f) : new Color(1, 0, 0, 240 / 255f);
            defenderGlassColored.color = defenderEntity.Faction == Faction.Player ? new Color(0, 0, 1, 240 / 255f) : new Color(1, 0, 0, 240 / 255f);
            defenderDarkGlassColored.color = defenderEntity.Faction == Faction.Player ? new Color(0, 0, .5f, 240 / 255f) : new Color(.5f, 0, 0, 240 / 255f);
            
            defenderHPBarFrontFill.color = defenderEntity.Faction == Faction.Player ? new Color(32 / 255f, 32 / 255f, 159 / 255f, 1f) :  new Color(159 / 255f, 32 / 255f, 32 / 255f, 1);
            defenderHPBarLossFill.color = defenderEntity.Faction == Faction.Player ? new Color(80 / 255f, 44 / 255f, 44 / 255f, 1f) : new Color(70 / 255f, 30 / 255f, 30 / 255f, 1);
            
            defenderAdvantage.SetActive(false); defenderDisadvantage.SetActive(false);
            switch (defenderWeaponMatchupStatus) {
                case WeaponMatchupResult.Advantage:
                    defenderAdvantage.SetActive(true);
                    break;
                case WeaponMatchupResult.Disadvantage:
                    defenderDisadvantage.SetActive(true);
                    break;
            }
            
            // Render exchanges
            foreach (Transform child in blowExchangeContainer.transform) {
                Destroy(child.gameObject);
            }
            
            int attackerHitsLeft = preview.AttackerNumAttacks;
            int defenderHitsLeft = preview.DefenderNumCounters;
            while (attackerHitsLeft > 0 && defenderHitsLeft > 0) {
                GameObject attackerArrow = Instantiate(blowExchangeArrowPrefab, blowExchangeContainer.transform);
                GameObject attackerCorrectArrow = attackerArrow.transform.Find(attackerEntity.Faction == Faction.Player ? "Blue" : "Red").gameObject;
                attackerCorrectArrow.GetComponentInChildren<TextMeshProUGUI>().SetText(preview.AttackerNonCritDamagePerHit.ToString());
                attackerCorrectArrow.GetComponent<CanvasGroup>().alpha = 1f;
                attackerHitsLeft -= 1;
                GameObject defenderArrow = Instantiate(blowExchangeArrowPrefab, blowExchangeContainer.transform);
                GameObject defenderCorrectArrow = defenderArrow.transform.Find(defenderEntity.Faction == Faction.Player ? "Blue" : "Red").gameObject;
                defenderCorrectArrow.GetComponentInChildren<TextMeshProUGUI>().SetText(preview.DefenderNonCritDamagePerHit.ToString());
                defenderCorrectArrow.GetComponent<CanvasGroup>().alpha = 1f;
                defenderHitsLeft -= 1;
            }
            while (attackerHitsLeft > 0) {
                GameObject attackerArrow = Instantiate(blowExchangeArrowPrefab, blowExchangeContainer.transform);
                GameObject attackerCorrectArrow = attackerArrow.transform.Find(attackerEntity.Faction == Faction.Player ? "Blue" : "Red").gameObject;
                attackerCorrectArrow.GetComponentInChildren<TextMeshProUGUI>().SetText(preview.AttackerNonCritDamagePerHit.ToString());
                attackerCorrectArrow.GetComponent<CanvasGroup>().alpha = 1f;
                attackerHitsLeft -= 1;
            }
            while (defenderHitsLeft > 0) {
                GameObject defenderArrow = Instantiate(blowExchangeArrowPrefab, blowExchangeContainer.transform);
                GameObject defenderCorrectArrow = defenderArrow.transform.Find(defenderEntity.Faction == Faction.Player ? "Blue" : "Red").gameObject;
                defenderCorrectArrow.GetComponentInChildren<TextMeshProUGUI>().SetText(preview.DefenderNonCritDamagePerHit.ToString());
                defenderCorrectArrow.GetComponent<CanvasGroup>().alpha = 1f;
                defenderHitsLeft -= 1;
            }


        }

    }
}
