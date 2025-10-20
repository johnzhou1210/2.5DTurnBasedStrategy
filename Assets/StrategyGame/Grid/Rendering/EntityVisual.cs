using System;
using UnityEngine;

namespace StrategyGame.Grid.Rendering {
    public class EntityVisual : MonoBehaviour {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        [SerializeField] private Renderer ringRenderer;

        private void Start() {
            ringRenderer.material.EnableKeyword("_EMISSION");
        }

        public void SetColor(Color c) {
            ringRenderer.material.color = c;
            ringRenderer.material.SetColor(EmissionColor, c * 5f);
        }
    }
}
