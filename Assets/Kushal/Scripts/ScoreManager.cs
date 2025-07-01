using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI Reference")]
    public TMP_Text scoreText;

    [Header("Floating Text (Reusable)")]
    public TMP_Text floatingText; // ✅ Drag your TextMeshPro object here in Inspector
    public float floatDuration = 1.2f;
    public Vector3 floatOffset = new Vector3(0, 0.5f, 0); // How much it floats upward

    private Coroutine floatingTextRoutine;
    public int score;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void AddScore(int points)
    {
        GameManager.instance.AddScore(points);

        // ✅ Show floating text
        ShowFloatingText("+" + points.ToString());
    }

    private void ShowFloatingText(string text)
    {
        if (floatingText == null) return;

        // If already running, stop previous fade
        if (floatingTextRoutine != null)
        {
            StopCoroutine(floatingTextRoutine);
        }

        // Set text and enable
        floatingText.text = text;
        floatingText.color = new Color(floatingText.color.r, floatingText.color.g, floatingText.color.b, 1f); // Full alpha
        floatingText.gameObject.SetActive(true);
        floatingText.transform.localPosition = Vector3.zero; // Reset position (adjust if needed)

        // Start new fade out routine
        floatingTextRoutine = StartCoroutine(FadeAndDisable(floatingText));
    }

    private IEnumerator FadeAndDisable(TMP_Text tmp)
    {
        float elapsed = 0f;
        Color originalColor = tmp.color;
        Vector3 startPos = tmp.transform.localPosition;
        Vector3 endPos = startPos + floatOffset;

        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / floatDuration;

            tmp.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1 - t); // Fade out
            tmp.transform.localPosition = Vector3.Lerp(startPos, endPos, t); // Move up
            yield return null;
        }

        tmp.gameObject.SetActive(false);
    }
}
