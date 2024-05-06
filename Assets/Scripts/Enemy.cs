using TMPro;
using UnityEngine;

namespace bitrush {
    public class Enemy : MonoBehaviour {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private float _movementSpeed = 5f;
        [SerializeField] private LayerMask _collisionMask;
        [SerializeField] private bool _isHorizontal = true;

        private Vector2 _direction;
        private RaycastHit2D _wallHit;
        private RaycastHit2D _floorHit;

        private void Start() {
            _direction = _isHorizontal ? Vector2.right : Vector2.up;
        }

        private void Update() {
            if (_isHorizontal) {
                _rb.velocity = _direction * _movementSpeed;
                Vector2 position = transform.position;
                _wallHit = Physics2D.Raycast(position, _direction, 1f, _collisionMask);
                if (_wallHit) {
                    _direction *= -1;
                }

                _floorHit = Physics2D.Raycast(position + _direction, Vector2.down, 1f, _collisionMask);
                if (!_floorHit) {
                    _direction *= -1;
                }
            } else {
                _rb.velocity = _direction * _movementSpeed;
                Vector2 position = transform.position;
                _wallHit = Physics2D.Raycast(position, _direction, 1f, _collisionMask);
                if (_wallHit) {
                    _direction *= -1;
                }
            }
        }
    }
}