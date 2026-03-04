using UnityEngine;
using System.Collections;

namespace StrategyGame.Utils.Effects {
    public class AutoCleanup : MonoBehaviour {
        [SerializeField] private float cleanupDelay = 2f;

        private ParticleSystem[] particleSystems;

        private void Awake() {
            particleSystems = GetComponentsInChildren<ParticleSystem>();
        }

        private void Start() {
            Invoke(nameof(BeginCleanup), cleanupDelay);
        }

        private void BeginCleanup() {
            StartCoroutine(FadeAndDestroy());
        }

        private IEnumerator FadeAndDestroy() {
            // Stop emitting new particles
            foreach (var ps in particleSystems) {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            // Wait until all particles are gone
            bool particlesAlive = true;
            while (particlesAlive) {
                particlesAlive = false;

                foreach (var ps in particleSystems) {
                    if (ps == null) continue;
                    if (ps.IsAlive(true)) {
                        particlesAlive = true;
                        break;
                    }
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
