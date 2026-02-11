using UnityEngine;

namespace StrategyGame.Utils.Effects {
    public class AutoCleanup : MonoBehaviour {
        [SerializeField] private float cleanupTime = 2f;

        private void Start() {
            Invoke(nameof(Cleanup), cleanupTime);
        }

        private void Cleanup() {
            Destroy(gameObject);
        }
    }
}
