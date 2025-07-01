using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class LevelIntroController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel; // The parent "Panel"
    public GameObject centerBG; // CenterBG (with fade out)
    public GameObject parentScoreBG; // ParentScoreBG (score)
    public TMP_Text scoreText; // The TMP_Text inside ParentScoreBG
    public TMP_Text textTargetTitle; // The "Reach this score" TMP_Text

    [Header("Config")]
    public Transform topScoreTarget; // Assign your top bar score position
    public float showDuration = 2.0f;
    public float fadeOutTime = 0.5f;
    public float moveTime = 0.8f;
    public float scalePunch = 1.2f;

    private Vector3 originalParentScoreBGPos;
    private Vector3 originalTextTargetPos;
    private Quaternion originalParentScoreBGRot;
    private Vector3 originalParentScoreBGScale;

    void Awake()
    {
        // Cache originals for resetting after animation
        originalParentScoreBGPos = parentScoreBG.transform.position;
        originalParentScoreBGRot = parentScoreBG.transform.rotation;
        originalParentScoreBGScale = parentScoreBG.transform.localScale;
        originalTextTargetPos = textTargetTitle.transform.position;

        GameObject mainImageObj = FindInactiveObjectByName("MainIMage");
if (mainImageObj != null) mainImageObj.SetActive(false);
    }

    /// <summary>
    /// Call this function on level start.
    /// </summary>
    public void PlayLevelIntro(int targetScore, System.Action onFinish = null)
    {
         GameObject mainImageObj = FindInactiveObjectByName("MainIMage");
if (mainImageObj != null) mainImageObj.SetActive(false);
        StartCoroutine(LevelIntroSequence(targetScore, onFinish));
    }

    private IEnumerator LevelIntroSequence(int targetScore, System.Action onFinish)
    {
        // Disable game logic during intro
    //    GameManager.instance.isGameActive = false;
        if (DartC.instance != null) DartC.instance.isGameActive = false;

        // Activate and reset everything
        panel.SetActive(true);
        centerBG.SetActive(true);
        parentScoreBG.SetActive(true);
        scoreText.text = targetScore.ToString();
        textTargetTitle.gameObject.SetActive(true);

        // Reset visuals (positions/scales)
        parentScoreBG.transform.position = originalParentScoreBGPos;
        parentScoreBG.transform.rotation = originalParentScoreBGRot;
        parentScoreBG.transform.localScale = originalParentScoreBGScale;
        textTargetTitle.transform.position = originalTextTargetPos;
        textTargetTitle.color = Color.white;

        // Wait at start for readability
        yield return new WaitForSeconds(showDuration);

        // 1. Animate TextTargetTitle ("Reach this score!") LERP LEFT + Fade Out
        Vector3 leftOffscreen = originalTextTargetPos + Vector3.left * 600f; // Adjust as needed
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.7f;
            textTargetTitle.transform.position = Vector3.Lerp(originalTextTargetPos, leftOffscreen, t);
            textTargetTitle.color = new Color(1, 1, 1, 1 - t);
            yield return null;
        }
        textTargetTitle.gameObject.SetActive(false);

        // 2. Animate ParentScoreBG: Move to top bar, scale punch
       // 2. Animate ParentScoreBG: Move to top bar, scale punch
Vector3 startPos = originalParentScoreBGPos;
Vector3 endPos = topScoreTarget.position;
Vector3 startScale = originalParentScoreBGScale;
Vector3 punchScale = originalParentScoreBGScale * scalePunch;

Image bgImage = centerBG.GetComponent<Image>();
Color bgOrigColor = bgImage.color;

float time = 0f;
while (time < 1f)
{
    time += Time.deltaTime / moveTime;
    float tMove = Mathf.Clamp01(time);
    parentScoreBG.transform.position = Vector3.Lerp(startPos, endPos, tMove);
    // Optional: scale punch logic here if needed

    // Simultaneous fade out for centerBG
    float fadeT = Mathf.Clamp01(time / fadeOutTime);
    bgImage.color = Color.Lerp(bgOrigColor, new Color(bgOrigColor.r, bgOrigColor.g, bgOrigColor.b, 0), fadeT);

    yield return null;
}
parentScoreBG.transform.position = endPos;
parentScoreBG.transform.localScale = originalParentScoreBGScale;
centerBG.SetActive(false);

// ===> Add this block here
parentScoreBG.SetActive(false);

// --- Enable MainIMage (find even if inactive) ---
GameObject mainImageObj = FindInactiveObjectByName("MainIMage");
if (mainImageObj != null) mainImageObj.SetActive(true);



        // Done!
        yield return new WaitForSeconds(0.1f);
        if (onFinish != null) onFinish.Invoke();

        // Enable gameplay!
     //   GameManager.instance.isGameActive = true;
        if (DartC.instance != null) DartC.instance.isGameActive = true;
        panel.SetActive(false);
    }

    // Optionally: Call this to force-reset for next level (or on skip)
    public void ResetIntro()
    {
        panel.SetActive(false);
        centerBG.SetActive(true);
        parentScoreBG.transform.position = originalParentScoreBGPos;
        parentScoreBG.transform.rotation = originalParentScoreBGRot;
        parentScoreBG.transform.localScale = originalParentScoreBGScale;
        textTargetTitle.transform.position = originalTextTargetPos;
        textTargetTitle.gameObject.SetActive(true);
    }

    // Utility to find even inactive GameObjects in the scene by name
GameObject FindInactiveObjectByName(string name)
{
    var all = Resources.FindObjectsOfTypeAll<Transform>();
    foreach (var t in all)
        if (t.hideFlags == HideFlags.None && t.gameObject.name == name)
            return t.gameObject;
    return null;
}

}
