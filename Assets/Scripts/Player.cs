using UnityEngine;

namespace bitrush {
    public class Player : CharacterController2D {

        [Header("Player Variables")]
        [SerializeField] private float _minSpeed = 5f;
        [SerializeField] private float _maxSpeed = 20f;
        [SerializeField] private float _accelerationTime = 3f;
        [Space]
        [SerializeField] private float _jumpHeight = 3.5f;
        [SerializeField] private float _jumpTime = 0.35f;
        [SerializeField] private float _jumpBufferTime = 0.1f;
        [SerializeField] private float _coyoteTime = 0.1f;

        private float _moveSpeed;
        private float _horizontalAcceleration;
        private float _jumpForce;
        private float _jumpBufferTimer;
        private float _coyoteTimer;
        private bool _bufferedJump;

        void Start() {
            Application.targetFrameRate = 60;
            _gravity = -(_jumpHeight * 2) / (_jumpTime*_jumpTime); // (gt^2)/2 = h   ->   g = 2h/t^2
            _jumpForce = Mathf.Abs(_gravity) * _jumpTime; //v = gt
            _horizontalAcceleration = (_maxSpeed - _minSpeed) / (_accelerationTime * Application.targetFrameRate); // (vf - vi) / (t*framerate)
        }

        protected override void CalculateVelocity() {
            MovementInput();
            JumpInput();
        }

        void MovementInput() {
            if (Input.GetAxisRaw("Horizontal") == 0) {
                _moveSpeed = _minSpeed;
            } else if(_moveSpeed < _maxSpeed) {
                _moveSpeed += _horizontalAcceleration;
                _moveSpeed = Mathf.Clamp(_moveSpeed, _minSpeed, _maxSpeed);
            }
            _velocity.x = Input.GetAxisRaw("Horizontal") * _moveSpeed;
        }

        void JumpInput() {
            if (_isGrounded) {
                //If the player is grounded or buffered a jump previously, they can jump
                if (Input.GetButtonDown("Jump") || (_bufferedJump && _jumpBufferTimer <= _jumpBufferTime)) {
                    _velocity.y = _jumpForce;
                }
                //reset timers and flags
                _bufferedJump = false;
                _jumpBufferTimer = 0;
                _coyoteTimer = 0;
            } else {
                //If the player is not grounded, pressing jump within the coyote time will still allow them to jump
                if (Input.GetButtonDown("Jump") && _coyoteTimer <= _coyoteTime) {
                    _velocity.y = _jumpForce;
                //If the coyote time has passed, pressing jump will attempt to buffer the jump
                } else if(Input.GetButtonDown("Jump")) {
                    _bufferedJump = true;
                }
                //increment timers
                if (_bufferedJump) {
                    _jumpBufferTimer += Time.deltaTime;
                }
                _coyoteTimer += Time.deltaTime;
            }
        }
    }
}