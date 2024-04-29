using UnityEngine;

namespace bitrush {
    public class Missile : MonoBehaviour {
        [SerializeField] private float _speed = 20f;
        [SerializeField] private float _lifeTime = 5f;

        private void Start() {
            Destroy(gameObject, _lifeTime);
        }

        private void Update() {
            transform.Translate(Vector3.right * _speed * Time.deltaTime);
        }
    }
}