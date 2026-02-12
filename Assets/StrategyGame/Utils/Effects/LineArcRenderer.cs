using System;
using UnityEngine;

public class LineArcRenderer : MonoBehaviour
{
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 1f;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 3f;
    [SerializeField] private Color baseEmissionColor = Color.red;
    
    private Material _lineMaterial;

    private void Awake() {
        _lineMaterial = lineRenderer.material;
    }
    
    public void DrawArc(Vector3 start, Vector3 end)
    {
        int resolution = 20;
        lineRenderer.positionCount = resolution;

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);

            Vector3 point = Vector3.Lerp(start, end, t);

            // Add arc height
            float height = 2f;
            point.y += Mathf.Sin(t * Mathf.PI) * height;

            lineRenderer.SetPosition(i, point);
        }
    }
    
    private void Update()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        Color c = _lineMaterial.color;
        c.a = alpha;
        _lineMaterial.color = c;
        _lineMaterial.SetColor(EmissionColor, baseEmissionColor * intensity);
    }

}
