using UnityEngine;

namespace MiktoGames
{
    public class HeadBobbing : MonoBehaviour
    {
        [Header("Bobbing Settings")]
        [SerializeField] private float _walkingBobbingSpeed = 14f;
        [SerializeField] private float _sprintBobbingMultiplier = 1.5f;  // ”силитель покачивани€ при спринте.
        [SerializeField] private float _bobbingAmount = 0.05f;
        [SerializeField] private float _crouchBobbingSpeed = 7f;
        [SerializeField] private float _crouchedSprintBobbingMultiplier = 1.2f;  // ”силитель при бегу в приседе.
        [SerializeField] private float _recoilReturnSpeed = 8f;
        [SerializeField] private float _lerpSpeed = 10f;
        [Tooltip("¬ысота камеры в приседе")]
        [SerializeField] private float _crouchCameraHeight = 0.5f;
        [SerializeField] private Transform _cameraParent;

        private float _bobTimer;
        private float _currentBobOffset;
        private float _currentCameraHeight;
        private Vector3 _recoil = Vector3.zero;
        private float _originalCameraParentHeight;

        private PlayerMovement _playerMovement;

        private void Awake()
        {
            _originalCameraParentHeight = _cameraParent.localPosition.y;
            _currentCameraHeight = _originalCameraParentHeight;
            _playerMovement = GetComponentInParent<PlayerMovement>();
        }

        private void Update()
        {
            // ¬ычисл€ем горизонтальную скорость из CharacterController.
            float horizontalSpeed = 0f;
            if (_playerMovement != null)
            {
                CharacterController cc = _playerMovement.GetComponent<CharacterController>();
                horizontalSpeed = new Vector3(cc.velocity.x, 0f, cc.velocity.z).magnitude;
            }

            bool isMoving = horizontalSpeed > 0.1f;
            float targetBobOffset = isMoving ? Mathf.Sin(_bobTimer) * _bobbingAmount : 0f;
            _currentBobOffset = Mathf.Lerp(_currentBobOffset, targetBobOffset, Time.deltaTime * _walkingBobbingSpeed);

            // ≈сли игрок приседает, используем камеру дл€ приседа, иначе исходную.
            float targetCameraHeight = (_playerMovement != null && _playerMovement.IsCrouching)
                ? _crouchCameraHeight
                : _originalCameraParentHeight;
            _currentCameraHeight = Mathf.Lerp(_currentCameraHeight, targetCameraHeight, Time.deltaTime * _lerpSpeed);
            _currentCameraHeight = Mathf.Clamp(_currentCameraHeight, _crouchCameraHeight, _originalCameraParentHeight);

            if (!isMoving)
            {
                _bobTimer = 0f;
                _currentBobOffset = 0f;
            }
            else
            {
                float speedMultiplier;
                // ≈сли игрок приседает Ц провер€ем состо€ние спринта через PlayerMovement
                if (_playerMovement != null && _playerMovement.IsCrouching)
                {
                    speedMultiplier = _playerMovement.IsSprinting ? _crouchBobbingSpeed * _crouchedSprintBobbingMultiplier : _crouchBobbingSpeed;
                }
                else
                {
                    speedMultiplier = (_playerMovement != null && _playerMovement.IsSprinting) ? _walkingBobbingSpeed * _sprintBobbingMultiplier : _walkingBobbingSpeed;
                }
                _bobTimer += Time.deltaTime * speedMultiplier;
            }

            Vector3 parentPos = _cameraParent.localPosition;
            _cameraParent.localPosition = new Vector3(parentPos.x, _currentCameraHeight + _currentBobOffset, parentPos.z);

            _recoil = Vector3.Lerp(_recoil, Vector3.zero, Time.deltaTime * _recoilReturnSpeed);
            _cameraParent.localRotation = Quaternion.Euler(_recoil);
        }
    }
}