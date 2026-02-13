using DG.Tweening;
using TMPro;
using UnityEngine;

namespace StrategyGame.UI.World {
    public class DamageIndicatorBillboard : MonoBehaviour {
        [SerializeField] private GameObject normalAttackNumber;
        [SerializeField] private GameObject critAttackNumber;
        [SerializeField] private TextMeshProUGUI normalAttackNumberText;
        [SerializeField] private TextMeshProUGUI critAttackNumberText;
        [SerializeField] private TextMeshProUGUI normalAttackHeaderText;
        [SerializeField] private TextMeshProUGUI critAttackHeaderText;
        [SerializeField] private CanvasGroup canvasGroup;
        public void Setup(int damage, bool isCrit, bool isBreak, bool isHeal = false) {
            (isCrit ? critAttackNumber : normalAttackNumber).SetActive(true);
            TextMeshProUGUI targetText = (isCrit ? critAttackNumberText : normalAttackNumberText);
            TextMeshProUGUI targetHeaderText = (isCrit ? critAttackHeaderText : normalAttackHeaderText);
            targetText.SetText(damage.ToString());
            targetHeaderText.SetText(isCrit && isBreak ? "CRIT BREAK!" : isCrit ? "CRIT!" : isBreak ? "BREAK!" : string.Empty);
            transform.localScale = Vector3.zero;
            
            Vector3 directionToCamera = (Camera.main.transform.position - transform.position).normalized;
            float popDistance = 2f;
            Vector3 targetPosition = transform.position + (Vector3.up * .2f) + (directionToCamera * popDistance);
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(isCrit ? .15f : .1f, 0.15f))
                .Join(transform.DOMove(targetPosition, 0.6f))
                .Append(canvasGroup.DOFade(0, 1f))
                .OnComplete(() => Destroy(gameObject));
        }
    }
}
