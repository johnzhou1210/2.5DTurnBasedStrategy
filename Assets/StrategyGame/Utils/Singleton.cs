using UnityEngine;

namespace StrategyGame.Utils {
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
        public static T Instance { get; protected set; }
        protected virtual void Awake() {
            if (Instance != null && Instance != this) {
                Debug.LogWarning($"Singleton.Awake(): {typeof(T).Name} singleton already exists.");
                Destroy(gameObject);
                return;
            }
            Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        protected virtual void OnDestroy() {
            if (Instance == this) {
                Instance = null;
            }
        }
    }
}
