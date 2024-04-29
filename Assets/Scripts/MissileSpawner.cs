using UnityEngine;

namespace bitrush {
    public class MissileSpawner : MonoBehaviour {
        [SerializeField] private GameObject _missilePrefab;

        private void Update() {
            if (Input.GetButtonDown("Fire1")) {
                Instantiate(_missilePrefab, transform.position, Quaternion.identity);
            }
        }
    }
}