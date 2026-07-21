using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StrategyGame.Core.Bootstrap {
    public class GameBootstrap : MonoBehaviour {
        private void Start() {
            SceneManager.LoadScene("Scenes/TitleScene");
        }
    }
}
