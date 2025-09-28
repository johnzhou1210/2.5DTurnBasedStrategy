using UnityEngine;

public class BillboardFollow : MonoBehaviour {
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);

    private void LateUpdate() {
        if (target != null) {
            transform.position = target.position + offset;
        }
    }
    public void SetTarget(Transform targetTransform) {
        target = targetTransform;
    }
}

