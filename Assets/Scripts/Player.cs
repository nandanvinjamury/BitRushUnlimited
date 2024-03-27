using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

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

        [Header("Player References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private float _moveSpeed;
        private float _horizontalAcceleration;
        private float _jumpForce;
        private float _jumpBufferTimer;
        private float _coyoteTimer;
        private bool _bufferedJump;
        private bool _isHurt;

        void Start() {
            Application.targetFrameRate = 60;
            _isHurt = false;
            _gravity = -(_jumpHeight * 2) / (_jumpTime*_jumpTime); // (gt^2)/2 = h   ->   g = 2h/t^2
            _jumpForce = Mathf.Abs(_gravity) * _jumpTime; //v = gt
            _horizontalAcceleration = (_maxSpeed - _minSpeed) / (_accelerationTime * Application.targetFrameRate); // (vf - vi) / (t*framerate)
        }

        protected override void CalculateVelocity() {
            if (_isHurt) return; //lockout input when hurt
            MovementInput();
            JumpInput();
        }

        protected override void Move(Vector2 velocity) {
            _isGrounded = false;
            CheckHorizontalCollisions(ref velocity);
            RaycastHit2D hit = CheckVerticalCollisions(ref velocity);
            if (hit) {
                switch (hit.transform.tag) {
                    case "Spikes":
                        HurtPlayer(ref velocity);
                        break;
                    default:
                        break;
                }
            }
            transform.Translate(velocity);
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

        protected override RaycastHit2D CheckVerticalCollisions(ref Vector2 velocity) {
            if(_isHurt) return new RaycastHit2D(); //turn off collisions when hurt
            return base.CheckVerticalCollisions(ref velocity);
        }

        void HurtPlayer(ref Vector2 velocity) {
            _spriteRenderer.color = new Color(1, 0, 0, 1);
            transform.localScale = new Vector3(1f, 0.85f, 1);
            _isHurt = true;
            velocity.y = 5*_jumpForce*Time.deltaTime;
            //TODO: Reset Level
        }
    }
}