using UnityEngine;

namespace MiktoGames
{
    public class PlayerMovement : MonoBehaviour
    {
        // Публичные константы
        public const float MinYVelocity = -2f;

        // Приватные константы

        // Поля с [SerializeField]
        [Header("Movement Settings")]
        [SerializeField] private float _walkSpeed = 10f;
        [SerializeField] private float _sprintSpeed = 15f;
        [SerializeField] private float _crouchSpeed = 6f;
        [SerializeField] private float _crouchedSprintSpeedMultiplier = 1.2f;
        [SerializeField] private float _jumpSpeed = 3f;
        [SerializeField] private float _pushForce;
        [SerializeField] private float _gravity = 9.81f;
        [SerializeField] private bool _canJump = true;
        [SerializeField] private bool _canSprint = true;
        [SerializeField] private bool _canCrouch = true;

        [SerializeField] private float _fallMultiplier = 2.5f;

        [Header("Crouching Settings")]
        [SerializeField] private float _crouchHeight = 1f;
        private float _originalHeight;

        [Header("Ground Settings")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundDistance = 0.3f;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private float _coyoteTimeDuration = 0.25f;

        [Header("Obstacle Detection")]
        [SerializeField] private float _obstacleCheckDistance = 0.5f;
        [SerializeField] private float _obstacleAngleThreshold = 45f;

        [SerializeField] private FootstepController _footstepController;

        [SerializeField] private float _fallLandingThreshold = 1f;

        [HideInInspector] private Vector2 _moveInput;
        [HideInInspector] private bool _isGrounded;
        [HideInInspector] private bool _isSprinting;
        [HideInInspector] private bool _isCrouching;

        [HideInInspector] private float _coyoteTimer = 0f;
        private bool _hasJumped = false;
        private bool _inFall = false;
        private float _fallStartHeight = 0f;

        private CharacterController _characterController;
        private Vector3 _moveDirection = Vector3.zero;
        private bool _wasGrounded = true;

        #region Public Properties

        public Vector2 MoveInput => _moveInput;
        public bool IsGrounded => _isGrounded;
        public bool IsSprinting => _isSprinting;
        public bool IsCrouching => _isCrouching;
        public float CoyoteTimer => _coyoteTimer;

        public bool CanSprint
        {
            get => _canSprint;
            set => _canSprint = value;
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _originalHeight = _characterController.height;
        }

        private void Update()
        {
            _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

            if (_isGrounded && _moveDirection.y < 0f)
            {
                _moveDirection.y = MinYVelocity;
                _coyoteTimer = _canJump ? _coyoteTimeDuration : 0f;
            }
            else if (_coyoteTimeDuration > 0f)
            {
                _coyoteTimer -= Time.deltaTime;
            }

            _moveInput.x = Input.GetAxis("Horizontal");
            _moveInput.y = Input.GetAxis("Vertical");

            _isCrouching = _canCrouch && _isGrounded && Input.GetKey(KeyCode.LeftControl);

            if (_isCrouching)
                _characterController.height = Mathf.Lerp(_characterController.height, _crouchHeight, Time.deltaTime * 10f);
            else
                _characterController.height = Mathf.Lerp(_characterController.height, _originalHeight, Time.deltaTime * 10f);

            Vector3 center = _characterController.center;
            center.y = _characterController.height * 0.5f;
            _characterController.center = center;

            bool obstacleBlocked = IsObstacleBlocking();
            bool isShiftPressed = Input.GetKey(KeyCode.LeftShift);
            _isSprinting = _canSprint && isShiftPressed && (_moveInput.y > 0.1f) && _isGrounded && (obstacleBlocked == false);

            if (_isGrounded == false && _wasGrounded)
            {
                _fallStartHeight = transform.position.y;
                _inFall = true;
            }

            float currentSpeed;

            if (_isCrouching)
            {
                currentSpeed = (isShiftPressed && _canSprint)
                    ? _crouchSpeed * _crouchedSprintSpeedMultiplier
                    : _crouchSpeed;
            }
            else if (_isSprinting)
            {
                currentSpeed = _sprintSpeed;
            }
            else
            {
                currentSpeed = _walkSpeed;
            }

            Vector3 direction = new(_moveInput.x, 0f, _moveInput.y);
            Vector3 moveVector = transform.TransformDirection(direction) * currentSpeed;
            moveVector = Vector3.ClampMagnitude(moveVector, currentSpeed);

            if (_isGrounded || _coyoteTimer > 0f)
            {
                if (_canJump && Input.GetKeyDown(KeyCode.Space))
                {
                    _moveDirection.y = _jumpSpeed;
                    _hasJumped = true;
                    if (_footstepController != null)
                        _footstepController.PlayJumpStartSound();
                }
                else if (_moveDirection.y < 0f)
                {
                    _moveDirection.y = MinYVelocity;
                }
            }
            else
            {
                _moveDirection.y -= _gravity * _fallMultiplier * Time.deltaTime;
            }

            _moveDirection.x = moveVector.x;
            _moveDirection.z = moveVector.z;
            _characterController.Move(_moveDirection * Time.deltaTime);

            // effectiveBlocked теперь зависит только от обнаружения препятствия
            bool effectiveBlocked = obstacleBlocked;

            if (_footstepController != null)
                _footstepController.UpdateFootsteps(_isGrounded, _moveInput, _isSprinting, _isCrouching, effectiveBlocked, Time.deltaTime);

            if (_wasGrounded == false && _isGrounded)
            {
                float fallDistance = _inFall ? (_fallStartHeight - transform.position.y) : 0f;
                if (_hasJumped || fallDistance >= _fallLandingThreshold)
                {
                    if (_footstepController != null)
                        _footstepController.PlayLandingSound();
                }
                _hasJumped = false;
                _inFall = false;
            }

            _wasGrounded = _isGrounded;
        }
        #endregion

        public void Disable()
        {
            this.enabled = false;
            _isSprinting = false;
            _moveInput = Vector2.zero;
        }

        #region Private Methods

        private bool IsObstacleBlocking()
        {
            if (_moveInput.sqrMagnitude < 0.01f)
                return false;

            Vector3 moveDir = transform.TransformDirection(new Vector3(_moveInput.x, 0f, _moveInput.y)).normalized;
            // Исправление: рассчитываем точку начала Raycast с учётом направления движения
            Vector3 origin = transform.position + moveDir * _characterController.radius + Vector3.up * (_characterController.height * 0.5f);

            if (Physics.Raycast(origin, moveDir, out RaycastHit hit, _obstacleCheckDistance))
            {
                float angle = Vector3.Angle(moveDir, -hit.normal);
                if (angle < _obstacleAngleThreshold && hit.distance < _obstacleCheckDistance)
                    return true;
            }

            return false;
        }
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            if (body != null && !body.isKinematic)
            {
                // Определяем горизонтальную составляющую силы столкновения:
                Vector3 pushDirection = hit.controller.velocity;
                pushDirection.y = 0f; // Убираем вертикальную составляющую
                body.AddForce(pushDirection * _pushForce, ForceMode.Impulse);
            }
        }


        #endregion
    }

}