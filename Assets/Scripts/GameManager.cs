using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace bitrush {
    public class GameManager : MonoBehaviour {
        public static GameManager Instance { get; private set; }

        void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }
        }

        public async void RestartLevel() {
            await Task.Delay(1000);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}