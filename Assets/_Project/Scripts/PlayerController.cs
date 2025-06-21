using UnityEngine;

namespace MiktoGames
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private MouseLook _mouseLook;
        [SerializeField] private HeadBobbing _headBobbing;
        [SerializeField] private FootstepController _footstepController;

        private void Update()
        {
            if (_movement != null && _footstepController != null)
                _footstepController.UpdateFootsteps(_movement.IsGrounded, _movement.MoveInput, _movement.IsSprinting, _movement.IsCrouching, false, Time.deltaTime);
        }
    }
}