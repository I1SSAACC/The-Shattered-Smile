using UnityEngine;

namespace MiktoGames
{
    public class MouseLook : MonoBehaviour
    {
        [Header("Mouse Look Settings")]
        [SerializeField] private float _mouseSensitivity = 25f;
        [SerializeField] private Vector2 _lookYLimits = new(-90f, 90f);
        [SerializeField] private Transform _playerCamera;

        private readonly float _snappiness = 100f;
        private float _rotX;
        private float _rotY;
        private float _xVelocity;
        private float _yVelocity;
        private float _currentTiltAngle;
        private float _tiltVelocity;

        public float MouseSensitivity => _mouseSensitivity;

        public Transform CameraTransform => _playerCamera;

        public void SetMouseSensitivity(float value)
        {
            _mouseSensitivity = value;
        }

        public void SetCurrentRotaton()
        {
            _rotX = transform.eulerAngles.y;
            _rotY = _playerCamera.localEulerAngles.x;

            if (_rotY > 180f) 
                _rotY -= 360f;

            _xVelocity = _rotX;
            _yVelocity = _rotY;
        }

        private void Update()
        {
            float mouseX = 10f * _mouseSensitivity * Time.deltaTime * Input.GetAxis("Mouse X");
            float mouseY = 10f * _mouseSensitivity * Time.deltaTime * Input.GetAxis("Mouse Y");

            _rotX += mouseX;
            _rotY -= mouseY;
            _rotY = Mathf.Clamp(_rotY, _lookYLimits.x, _lookYLimits.y);

            _xVelocity = Mathf.Lerp(_xVelocity, _rotX, _snappiness * Time.deltaTime);
            _yVelocity = Mathf.Lerp(_yVelocity, _rotY, _snappiness * Time.deltaTime);

            _currentTiltAngle = Mathf.SmoothDamp(_currentTiltAngle, 0f, ref _tiltVelocity, 0.2f);

            if (_playerCamera != null)
                _playerCamera.localRotation = Quaternion.Euler(_yVelocity - _currentTiltAngle, 0f, 0f);

            transform.rotation = Quaternion.Euler(0f, _xVelocity, 0f);
        }
    }
}