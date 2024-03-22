using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace bitrush {
    public class PlayerController : MonoBehaviour {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpHeight = 3f;
        [SerializeField] private float _jumpTime = 0.4f;
        [SerializeField] private LayerMask _collisionMask;
        private struct RaycastOrigins {
            public Vector2 Bottom, Left;
        }

        private float _gravity;
        private Vector2 _velocity;
        private bool _isGrounded;
        private float _jumpForce;
        private const float HWidth = 0.475f, HHeight = 0.975f; //half width and half height
        private const float HRaySpacing = (2f*HHeight)/7, VRaySpacing = (2f*HWidth)/7; //8 raycasts for each side, 1.9 to leave buffer
        private RaycastOrigins _raycastOrigins;

        void Start() {
            _gravity = -(_jumpHeight * 2) / Mathf.Pow(_jumpTime, 2); //0.5gt^2 = h -> g = 2h/t^2
            _jumpForce = Mathf.Abs(_gravity) * _jumpTime; //v = gt
        }


        void Update() {
            UpdateRaycastOrigins();
            AddGravity();
            CheckInput();
            Move(_velocity * Time.deltaTime);

            if (transform.position.y < -10f) {
                transform.position = new Vector3(0, 0, 0);
                _velocity = Vector2.zero;
            }
        }

        void UpdateRaycastOrigins() {
            Vector2 position = transform.position;
            _raycastOrigins.Bottom = new Vector2(position.x, position.y - HHeight);
            _raycastOrigins.Left = new Vector2(position.x - HWidth, position.y);
        }

        void AddGravity() {
            Vector2 minX = _raycastOrigins.Bottom - new Vector2(0.35f, 0);
            Vector2 maxX = _raycastOrigins.Bottom + new Vector2(0.35f, 0);
            _isGrounded = Physics2D.OverlapCircle(minX, 0.05f, _collisionMask) || Physics2D.OverlapCircle(maxX, 0.05f, _collisionMask);
            if (_isGrounded) {
                _velocity.y = 0;
            } else {
                _velocity.y += _gravity * Time.deltaTime;
            }
        }

        void CheckInput() {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                _velocity.x = -_moveSpeed;
            } else if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                _velocity.x = _moveSpeed;
            } else {
                _velocity.x = 0;
            }

            if (Input.GetKeyDown(KeyCode.Space) && _isGrounded) {
                _velocity.y = _jumpForce;
            }
        }

        void Move(Vector2 velocity) {
            CheckHorizontalCollisions(ref velocity);
            CheckVerticalCollisions(ref velocity);
            transform.Translate(velocity);
        }

        void CheckHorizontalCollisions(ref Vector2 velocity) {
            if(velocity.x == 0) return;
            float dirX = Mathf.Sign(velocity.x);
            for (int i = 0; i < 8; i++) {
                RaycastHit2D hit = Physics2D.Raycast(_raycastOrigins.Bottom + Vector2.up * (i * HRaySpacing), Vector2.right * dirX,
                    HWidth + velocity.x, _collisionMask);
                Debug.DrawRay(_raycastOrigins.Bottom + Vector2.up * (i * HRaySpacing), Vector2.right * (dirX * (HWidth + velocity.x)), Color.red);
                if (hit) {
                    velocity.x = (hit.distance - HWidth) * dirX;
                    break;
                }
            }
        }

        void CheckVerticalCollisions(ref Vector2 velocity) {
            if(velocity.y == 0) return;
            float dirY = Mathf.Sign(velocity.y);
            for (int i = 0; i < 8; i++) {
                RaycastHit2D hit = Physics2D.Raycast(_raycastOrigins.Left + Vector2.right * (i * VRaySpacing), Vector2.up * dirY,
                    HHeight + velocity.y, _collisionMask);
                Debug.DrawRay(_raycastOrigins.Left + Vector2.right * (i * VRaySpacing), (Vector2.up) * (dirY * (HHeight + velocity.y)), Color.red);
                if (hit) {
                    velocity.y = (hit.distance - HHeight) * dirY;
                    break;
                }
            }
        }
    }
}