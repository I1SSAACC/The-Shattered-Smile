using UnityEngine;

namespace MiktoGames
{
    public class FootstepController : MonoBehaviour
    {
        [Header("Walking Footstep Settings")]
        [SerializeField, Tooltip("AudioSource для звуков ходьбы")]
        private AudioSource _walkingAudioSource;
        [SerializeField, Tooltip("Массив звуков шагов при ходьбе")]
        private AudioClip[] _walkingFootstepSounds;
        [SerializeField, Tooltip("Массив звуков шагов при ходьбе по дереву")]
        private AudioClip[] _woodWalkingFootstepSounds;
        [SerializeField, Tooltip("Интервал между шагами при ходьбе (сек)")]
        private float _walkingFootstepInterval = 0.5f;

        [Header("Running Footstep Settings")]
        [SerializeField, Tooltip("AudioSource для звуков бега")]
        private AudioSource _runningAudioSource;
        [SerializeField, Tooltip("Массив звуков шагов при беге")]
        private AudioClip[] _runningFootstepSounds;
        [SerializeField, Tooltip("Массив звуков шагов при беге по дереву")]
        private AudioClip[] _woodRunningFootstepSounds;
        [SerializeField, Tooltip("Интервал между шагами при беге (сек)")]
        private float _runningFootstepInterval = 0.3f;

        [Header("Jump & Landing Sounds")]
        [SerializeField, Tooltip("Звук старта прыжка")]
        private AudioClip _jumpStartSound;
        [SerializeField, Tooltip("Звук приземления")]
        private AudioClip _jumpLandSound;
        [SerializeField, Tooltip("AudioSource для прыжка и приземления")]
        private AudioSource _jumpAudioSource;

        [Header("Ground Material Check")]
        [SerializeField, Tooltip("Точка проверки материала под ногами")]
        private Transform _groundCheck;
        [SerializeField, Tooltip("Расстояние Raycast для проверки поверхности")]
        private float _materialCheckDistance = 1f;
        [SerializeField, Tooltip("Маска слоёв для проверки материала под ногами")]
        private LayerMask _groundMaterialMask;

        private float _footstepTimer;
        private bool _wasMoving;

        /// <summary>
        /// Выполняет Raycast вниз из _groundCheck и проверяет, имеет ли объект, на который он попадает, тег "Wood".
        /// </summary>
        private bool IsGroundWood()
        {
            Vector3 origin = _groundCheck.position;
            Vector3 direction = Vector3.down;

            // Для отладки: рисуем луч
            Debug.DrawRay(origin, direction * _materialCheckDistance, Color.red, 0.5f);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, _materialCheckDistance, _groundMaterialMask))
            {
                // Убедимся, что hit.collider существует и имеет нужный тег.
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

            // Если под ногами обнаружено дерево, переключаем массив звуков.
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