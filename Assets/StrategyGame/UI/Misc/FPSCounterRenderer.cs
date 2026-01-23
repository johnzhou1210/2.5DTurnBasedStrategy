using TMPro;
using UnityEngine;

namespace StrategyGame.UI.Misc {
    public class FPSCounterRenderer : MonoBehaviour {
        [SerializeField]
        private TextMeshProUGUI fpsText;
        private float _timer;
        private int _frames;
        private void Update() {
            _frames += 1;
            _timer += Time.unscaledDeltaTime;
            if (_timer >= 0.5f) {
                float fps = _frames / _timer;
                fpsText.SetText($"{fps:0} FPS");
                _frames = 0;
                _timer = 0;
            }
        }
    }
}
