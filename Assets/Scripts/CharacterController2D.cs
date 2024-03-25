using System;
using UnityEngine;

namespace bitrush {
    [RequireComponent(typeof(BoxCollider2D))]
    public class CharacterController2D : MonoBehaviour {
        private struct RaycastOrigins {
            public Vector2 Top, Left, Right;
        }

        [Header("Collisions")]
        [SerializeField][Range(2,32)] private int _horizontalRayCount = 8;
        [SerializeField][Range(2,32)] private int _verticalRayCount = 8;
        [SerializeField] private BoxCollider2D _collider;
        [SerializeField] private LayerMask _collisionMask;

        protected float _gravity;
        protected Vector2 _velocity;
        protected bool _isGrounded;
        private readonly float _skinWidth = 0.015f;
        private float _hRaySpacing, _vRaySpacing;
        private RaycastOrigins _raycastOrigins;

        void Update() {
            UpdateRaycastOrigins();
            AddGravity();
            CalculateVelocity();
            Move(_velocity * Time.deltaTime);
        }

        protected void UpdateRaycastOrigins() {
            Bounds bounds = _collider.bounds;
            Vector2 size = _collider.size;
            _raycastOrigins.Left = new Vector2(bounds.min.x + _skinWidth, bounds.min.y + _skinWidth);
            _raycastOrigins.Right = new Vector2(bounds.max.x - _skinWidth, bounds.min.y + _skinWidth);
            _raycastOrigins.Top = new Vector2(bounds.min.x + _skinWidth, bounds.max.y - _skinWidth);
            _hRaySpacing = (size.y - 2*_skinWidth) / (_horizontalRayCount - 1);
            _vRaySpacing = (size.x - 2*_skinWidth) / (_verticalRayCount - 1);
        }

        protected void AddGravity() {
            if (_isGrounded) {
                _velocity.y = 0;
            } else {
                _velocity.y += _gravity * Time.deltaTime;
            }
        }

        protected virtual void CalculateVelocity() {}

        protected void Move(Vector2 velocity) {
            _isGrounded = false;
            CheckHorizontalCollisions(ref velocity);
            CheckVerticalCollisions(ref velocity);
            transform.Translate(velocity);
        }

        private void CheckHorizontalCollisions(ref Vector2 velocity) {
            if(velocity.x == 0) return;
            Vector2 dirX = Math.Sign(velocity.x) * Vector2.right;
            Vector2 origin = velocity.x > 0 ? _raycastOrigins.Right : _raycastOrigins.Left;
            float distance = _skinWidth + Mathf.Abs(velocity.x);
            for (int i = 0; i < _horizontalRayCount; i++) {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(origin.x, origin.y + i * _hRaySpacing), dirX, distance, _collisionMask);
                //Debug.DrawRay(new Vector2(origin.x, origin.y + i * _hRaySpacing), dirX * distance, Color.red); //Only for debugging
                if (hit) {
                    velocity.x = dirX.x < 0 ? _skinWidth - hit.distance : hit.distance - _skinWidth;
                    distance = hit.distance;
                }
            }
        }

        private void CheckVerticalCollisions(ref Vector2 velocity) {
            int dirY = Math.Sign(velocity.y);
            Vector2 direction = dirY == 0 ? Vector2.down : dirY * Vector2.up;
            Vector2 origin = velocity.y > 0 ? _raycastOrigins.Top : _raycastOrigins.Left;
            float distance = _skinWidth + Mathf.Abs(velocity.y);
            for (int i = 0; i < _verticalRayCount; i++) {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(origin.x + i * _vRaySpacing + velocity.x, origin.y), direction, distance, _collisionMask);
                //Debug.DrawRay(new Vector2(origin.x + i * _vRaySpacing + velocity.x, origin.y), direction * distance, Color.red); //Only for debugging
                if (hit) {
                    velocity.y = dirY <= 0 ? _skinWidth - hit.distance : hit.distance - _skinWidth;
                    distance = hit.distance;
                    _isGrounded = dirY <= 0;
                }
            }
        }
    }
}