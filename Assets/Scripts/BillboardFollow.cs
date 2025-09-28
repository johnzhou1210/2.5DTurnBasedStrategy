using UnityEngine;

public class FollowTarget : MonoBehaviour {
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);

    void LateUpdate() {
        if (target != null) {
            transform.position = target.position + offset;
        }
    }
}

