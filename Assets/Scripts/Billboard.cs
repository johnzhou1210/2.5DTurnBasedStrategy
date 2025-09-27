using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void LateUpdate() {
        transform.LookAt(targetTransform);
        transform.forward = Camera.main.transform.forward;
        
    }

}
