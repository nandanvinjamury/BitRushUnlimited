using UnityEngine;

namespace bitrush {
    public class Player : CharacterController2D {

        [Header("Player Variables")]
        [SerializeField] private float _moveSpeed = 10f;
        [Space]
        [SerializeField] private float _jumpHeight = 3.5f;
        [SerializeField] private float _jumpTime = 0.35f;
        [SerializeField] private float _jumpBufferTime = 0.1f;
        [SerializeField] private float _coyoteTime = 0.1f;

        private float _jumpForce;
        private float _jumpBufferTimer;
        private float _coyoteTimer;

        void Start() {
            Application.targetFrameRate = 30;
            _gravity = -(_jumpHeight * 2) / (_jumpTime*_jumpTime); // (gt^2)/2 = h   ->   g = 2h/t^2
            _jumpForce = Mathf.Abs(_gravity) * _jumpTime; //v = gt
        }

        protected override void CalculateVelocity() {
            _velocity.x = Input.GetAxisRaw("Horizontal") * _moveSpeed;
            JumpInput();
        }

        void JumpInput() {
            if (!_isGrounded) {
                _coyoteTimer += Time.deltaTime;
            } else {
                _coyoteTimer = 0;
            }

            if (!Input.GetKey(KeyCode.Space)) return;
            _jumpBufferTimer += Time.deltaTime;

            if (_isGrounded && _jumpBufferTime >= _jumpBufferTimer) {
                _jumpBufferTimer = 0;
                _velocity.y = _jumpForce;
            } else if(_coyoteTime >= _coyoteTimer) {
                _velocity.y = _jumpForce;
            }
        }
    }
}