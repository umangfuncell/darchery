using UnityEngine;
using TMPro;
using System.Collections;


public class Shatter : MonoBehaviour
{

    public ScoreArea scoreAreaRef; // Add this at the top

    [Header("Shatter Settings")]
    [Tooltip("List of objects that will be shattered (Rigidbody will be enabled) when hit requirement is met.")]
    public GameObject[] objectsToShatter;

    [Tooltip("Number of times this object needs to be hit to trigger shatter.")]
    public int requiredHits = 3;

    [Tooltip("Enable to display current hit count using a TextMeshPro UI element.")]
    public bool showTextUI = false;

    [Header("Material")]
public Material normalMaterial;
public Material hitMaterial;
public bool secondLogic = false;

    [Header("UI (Optional)")]
    [Tooltip("Text UI to display remaining hits (e.g., 'Hits Left: 1')")]
    public TextMeshPro hitText;

    [Header("Visual Feedback")]
    [Tooltip("Optional: Particle effect or animation to trigger when shatter occurs.")]
    public GameObject shatterEffect;




    private int currentHits = 0;
    private bool alreadyShattered = false;

    private int originalScoreValue = -1;
public ScoreArea[] extraScoreAreas; // Optional extra areas

private int[] originalExtraScores; // Internal cache


   void Start()
{
    if (showTextUI && hitText != null)
    {
        hitText.text = $" {requiredHits - currentHits}";
    }


if (scoreAreaRef != null)
    {
       scoreAreaRef.isWood = requiredHits > 0;
    }
    // Apply default material only if secondLogic is true
    if (secondLogic)
    {
        ApplyMaterialToAll(normalMaterial);
    }
    else
    {
        UpdateShatterMaterials(requiredHits);
    }

    if (scoreAreaRef != null)
{
    // Store the original scoreValue for later restoration
    originalScoreValue = scoreAreaRef.scoreValue;
    scoreAreaRef.isWood = requiredHits > 0;
    // Set score to 0 if wood mode
    if (scoreAreaRef.isWood)
        scoreAreaRef.scoreValue = 0;
}

 if (extraScoreAreas != null && extraScoreAreas.Length > 0)
    {
        originalExtraScores = new int[extraScoreAreas.Length];
        for (int i = 0; i < extraScoreAreas.Length; i++)
        {
            if (extraScoreAreas[i] != null)
            {
                originalExtraScores[i] = extraScoreAreas[i].scoreValue;
                extraScoreAreas[i].isWood = requiredHits > 0;
                if (extraScoreAreas[i].isWood)
                    extraScoreAreas[i].scoreValue = 0;
            }
        }
    }
}


  private void OnCollisionEnter(Collision collision)
{
    if (alreadyShattered) return;

    if (collision.gameObject.CompareTag("Dart"))
    {
        currentHits++;

        int hitsLeft = Mathf.Max(0, requiredHits - currentHits);

        if (showTextUI && hitText != null)
        {
            hitText.text = hitsLeft > 0 ? $"  {hitsLeft}" : " ";
        }

        if (!secondLogic)
        {
            UpdateShatterMaterials(hitsLeft); // Only update during hits if secondLogic is false
        }

        if (currentHits >= requiredHits)
        {
            StartCoroutine(shattering());
            
        }
    }
}


private void UpdateShatterMaterials(int hitsLeft)
{
    Material targetMat = hitsLeft == 1 ? hitMaterial : normalMaterial;

    foreach (GameObject obj in objectsToShatter)
    {
        if (obj != null)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null && targetMat != null)
            {
                renderer.material = targetMat;
            }
        }
    }
}

IEnumerator shattering()
{
    yield return new WaitForSeconds(0.3f);
    TriggerShatter();

}
  private void TriggerShatter()
{

    alreadyShattered = true;

    if (secondLogic)
    {
        ApplyMaterialToAll(hitMaterial);
    }

    foreach (GameObject obj in objectsToShatter)
    {
        if (obj != null)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            else
            {
                rb = obj.AddComponent<Rigidbody>();
            }

rb.AddExplosionForce(
    1f,                  // Super small force
    rb.transform.position + Random.insideUnitSphere * 0.2f, // Randomize explosion center just a bit for variety
    2f,                  // Reasonable radius
    0.05f,               // Very little upward force
    ForceMode.Impulse
);

        }
    }
         // Restore main
    if (scoreAreaRef != null)
    {
        scoreAreaRef.isWood = false;
        if (originalScoreValue != -1)
            scoreAreaRef.scoreValue = originalScoreValue;
    }
    // Restore all extra
    if (extraScoreAreas != null && originalExtraScores != null)
    {
        for (int i = 0; i < extraScoreAreas.Length; i++)
        {
            if (extraScoreAreas[i] != null)
            {
                extraScoreAreas[i].isWood = false;
                extraScoreAreas[i].scoreValue = originalExtraScores[i];
            }
        }
    }

    if (shatterEffect != null)
    {
       // Instantiate(shatterEffect, transform.position, Quaternion.identity);
       shatterEffect.SetActive(false);
    }
}
private void ApplyMaterialToAll(Material mat)
{
    foreach (GameObject obj in objectsToShatter)
    {
        if (obj != null)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null && mat != null)
            {
                renderer.material = mat;
            }
        }
    }
}

}
