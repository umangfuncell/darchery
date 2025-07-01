using UnityEngine;

public class RotateBoard : MonoBehaviour
{
    private float rotationTime = 60f;   // seconds for 1 full spin
    public bool pause = false;
    private float angle = 0f;

    void Update()
    {
        if (pause || rotationTime <= 0f) return;
        float degreesPerSecond = 360f / rotationTime;
        angle += degreesPerSecond * Time.deltaTime;
        if (angle > 360f) angle -= 360f;

        // X rotates, Y and Z are fixed
        transform.localEulerAngles = new Vector3(angle, 90f, -90f);
    }
}
