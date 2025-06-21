using UnityEngine;
using UnityEngine.UI;
using MiktoGames;

public class PauseController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Slider mouseSensitivitySlider;

    [Header("Dependencies")]
    [SerializeField] private MouseLook mouseLook;

    private bool _isPaused = false;

    private void Start()
    {
        QualitySettings.vSyncCount = 1;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ������������� ������ �����
        pausePanel.SetActive(false);

        // �������������� ��������� �������� �� 0 �� 20
        mouseSensitivitySlider.minValue = 0f;
        mouseSensitivitySlider.maxValue = 20f;

        if (mouseLook != null)
        {
            mouseSensitivitySlider.value = mouseLook.MouseSensitivity;
        }

        // �������� �� ��������� �������� ��������
        mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
    }

    private void Update()
    {
        // �� ������� ������� Esc ����������� �����
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        _isPaused = !_isPaused;
        pausePanel.SetActive(_isPaused);
        Time.timeScale = _isPaused ? 0f : 1f;

        if (_isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnMouseSensitivityChanged(float value)
    {
        if (mouseLook != null)
        {
            mouseLook.SetMouseSensitivity(value);
        }
    }
}