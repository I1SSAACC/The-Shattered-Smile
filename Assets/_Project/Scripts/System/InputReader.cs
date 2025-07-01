using System;
using UnityEngine;

public class InputReader : MonoBehaviour
{
    private const KeyCode Flashlight = KeyCode.F;

    public event Action FlashlightPressed;

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