using UnityEngine;

namespace MiktoGames
{
    public class FootstepController : MonoBehaviour
    {
        [Header("Walking Footstep Settings")]
        [SerializeField, Tooltip("AudioSource ��� ������ ������")]
        private AudioSource _walkingAudioSource;
        [SerializeField, Tooltip("������ ������ ����� ��� ������")]
        private AudioClip[] _walkingFootstepSounds;
        [SerializeField, Tooltip("������ ������ ����� ��� ������ �� ������")]
        private AudioClip[] _woodWalkingFootstepSounds;
        [SerializeField, Tooltip("�������� ����� ������ ��� ������ (���)")]
        private float _walkingFootstepInterval = 0.5f;

        [Header("Running Footstep Settings")]
        [SerializeField, Tooltip("AudioSource ��� ������ ����")]
        private AudioSource _runningAudioSource;
        [SerializeField, Tooltip("������ ������ ����� ��� ����")]
        private AudioClip[] _runningFootstepSounds;
        [SerializeField, Tooltip("������ ������ ����� ��� ���� �� ������")]
        private AudioClip[] _woodRunningFootstepSounds;
        [SerializeField, Tooltip("�������� ����� ������ ��� ���� (���)")]
        private float _runningFootstepInterval = 0.3f;

        [Header("Jump & Landing Sounds")]
        [SerializeField, Tooltip("���� ������ ������")]
        private AudioClip _jumpStartSound;
        [SerializeField, Tooltip("���� �����������")]
        private AudioClip _jumpLandSound;
        [SerializeField, Tooltip("AudioSource ��� ������ � �����������")]
        private AudioSource _jumpAudioSource;

        [Header("Ground Material Check")]
        [SerializeField, Tooltip("����� �������� ��������� ��� ������")]
        private Transform _groundCheck;
        [SerializeField, Tooltip("���������� Raycast ��� �������� �����������")]
        private float _materialCheckDistance = 1f;
        [SerializeField, Tooltip("����� ���� ��� �������� ��������� ��� ������")]
        private LayerMask _groundMaterialMask;

        private float _footstepTimer;
        private bool _wasMoving;

        /// <summary>
        /// ��������� Raycast ���� �� _groundCheck � ���������, ����� �� ������, �� ������� �� ��������, ��� "Wood".
        /// </summary>
        private bool IsGroundWood()
        {
            Vector3 origin = _groundCheck.position;
            Vector3 direction = Vector3.down;

            // ��� �������: ������ ���
            Debug.DrawRay(origin, direction * _materialCheckDistance, Color.red, 0.5f);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, _materialCheckDistance, _groundMaterialMask))
            {
                // ��������, ��� hit.collider ���������� � ����� ������ ���.
                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("Wood"))
                        return true;
                }
            }
            return false;
        }

        public void UpdateFootsteps(bool isGrounded, Vector2 moveInput, bool isSprinting, bool isCrouching, bool effectiveBlocked, float deltaTime)
        {
            bool isMoving = moveInput.sqrMagnitude > 0.01f;

            if (!isMoving)
            {
                _footstepTimer = 0f;
                _wasMoving = false;
                return;
            }

            if (effectiveBlocked || isCrouching || !isGrounded)
            {
                _footstepTimer = 0f;
                return;
            }

            if (isMoving && !_wasMoving)
            {
                PlayStepSound(isSprinting);
                _footstepTimer = 0f;
                _wasMoving = true;
                return;
            }

            _footstepTimer += deltaTime;
            float interval = isSprinting ? _runningFootstepInterval : _walkingFootstepInterval;
            if (_footstepTimer >= interval)
            {
                PlayStepSound(isSprinting);
                _footstepTimer = 0f;
            }
        }

        private void PlayStepSound(bool isSprinting)
        {
            AudioSource currentSource = isSprinting ? _runningAudioSource : _walkingAudioSource;
            AudioClip[] clips = isSprinting ? _runningFootstepSounds : _walkingFootstepSounds;

            // ���� ��� ������ ���������� ������, ����������� ������ ������.
            if (IsGroundWood())
            {
                clips = isSprinting ? _woodRunningFootstepSounds : _woodWalkingFootstepSounds;
            }

            if (currentSource != null && clips != null && clips.Length > 0)
            {
                currentSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
            }
        }

        public void PlayJumpStartSound()
        {
            if (_jumpAudioSource != null && _jumpStartSound != null)
                _jumpAudioSource.PlayOneShot(_jumpStartSound);
        }

        public void PlayLandingSound()
        {
            if (_jumpAudioSource != null && _jumpLandSound != null)
                _jumpAudioSource.PlayOneShot(_jumpLandSound);
        }
    }
}