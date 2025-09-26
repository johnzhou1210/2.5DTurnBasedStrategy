using UnityEngine;

public class Billboard : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void LateUpdate() {
        transform.forward = Camera.main.transform.forward;
    }

}
