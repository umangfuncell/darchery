using UnityEngine;
using System.Collections;

public class DartboardRotator : MonoBehaviour
{
    public float defaultRotationSpeed = 30.0f; // Default rotation speed
    public bool rotateClockwise = true; // Initial direction of rotation for the default mode

    public float specialSpeed1 = 50.0f;
    public float specialDuration1 = 5.0f;
    public float specialSpeed2 = 70.0f;
    public float specialDuration2 = 3.0f;
    public bool isSpecialModeActive = false; // Toggle this in the Inspector to activate/deactivate special mode on start

    void Start()
    {
        if (isSpecialModeActive)
        {
            StartCoroutine(SpecialRotationSequence());
        }
    }

    void Update()
    {
        if (!isSpecialModeActive) // Normal rotation
        {
            RotateDartboard(defaultRotationSpeed, rotateClockwise);
        }
    }

    private void RotateDartboard(float speed, bool clockwise)
    {
        float rotationStep = speed * Time.deltaTime;
        rotationStep = clockwise ? rotationStep : -rotationStep;
        transform.Rotate(new Vector3(0, 0, rotationStep));
    }

    IEnumerator SpecialRotationSequence()
    {
        while (isSpecialModeActive) // Continues looping as long as special mode is active
        {
            // First rotation phase
            for (float timer = specialDuration1; timer > 0; timer -= Time.deltaTime)
            {
                RotateDartboard(specialSpeed1, true); // Clockwise rotation
                yield return null;
            }

            // Second rotation phase
            for (float timer = specialDuration2; timer > 0; timer -= Time.deltaTime)
            {
                RotateDartboard(specialSpeed2, false); // Counterclockwise rotation
                yield return null;
            }
        }
    }

    // This method allows enabling/disabling special mode dynamically
    public void ToggleSpecialMode(bool activate)
    {
        isSpecialModeActive = activate;
        if (isSpecialModeActive)
        {
            StartCoroutine(SpecialRotationSequence());
        }
        else
        {
            StopCoroutine(SpecialRotationSequence()); // Stop the special mode coroutine if it is running
        }
    }
}
