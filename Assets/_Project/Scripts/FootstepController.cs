using UnityEngine;

namespace MiktoGames
{
    public class FootstepController : MonoBehaviour
    {
        [SerializeField] private float _walkingFootstepInterval = 0.25f;        
        [SerializeField] private float _runningFootstepInterval = 0.15f;        
        [SerializeField] private float _materialCheckDistance = 1f;
        [SerializeField] private LayerMask _groundMaterialMask;

        private float _footstepTimer;
        private bool _wasMoving;

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
            FootstepType footstepType = 0;

            Debug.Log($"IsGroundWood: {IsGroundWood()}");

            if (IsGroundWood())
                if (isSprinting)
                    footstepType = isSprinting ? FootstepType.WoodRunning : FootstepType.WoodWalking;
            else
                if (isSprinting)
                    footstepType = isSprinting ? FootstepType.OrdinaryRunning : FootstepType.OrdinaryWalking;

            SfxPlayer3D.Instance.PlayFootstep(footstepType, transform);
        }

        public void PlayJumpStartSound() =>
            SfxPlayer3D.Instance.PlayJumpStartSound(transform);

        public void PlayJumpLandSound() =>
            SfxPlayer3D.Instance.PlayJumpLandSound(transform);
    }
}