using System;
using UnityEngine;

public class InputReader : MonoBehaviour
{
    public static InputReader Instance { get; private set; }

    private const KeyCode Flashlight = KeyCode.F;

    public event Action FlashlightPressed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        ReadFlashLight();
    }

    private void ReadFlashLight()
    {
        if (Input.GetKeyDown(Flashlight))
            FlashlightPressed?.Invoke();
    }
}