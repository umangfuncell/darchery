using UnityEngine;

public class ShrinkingBullseye : MonoBehaviour
{
    public float shrinkRate = 0.1f; // per second
    public float minScale = 0.2f;

    void Update()
    {
        if (transform.localScale.x > minScale)
        {
            transform.localScale -= Vector3.one * shrinkRate * Time.deltaTime;
        }
    }
}
