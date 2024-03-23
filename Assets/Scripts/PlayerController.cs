using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace bitrush {
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour {

        private struct RaycastOrigins {
            public Vector2 Top, Left, Right;
        }

        [SerializeField] private BoxCollider2D _collider;
        [SerializeField] private LayerMask _collisionMask;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpHeight = 3f;
        [SerializeField] private float _jumpTime = 0.4f;
        [SerializeField][Range(2,32)] private int _horizontalRayCount = 8;
        [SerializeField][Range(2,32)] private int _verticalRayCount = 8;

        private readonly float _skinWidth = 0.015f;
        private float _gravity;
        private float _jumpForce;
        private float _hRaySpacing, _vRaySpacing;
        private bool _isGrounded;
        private Vector2 _velocity;
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
        }

        void UpdateRaycastOrigins() {
            Bounds bounds = _collider.bounds;
            Vector2 size = _collider.size;
            _raycastOrigins.Left = new Vector2(bounds.min.x + _skinWidth, bounds.min.y + _skinWidth);
            _raycastOrigins.Right = new Vector2(bounds.max.x - _skinWidth, bounds.min.y + _skinWidth);
            _raycastOrigins.Top = new Vector2(bounds.min.x + _skinWidth, bounds.max.y - _skinWidth);
            _hRaySpacing = (size.y - 2*_skinWidth) / (_horizontalRayCount - 1);
            _vRaySpacing = (size.x - 2*_skinWidth) / (_verticalRayCount - 1);
        }

        void AddGravity() {
            if (_isGrounded) {
                _velocity.y = 0;
            } else {
                _velocity.y += _gravity * Time.deltaTime;
            }
        }

        void CheckInput() {
            //TODO: Improve input handling with buffering, animation, smoothness, etc.
            _velocity.x = Input.GetAxisRaw("Horizontal") * _moveSpeed;
            if (Input.GetKeyDown(KeyCode.Space) && _isGrounded) {
                _velocity.y = _jumpForce;
            }
        }

        void Move(Vector2 velocity) {
            _isGrounded = false;
            CheckHorizontalCollisions(ref velocity);
            CheckVerticalCollisions(ref velocity);
            transform.Translate(velocity);
        }

        void CheckHorizontalCollisions(ref Vector2 velocity) {
            if(velocity.x == 0) return;
            Vector2 dirX = Mathf.Sign(velocity.x) * Vector2.right;
            Vector2 origin = velocity.x < 0 ? _raycastOrigins.Left : _raycastOrigins.Right;
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

        void CheckVerticalCollisions(ref Vector2 velocity) {
            if(velocity.y == 0) return;
            Vector2 dirY = Mathf.Sign(velocity.y) * Vector2.up;
            Vector2 origin = velocity.y < 0 ? _raycastOrigins.Left : _raycastOrigins.Top;
            float distance = _skinWidth + Mathf.Abs(velocity.y);
            for (int i = 0; i < _verticalRayCount; i++) {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(origin.x + i * _vRaySpacing + velocity.x, origin.y), dirY, distance, _collisionMask);
                //Debug.DrawRay(new Vector2(origin.x + i * _vRaySpacing + velocity.x, origin.y), dirY * distance, Color.red); //Only for debugging
                if (hit) {
                    velocity.y = dirY.y < 0 ? _skinWidth - hit.distance : hit.distance - _skinWidth;
                    distance = hit.distance;
                    _isGrounded = dirY.y < 0;
                }
            }
        }
    }
}