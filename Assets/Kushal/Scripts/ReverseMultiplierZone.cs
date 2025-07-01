using UnityEngine;

public class ReverseMultiplierZone : MonoBehaviour
{
    public int multiplier = 2;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dart"))
        {
            ScoreManager.instance.AddScore(-ScoreManager.instance.score * multiplier);
            Debug.Log("Reverse multiplier triggered!");
        }
    }
}
