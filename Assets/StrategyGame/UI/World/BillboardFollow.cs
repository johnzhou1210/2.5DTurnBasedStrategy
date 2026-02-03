using UnityEngine;

namespace StrategyGame.UI.World {
    public class BillboardFollow : MonoBehaviour {
        [SerializeField] private Transform target;
        [SerializeField] Vector3 offset = new Vector3(0, -1f, 0.25f);

        private void LateUpdate() {
            if (target != null) {
                transform.position = target.position + offset;
            }
        }
        public void SetTarget(Transform targetTransform) {
            target = targetTransform;
        }
    }
}

