using UnityEngine;

public class CoinFlipZone : MonoBehaviour
{
    public float negativeChance = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dart"))
        {
            int score = ScoreManager.instance.score;
            bool heads = Random.value > negativeChance;
            int finalScore = heads ? score : -score;
            ScoreManager.instance.AddScore(finalScore);
            Debug.Log("Coin Flip! Score: " + finalScore);
        }
    }
}
