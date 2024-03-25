using UnityEngine;

namespace bitrush {
    public class Player : CharacterController2D {

        [Header("Player Variables")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpHeight = 3f;
        [SerializeField] private float _jumpTime = 0.4f;

        private float _jumpForce;

        void Start() {
            _gravity = -(_jumpHeight * 2) / Mathf.Pow(_jumpTime, 2); //0.5gt^2 = h -> g = 2h/t^2
            _jumpForce = Mathf.Abs(_gravity) * _jumpTime; //v = gt
        }
        void Update() {
            UpdateRaycastOrigins();
            AddGravity();
            CalculateVelocity();
            Move(_velocity * Time.deltaTime);
        }

        protected override void CalculateVelocity() {
            base.CalculateVelocity();
            //TODO: Improve input handling with buffering, animation, smoothness, etc.
            _velocity.x = Input.GetAxisRaw("Horizontal") * _moveSpeed;
            if (Input.GetKeyDown(KeyCode.Space) && _isGrounded) {
                _velocity.y = _jumpForce;
            }
        }
    }
}