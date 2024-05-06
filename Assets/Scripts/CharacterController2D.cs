using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace bitrush {
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterController2D : MonoBehaviour {
        [Header("Player Variables")]
        [SerializeField] private float _movementSpeed = 5f;
        [Space]
        [SerializeField] private float _jumpHeight = 3.5f;
        [SerializeField] private float _jumpTime = 0.35f;
        [SerializeField] private float _jumpBufferTime = 0.1f;
        [SerializeField] private float _coyoteTime = 0.1f;
        [SerializeField] private float _fallTime = 0.2f;
        [SerializeField] private float _parachuteFallSpeed = 100f;

        [Header("Player References")]
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private CapsuleCollider2D _collider;
        [SerializeField] private Tilemap _blueTilemap;
        [SerializeField] private Tilemap _orangeTilemap;
        [SerializeField] private TilemapCollider2D _blueTilemapCollider;
        [SerializeField] private TilemapCollider2D _orangeTilemapCollider;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _gliderSprite;
        [SerializeField] private PlatformEffector2D _platformEffector;
        [SerializeField] private ParticleSystem _dustParticles;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private LayerMask _groundLayer;

        private Vector2 _velocity;
        private float _jumpForce;
        private float _jumpBufferTimer;
        private float _coyoteTimer;
        private float _fallTimer;
        private bool _bufferedJump;
        private bool _isGrounded;
        private bool _isUnderwater;
        private bool _isHurt;
        private bool _isGliding;
        private bool _blueJump = true;

        void Start() {
            Application.targetFrameRate = 60;
            Physics2D.gravity = new Vector2(0,-(2 * _jumpHeight) / Mathf.Pow(_jumpTime, 2));
            _jumpForce = Mathf.Abs(Physics2D.gravity.y) * _jumpTime;
        }

        void Update() {
            AddGravity();
            MovementInput();
            JumpInput();
            FallInput();
            _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, 0.05f, _groundLayer);
            if(!_isGrounded || _isUnderwater) {
                _dustParticles.Stop();
            } else if(_dustParticles.isStopped && (_isGrounded||!_isUnderwater)) {
                _dustParticles.Play();
            }

            _rb.velocity = _rb.velocity.y < 0 ? new Vector2(_velocity.x, _rb.velocity.y * 1 / _parachuteFallSpeed) : new Vector2(_velocity.x, _rb.velocity.y);

            if(_isGliding) {
                Glider().Forget();
                _isGliding = false;
            }
        }

        void MovementInput() {
            if (_isHurt) return;
            _velocity.x = Input.GetAxisRaw("Horizontal") * _movementSpeed;

            if (_velocity.x < 0) {
                _spriteRenderer.flipX = true;
                _gliderSprite.flipX = true;
            } else if(_velocity.x > 0) {
                _spriteRenderer.flipX = false;
                _gliderSprite.flipX = false;
            }
        }

        void JumpInput() {
            if(_isHurt) return;
            if (_isGrounded || _isUnderwater) {
                //If the player is grounded or buffered a jump previously, they can jump
                if (Input.GetButtonDown("Jump") || (_bufferedJump && _jumpBufferTimer <= _jumpBufferTime)) {
                    _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);
                    BlueOrangePlatforms();
                }
                //reset timers and flags
                _bufferedJump = false;
                _jumpBufferTimer = 0;
                _coyoteTimer = 0;
            } else {
                //If the player is not grounded, pressing jump within the coyote time will still allow them to jump
                if (Input.GetButtonDown("Jump") && _coyoteTimer <= _coyoteTime) {
                    _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);
                    BlueOrangePlatforms();
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

        void FallInput() {
            if(_isHurt) return;
            if (Input.GetAxisRaw("Vertical") < 0) {
                _platformEffector.rotationalOffset = 180;
                _fallTimer = 0;
            }
            if (_platformEffector.rotationalOffset != 0) {
                _fallTimer += Time.deltaTime;
            }
            if(_fallTimer >= _fallTime) {
                _platformEffector.rotationalOffset = 0;
                _fallTimer = 0;
            }
        }

        void AddGravity() {
            if (_rb.velocity.y < 0) {
                _rb.velocity += Vector2.up * (1.5f * Time.deltaTime * Physics2D.gravity.y);
            } else if (_rb.velocity.y > 0 && !Input.GetButton("Jump")) {
                _rb.velocity += Vector2.up * (Time.deltaTime * Physics2D.gravity.y);
            }
            Vector2 rbVelocity = _rb.velocity;
            _rb.velocity = new Vector2(rbVelocity.x, Mathf.Clamp(rbVelocity.y, -_jumpForce, _jumpForce));
        }

        void HurtPlayer() {
            _isHurt = true;
            _parachuteFallSpeed = 1f;
            _gliderSprite.gameObject.SetActive(false);
            _spriteRenderer.color = new Color(1, 0, 0, 1);
            transform.localScale = new Vector3(1f, 0.85f, 1);
            _collider.enabled = false;
            _velocity.x = 0;
            _rb.velocity = new Vector2(0, 5*_jumpForce);
            GameManager.Instance.RestartLevel();
        }

        void BlueOrangePlatforms() {
            _blueJump = !_blueJump;
            if (_blueJump) {
                _blueTilemapCollider.enabled = true;
                _orangeTilemapCollider.enabled = false;
                _blueTilemap.color = new Color(.075f, .871f, 0.933f, 1f);
                _orangeTilemap.color = new Color(1f, 0.471f, 0f, 0.1f);
            } else {
                _orangeTilemapCollider.enabled = true;
                _blueTilemapCollider.enabled = false;
                _orangeTilemap.color = new Color(1f, 0.471f, 0f, 1f);
                _blueTilemap.color = new Color(.075f, .871f, 0.933f, 0.1f);
            }
        }

        async UniTaskVoid Glider() {
            _parachuteFallSpeed = 25f;
            _gliderSprite.gameObject.SetActive(true);
            float timer = 0;
            while (timer < 5f) {
                await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
                timer += Time.deltaTime;
            }
            _gliderSprite.gameObject.SetActive(false);
            _parachuteFallSpeed = 1f;
        }

        async UniTaskVoid BlackHole() {
            Physics2D.gravity *= 0.5f;
            float timer = 0;
            while (timer < 5f) {
                await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
                timer += Time.deltaTime;
            }
            Physics2D.gravity *= 2f;
        }

        async UniTaskVoid Swap() {
            _movementSpeed *= -1f;
            float timer = 0;
            while (timer < 5f) {
                await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
                timer += Time.deltaTime;
            }
            _movementSpeed *= -1f;
        }

        private void OnCollisionEnter2D(Collision2D col) {
            if(col.gameObject.CompareTag("Spikes")){
                HurtPlayer();
            }
            if(col.gameObject.CompareTag("MovingPlatform")) {
                transform.parent = col.transform;
            }

            if (col.gameObject.CompareTag("Destroy")) {
                col.gameObject.SetActive(false);
            }
        }

        private void OnCollisionExit2D(Collision2D col) {
            if(col.gameObject.CompareTag("MovingPlatform")) {
                transform.parent = null;
            }
        }

        private void OnTriggerEnter2D(Collider2D col) {
            if(col.gameObject.CompareTag("Water")) {
                _isUnderwater = true;
            }

            if (col.gameObject.CompareTag("Glider")) {
                _isGliding = true;
                Destroy(col.gameObject);
            }

            if(col.gameObject.CompareTag("Spikes")){
                HurtPlayer();
            }

            if (col.gameObject.CompareTag("BlackHole")) {
                BlackHole().Forget();
                Destroy(col.gameObject);
            }

            if (col.gameObject.CompareTag("Swap")) {
                Swap().Forget();
                Destroy(col.gameObject);
            }
        }

        private void OnTriggerExit2D(Collider2D col) {
            if(col.gameObject.CompareTag("Water")) {
                _isUnderwater = false;
            }
        }
    }
}