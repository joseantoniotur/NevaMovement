using System;
using UnityEngine;

namespace TarodevController
{
    /// <summary>
    /// Hey!
    /// Tarodev here. I built this controller as there was a severe lack of quality & free 2D controllers out there.
    /// I have a premium version on Patreon, which has every feature you'd expect from a polished controller. Link: https://www.patreon.com/tarodev
    /// You can play and compete for best times here: https://tarodev.itch.io/extended-ultimate-2d-controller
    /// If you hve any questions or would like to brag about your score, come to discord: https://discord.gg/tarodev
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [Header("Player Components")]
        public PlayerInputSystem playerInput;

        [Space]
        [SerializeField] private ScriptableStats _stats;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;

        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public event Action Dodged;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        private void Start()
        {
            _frameInput = new FrameInput
            {
                JumpDown = false,
                DodgeDown = false,
                Move = new Vector2(0f, 0f)
            };

            if (playerInput)
            { 
                playerInput.OnPlayerMove += OnMove;
                playerInput.OnPlayerJump += OnJump;
                playerInput.OnPlayerDodge += OnDodge;
            }
        }

        #region Inputs
        private void OnMove(Vector2 playerMovement)
        {
            _frameInput.Move.x = Mathf.Abs(playerMovement.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(playerMovement.x);
            if (_frameInput.Move.x != 0)
                transform.localScale = new Vector3(_frameInput.Move.x, 1f, 1f);
        }

        private void OnJump()
        {
            _frameInput.JumpDown = true;

            _jumpToConsume = true;
            _timeJumpWasPressed = Time.time;
        }

        private void OnDodge()
        {
            _frameInput.DodgeDown = true;

            _dodgeToConsume = true;
        }

        #endregion

        private void FixedUpdate()
        {
            CheckCollisions();

            HandleDirection();
            HandleDodge();

            HandleJump();
            HandleGravity();

            //Debug.Log($"player has jumped {_currentJumpCount} times");
            Debug.Log($"player velocity {_rb.linearVelocity}");

            ApplyMovement();
        }

        #region Collisions

        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Ground and Ceiling
            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

            // Hit a Ceiling
            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            // Landed on the Ground
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _currentJumpCount = 0;
                _currentDodgeCount = 0;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // Left the Ground
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = Time.time;
                GroundedChanged?.Invoke(false, 0);
            }

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion

        #region Jumping

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private int _currentJumpCount;

        private bool HasBufferedJump => _bufferedJumpUsable && Time.time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && Time.time < _frameLeftGrounded + _stats.CoyoteTime;
        private bool CanJump => _currentJumpCount < _stats.MaxJumpCount;

        private void HandleJump()
        {
            if (!HasFinishedDodge) return;

            if ((!_jumpToConsume && !HasBufferedJump) || (!_jumpToConsume && !CanJump) || _dodgeToConsume) return;

            if (_grounded || CanUseCoyote || CanJump) ExecuteJump();

            _jumpToConsume = false;
        }

        private void ExecuteJump()
        {
            _currentJumpCount += 1;
            _endedJumpEarly = false;
            _timeJumpWasPressed = Time.time;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = _stats.JumpPower;
            Jumped?.Invoke();
        }

        #endregion

        #region Horizontal

        private void HandleDirection()
        {
            if (!HasFinishedDodge) return;

            if (_frameInput.Move.x == 0)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Dodging

        private bool _dodgeToConsume;
        private int _currentDodgeCount;
        private float _timeDodgeWasStarting;
        private bool HasFinishedDodge  => Time.time > _timeDodgeWasStarting + _stats.DodgeDuration;
        private bool CanDodge => _currentDodgeCount < _stats.MaxDodgeCount;

        private void HandleDodge()
        {
            if (_dodgeToConsume && CanDodge && HasFinishedDodge) ExecuteDodge();

            if (HasFinishedDodge && _dodgeToConsume)
            {
                Debug.Log("player has finished dodging");

                KillMomentum();
                _dodgeToConsume = false;
                _currentDodgeCount = 0;
            }
        }

        private void ExecuteDodge()
        {
            KillMomentum();

            _currentDodgeCount += 1;
            _timeDodgeWasStarting = Time.time;

            _frameVelocity.x = _stats.DodgePower * (_frameInput.Move.x < 1f ? transform.localScale.x : _frameInput.Move.x);
            _frameVelocity.y = 0f;
            Dodged?.Invoke();
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            if (!HasFinishedDodge) return;

            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else
            {
                var inAirGravity = _stats.FallAcceleration;
                if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }

        #endregion

        private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;
        private void KillMomentum() => _rb.linearVelocity = Vector2.zero;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
#endif
    }

    public struct FrameInput
    {
        public bool JumpDown;
        public bool DodgeDown;
        public Vector2 Move;
    }

    public interface IPlayerController
    {
        public Vector2 FrameInput { get; }

        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public event Action Dodged;
    }
}