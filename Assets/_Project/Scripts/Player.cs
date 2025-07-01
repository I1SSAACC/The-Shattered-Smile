using UnityEngine;

namespace MiktoGames
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private MouseLook _mouseLook;
        [SerializeField] private HeadBobbing _headBobbing;
        [SerializeField] private FootstepController _footstepController;
        [SerializeField] private Flashlight _flashlight;

        private bool _isHidden;

        public Transform CameraTransform => _mouseLook.CameraTransform;

        public bool IsHidden => _isHidden;

        private void OnEnable()
        {
            InputReader.Instance.FlashlightPressed += OnFlashLightPressed;
        }

        private void OnDisable()
        {
            InputReader.Instance.FlashlightPressed -= OnFlashLightPressed;
        }

        public void EnableControl()
        {
            EnableHeadBobbing();
            EnablePlayerMovement();
            EnableMouseLook();
        }

        public void DisableControl()
        {
            DisableHeadBobbing();
            DisablePlayerMovement();
            DisableMouseLook();
        }

        public void SetTempParamsMouseLook(Vector2 verticalLimis, Vector2 horizontalLimits, float multiplierSensitivity) =>
            _mouseLook.SetTempParams(verticalLimis, horizontalLimits, multiplierSensitivity);

        public void SetDefaultMouseLook() =>
             _mouseLook.SetDefaultParams();

        public void DisableFlashlight() =>
            _flashlight.DisableInteractable();

        public void EnableFlashlight() =>
            _flashlight.EnableInteractable();

        public void SetHiddenStatus(bool isHidden) =>
            _isHidden = isHidden;

        private void EnableHeadBobbing() =>
            _headBobbing.enabled = true;

        private void DisableHeadBobbing() =>
            _headBobbing.enabled = false;

        private void EnablePlayerMovement() =>
            _movement.enabled = true;

        private void DisablePlayerMovement() =>
            _movement.Disable();

        public void EnableMouseLook() =>
            _mouseLook.enabled = true;

        public void DisableMouseLook() =>
            _mouseLook.enabled = false;

        private void OnFlashLightPressed() =>
            _flashlight.ToggleFlashlight();
    }
}