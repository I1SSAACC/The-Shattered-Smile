using UnityEngine;

namespace MiktoGames
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private MouseLook _mouseLook;
        [SerializeField] private HeadBobbing _headBobbing;
        [SerializeField] private FootstepController _footstepController;
        [SerializeField] private FlashlightController _flashlight;

        public Transform CameraTransform => _mouseLook.CameraTransform;

        private void Update()
        {
            if (_movement != null && _footstepController != null)
                _footstepController.UpdateFootsteps(_movement.IsGrounded, _movement.MoveInput, _movement.IsSprinting, _movement.IsCrouching, false, Time.deltaTime);
        }

        public void EnableControl()
        {
            EnableHeadBobbing();
            EnablePlayerMovement();
            EnableMouseLock();
        }

        public void DisableControl()
        {
            DisableHeadBobbing();
            DisablePlayerMovement();
            DisableMouseLock();
        }

        public void DisableFlashlight() =>
            _flashlight.DisableInteractable();

        public void EnableFlashlight() =>
            _flashlight.EnableInteractable();

        private void EnableHeadBobbing() =>
            _headBobbing.enabled = true;

        private void DisableHeadBobbing() =>
            _headBobbing.enabled = false;

        private void EnablePlayerMovement() =>
            _movement.enabled = true;

        private void DisablePlayerMovement() =>
            _movement.Disable();

        private void EnableMouseLock()
        {
            _mouseLook.SetCurrentRotaton();
            _mouseLook.enabled = true;
        }

        private void DisableMouseLock() =>
            _mouseLook.enabled = false;
    }
}