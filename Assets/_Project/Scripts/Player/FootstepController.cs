using UnityEngine;

namespace MiktoGames
{
    public class FootstepController : MonoBehaviour
    {
        [SerializeField] private float _walkingFootstepInterval = 0.5f;
        [SerializeField] private float _runningFootstepInterval = 0.3f;
        [SerializeField] private float _materialCheckDistance = 0.2f;

        private float _footstepTimer;

        private bool IsGroundWood()
        {
            Vector3 origin = transform.position;
            Vector3 direction = Vector3.down;

            Debug.DrawRay(origin, direction * _materialCheckDistance, Color.red, 0.5f);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, _materialCheckDistance))
                if (hit.collider != null)
                    return hit.collider.TryGetComponent(out Wood _);

            return false;
        }

        public void UpdateFootsteps(bool isGrounded, Vector2 moveInput, bool isSprinting, bool isCrouching, bool effectiveBlocked, float deltaTime)
        {
            bool isMoving = moveInput.sqrMagnitude > 0.01f;

            if (isMoving == false || effectiveBlocked || isCrouching || isGrounded == false)
            {
                ResetFootStepTimer();
                return;
            }

            _footstepTimer += deltaTime;
            float interval = isSprinting ? _runningFootstepInterval : _walkingFootstepInterval;

            if (_footstepTimer >= interval)
            {
                ResetFootStepTimer();
                PlayStepSound(isSprinting);
            }
        }

        private void ResetFootStepTimer() =>
            _footstepTimer = 0f;

        private void PlayStepSound(bool isSprinting)
        {
            FootstepType footstepType;

            if (IsGroundWood())
                footstepType = isSprinting ? FootstepType.WoodRunning : FootstepType.WoodWalking;
            else
                footstepType = isSprinting ? FootstepType.OrdinaryRunning : FootstepType.OrdinaryWalking;

            SfxPlayer3D.Instance.PlayFootstep(footstepType, transform);
        }

        public void PlayJumpStartSound() =>
            SfxPlayer3D.Instance.PlayJumpStartSound(transform);

        public void PlayJumpLandSound() =>
            SfxPlayer3D.Instance.PlayJumpLandSound(transform);
    }
}