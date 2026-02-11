using DG.Tweening;
using UnityEngine;

namespace StrategyGame.Utils.Effects {
    public class ContinuousSpin : MonoBehaviour {
        [SerializeField] private float singleCycleDuration = 2f;
        [Header("Set axis: 0=x, 1=y, 2=z")]
        [SerializeField] private int axis = 1;
        private void Start() {
            Vector3 rotationAxis = axis switch
            {
                0 => new Vector3(360, 0, 0),
                1 => new Vector3(0, 360, 0),
                2 => new Vector3(0, 0, 360),
                _ => Vector3.zero
            };
            transform
                .DOLocalRotate(rotationAxis, singleCycleDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetRelative()
                .SetLoops(-1, LoopType.Restart);
        }
    }
}
