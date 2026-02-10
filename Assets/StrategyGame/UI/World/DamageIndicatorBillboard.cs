using DG.Tweening;
using TMPro;
using UnityEngine;

namespace StrategyGame.UI.World {
    public class DamageIndicatorBillboard : MonoBehaviour {
        [SerializeField] private GameObject normalAttackNumber;
        [SerializeField] private GameObject critAttackNumber;
        [SerializeField] private TextMeshProUGUI normalAttackNumberText;
        [SerializeField] private TextMeshProUGUI critAttackNumberText;
        [SerializeField] private CanvasGroup canvasGroup;
        public void Setup(int damage, bool isCrit, bool isHeal = false) {
            (isCrit ? critAttackNumber : normalAttackNumber).SetActive(true);
            TextMeshProUGUI targetText = (isCrit ? critAttackNumberText : normalAttackNumberText);
            targetText.SetText(damage.ToString());
            transform.localScale = Vector3.zero;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(isCrit ? .45f : .4f, 0.15f))
                .Join(transform.DOMoveY(transform.position.y + .8f, .6f))
                .Join(canvasGroup.DOFade(1, .5f))
                .Join(canvasGroup.DOFade(0, 1f))
                .OnComplete(() => Destroy(gameObject));
        }
    }
}
