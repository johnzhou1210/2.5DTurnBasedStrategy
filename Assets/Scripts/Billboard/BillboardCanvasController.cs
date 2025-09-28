using System;
using UnityEngine;

public class BillboardCanvasController : MonoBehaviour
{
    private void OnEnable() {
        BillboardDelegates.GetBillboardCanvasTransform = GetTransform;
    }

    private void OnDisable() {
        BillboardDelegates.GetBillboardCanvasTransform = null;
    }

    private Transform GetTransform() {
        return transform;
    }
}
