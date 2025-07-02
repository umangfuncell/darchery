
using UnityEngine;
using System;
using System.Collections;
using TMPro;
using Lofelt.NiceVibrations;
using System.Collections.Generic; // <-- THIS LINE


public class ScoreArea : MonoBehaviour
{
    public int scoreValue = 10; // Customize per area (e.g., 10, 20, 50, etc.)
    private bool requireCamera = false;
    public bool destroyer = false;
    public bool isWood = false;

    public bool dividingbool = false;   // Set this in Inspector per mesh
    private bool dividedOnce = false;   // Internal flag to track if division has already happened


    public Canvas mainCanvas; // Assign this in the inspector (drag your GameCanvas here)
    public Transform scoreDummyTarget;

    public TMP_Text dividingTMP; // Assign in Inspector or dynamically in code




    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        requireCamera = false;

        if (mainCanvas == null)
        {
            GameObject canvasObj = GameObject.Find("GameCanvasMain");
            if (canvasObj != null)
                mainCanvas = canvasObj.GetComponent<Canvas>();
            else
                Debug.LogWarning("GameCanvasMain not found!");
        }

        if (scoreDummyTarget == null)
        {
            GameObject targetObj = GameObject.Find("ScoreDummyPosition");
            if (targetObj != null)
            {
                scoreDummyTarget = targetObj.transform;
            }
            else
            {
                Debug.LogWarning("ScoreDummyPosition object not found in scene!");
            }
        }
    }
    //private static HashSet<GameObject> processedDarts = new HashSet<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        Debug.LogError("Safe Area: " + this.gameObject + " Dart: " + other.gameObject.name);
        StartCoroutine(HandleDartScoringAfterPhysics(other));


    }
    // private void OnCollisionEnter(Collision collision)
    // {
    //     if (!collision.gameObject.CompareTag("Dart")) return;
    //     Debug.LogError("Safe Area: " + this.gameObject + " Dart: " + collision.gameObject.name);

    //     // StartCoroutine(HandleDartScoringAfterPhysics(collision));
    // }

    //public void OnDartHit(GameObject dart, Collision collision)
    //{
    //  StartCoroutine(HandleDartScoringAfterPhysics(collision));
    //}

    public void OnDartScored(GameObject dart)
    {
        // You can copy all your scoring logic from HandleDartScoringAfterPhysics here,
        // or simply call HandleDartScoringAfterPhysics using a dummy Collision if you want.
        //  StartCoroutine(HandleDartScoringAfterPhysics(dart));
    }
    // -- NEW: All scoring logic is here, after physics finished --
    private IEnumerator HandleDartScoringAfterPhysics(Collider collision)
    {
        yield return new WaitForEndOfFrame(); // Ensures all physics/collision calls finish
        yield return null;
        // Only process if THIS collider is the nearest ScoreArea to the dart tip



        GameObject dartG = collision.gameObject;
        if (!collision.gameObject.CompareTag("Dart")) yield break;

        collision.enabled = false;
        // Collider dartCol = dartG.GetComponent<Collider>();
        // if (dartCol != null) dartCol.enabled = false;


        // Debug.LogError("Safe Area: " + this.gameObject + " Dart: " + dartG);
        var scoringStatus = dartG.GetComponent<Dart>();
        if (scoringStatus != null && scoringStatus.hasScored) yield break;
        if (scoringStatus != null) scoringStatus.hasScored = true;

        Transform dart = dartG.transform;
        dart.SetParent(this.transform, true); // 'true' keeps world position
        dart.localScale = new Vector3(3f, 3f, 2.5f);

        // Enable and scale last child of dart, with satisfying lerp
        if (dart.childCount > 0)
        {
            StartCoroutine(ScaleDartChild(dart));
        }

        //  GameObject dartG = collision.gameObject;

        // Defensive: Don't process if dart already destroyed
        if (dartG == null) yield break;

        // ----- Destroyer Logic -----
        if (destroyer)
        {
            GameObject.Find("AudioManager")?.GetComponent<AudioManager>()?.PlayHit();
            Time.timeScale = 1f;
            GameManager.instance.DeductMove();
            Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.HeavyImpact);
            StartCoroutine(DelayedResetAfterThrowD());
            DartC.instance.activeCrossIcon.SetActive(false);
            Destroy(dartG);
            yield break;
        }

        // ----- Wood Logic -----
        if (isWood)
        {
            GameObject.Find("AudioManager")?.GetComponent<AudioManager>()?.PlayHit();
            Time.timeScale = 1f;
            GameManager.instance.DeductMove();
            Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.HeavyImpact);
            StartCoroutine(DelayedResetAfterThrow());
            DartC.instance.activeCrossIcon.SetActive(false);

            Rigidbody dartRbb = dartG.GetComponent<Rigidbody>();
            if (dartRbb != null)
            {
                dartRbb.isKinematic = true;
                dartRbb.velocity = Vector3.zero;
                dartRbb.angularVelocity = Vector3.zero;
            }
            yield break;
        }

        // ----- Normal scoring -----
        GameObject.Find("AudioManager")?.GetComponent<AudioManager>()?.PlayHit();
        Time.timeScale = 1f;
        GameManager.instance.DeductMove();
        Lofelt.NiceVibrations.HapticPatterns.PlayPreset(Lofelt.NiceVibrations.HapticPatterns.PresetType.HeavyImpact);

        // Snap/parent/move only after physics
        //  dart.SetParent(collision);
        // dart.localScale = new Vector3(0.6f, 0.6f, 0.5f);

        // Ensure Rigidbody is stopped after all collisions
        Rigidbody dartRb = dartG.GetComponent<Rigidbody>();
        if (dartRb != null)
        {
            dartRb.isKinematic = true;
            dartRb.velocity = Vector3.zero;
            dartRb.angularVelocity = Vector3.zero;
        }

        // Visuals for your dart
        if (dart.childCount > 0)
        {
            dart.GetChild(0).gameObject.SetActive(false);
            if (dart.childCount > 1) dart.GetChild(1).gameObject.SetActive(true);
            if (dart.childCount > 2) dart.GetChild(2).gameObject.SetActive(true);
            StartCoroutine(DisableCam(dart));
        }

        if (DartC.instance != null)
        {
            DartC.instance.StopDartFollowing();
            DartC.instance.currentDart = null;
            DartC.instance.activeCrossIcon.SetActive(false);
        }
        StartCoroutine(DelayedResetAfterThrow());

        int finalScore = scoreValue;
        if (dividingbool && !dividedOnce)
        {
            finalScore = Mathf.CeilToInt(scoreValue / 2f);
            dividedOnce = true;
            scoreValue = finalScore;
            if (dividingTMP != null)
                dividingTMP.text = finalScore.ToString();
        }

        if (!destroyer)
        {
            Transform scoreTextTr = dart.Find("DartScoreText");
            if (scoreTextTr != null && scoreDummyTarget != null)
            {
                TMP_Text scoreText = scoreTextTr.GetComponent<TMP_Text>();
                scoreText.text = "" + finalScore;

                int current = GameManager.instance.currentScore;
                int target = GameManager.instance.targetScore;
                int newTotal = current + finalScore;

                bool isTooMuch = newTotal > target;
                scoreText.color = isTooMuch ? Color.red : Color.green;

                if (isTooMuch)
                {
                    GameManager.instance.HandleFloatingFeedback(finalScore, true);
                }
                else
                {
                    ScoreManager.instance.AddScore(finalScore);
                    GameManager.instance.AnimateCurrentScoreUp(
                    GameManager.instance.currentScore - finalScore,
                    GameManager.instance.currentScore
                );
                }

                scoreTextTr.gameObject.SetActive(true);



                StartCoroutine(MoveTextToTargetRoutine(
                    scoreTextTr,
                    scoreDummyTarget.position,
                    null
                ));
            }
        }
    }


    // public static ScoreArea GetScoreAreaAtPosition(Vector3 position, float radius = 0.05f)
    // {
    //     Collider[] hits = Physics.OverlapSphere(position, radius);
    //     ScoreArea found = null;
    //     float bestDist = float.MaxValue;
    //     foreach (Collider col in hits)
    //     {
    //         ScoreArea sa = col.GetComponent<ScoreArea>();
    //         if (sa != null)
    //         {
    //             float dist = Vector3.Distance(position, col.ClosestPoint(position));
    //             if (dist < bestDist)
    //             {
    //                 found = sa;
    //                 bestDist = dist;
    //             }
    //         }
    //     }
    //     return found;
    // }

    // public static Vector2 WorldToCanvasPosition(Canvas canvas, Vector3 worldPosition)
    // {
    //     Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);

    //     RectTransform canvasRect = canvas.transform as RectTransform;
    //     Vector2 anchoredPos;
    //     RectTransformUtility.ScreenPointToLocalPointInRectangle(
    //         canvasRect,
    //         screenPoint,
    //         canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
    //         out anchoredPos
    //     );
    //     return anchoredPos;
    // }

    IEnumerator DisableCam(Transform dartTransform)
    {
        yield return new WaitForSeconds(1f);

        //  Ensure dart still exists before trying to access children
        if (dartTransform != null && dartTransform.childCount > 0)
        {
            dartTransform.GetChild(0).gameObject.SetActive(false);
        }
    }

    IEnumerator DelayedResetAfterThrow()
    {
        yield return new WaitForSeconds(0.5f);  // 1 second delay (uses Time.timeScale, so use WaitForSecondsRealtime if you want unscaled)
        DartC.instance.StartCoroutine(DartC.instance.ResetAfterThrow());

        yield return new WaitForSeconds(1.5f);  // 1 second delay (uses Time.timeScale, so use WaitForSecondsRealtime if you want unscaled)
        GameManager.instance.CheckGameStatus();
    }

    IEnumerator DelayedResetAfterThrowD()
    {
        yield return new WaitForSeconds(0f);  // 1 second delay (uses Time.timeScale, so use WaitForSecondsRealtime if you want unscaled)
        DartC.instance.StartCoroutine(DartC.instance.ResetAfterThrow());

        yield return new WaitForSeconds(1.5f);  // 1 second delay (uses Time.timeScale, so use WaitForSecondsRealtime if you want unscaled)
        GameManager.instance.CheckGameStatus();
    }

    IEnumerator MoveTextToTargetRoutine(Transform textTr, Vector3 worldTarget, Action onComplete = null)
    {
        float duration = 2.5f;
        float fadeDuration = 0.5f; // Fade out in the last 0.3 seconds
        float elapsed = 0f;
        Vector3 start = textTr.position;
        TMP_Text tmp = textTr.GetComponent<TMP_Text>();
        Color startColor = tmp.color;
        float startFontSize = 120f;
        float endFontSize = 220f;

        tmp.fontSize = startFontSize; // Set starting font size

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            textTr.position = Vector3.Lerp(start, worldTarget, t);
            // Animate font size from 120 to 160
            tmp.fontSize = Mathf.Lerp(startFontSize, endFontSize, t);

            // Start fading out only in the last fadeDuration
            Color c = tmp.color;
            if (duration - elapsed <= fadeDuration)
            {
                float fadeT = Mathf.InverseLerp(duration, duration - fadeDuration, elapsed);
                c.a = Mathf.Lerp(0, 1, fadeT);
            }
            else
            {
                c.a = 1f;
            }
            tmp.color = c;

            yield return null;
        }
        textTr.gameObject.SetActive(false);
        tmp.color = startColor; // Reset alpha

        onComplete?.Invoke();
    }


    IEnumerator ScaleDartChild(Transform dart)
    {
        if (dart.childCount == 0) yield break;

        Transform lastChild = dart.GetChild(dart.childCount - 1);
        lastChild.gameObject.SetActive(true);

        Vector3 targetScale = lastChild.localScale * -1f;  // Scale down and invert direction
        Vector3 startScale = Vector3.zero;

        float duration = 1f;
        float elapsed = 0f;

        lastChild.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            lastChild.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        lastChild.localScale = targetScale;
    }



}