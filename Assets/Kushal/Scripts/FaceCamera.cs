using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    void LateUpdate()
    {
        // Get full direction from camera to object
        Vector3 direction = transform.position - Camera.main.transform.position;

        // Get full rotation
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // Keep only Z rotation (lock X and Y)
        Vector3 euler = lookRotation.eulerAngles;
        euler.x = transform.rotation.eulerAngles.x; // Lock X
        euler.y = transform.rotation.eulerAngles.y; // Lock Y

        transform.rotation = Quaternion.Euler(euler);
    }
}
