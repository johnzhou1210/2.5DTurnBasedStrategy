using UnityEngine;

public class CombatActionItemEventHandler : MonoBehaviour {
    [SerializeField] private Animator highlightAnimator;
    public void StopHighlightAnim() {
        highlightAnimator.enabled = false;
    }

    public void StartHighlightAnim() {
        highlightAnimator.enabled = true;
    }
}
