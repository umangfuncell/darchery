using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using GameAnalyticsSDK;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game Settings")]
    public int targetScore; // Score required to win
    public int totalMoves;    // Number of moves allowed
    private int currentMoves = 0; // Moves used
    public int currentScore; // Player's current score
        public int newCurrent;    // Number of moves allowed
            public int oldCurrent;    // Number of moves allowed



    [Header("UI References")]
    private TMP_Text scoreText;
    private TMP_Text movesText;
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;

    [Header("Floating Feedback")]
    private Image floatingImage;
    private Vector3 originalPosition;
    private Coroutine floatingRoutine;

    public TextMeshProUGUI floatingScoreText;
private Vector2 floatStartPos;
public RectTransform scoreTextRect; // assign via Inspector (drag ScoreGlobal here)
public RectTransform scoreTextNewRect; // assign via Inspector (drag ScoreGlobal here)


    [Header("Sprites")]
    public Sprite spriteOhNo;
    public Sprite spriteGood;
    public Sprite spriteBrilliant;
    public Sprite spriteImpressive;
    public Sprite spriteTooMuch; // Drag your "TooMuch" sprite here in the inspector!


private Coroutine scoreAnimateRoutine;
private int displayedScore = 0;
private Coroutine scorePopRoutine;

// In GameManager.cs
public float windEffectFrequencyX = 0.5f; // You can tweak in Inspector
private int displayedRemaining = 0; // Track currently shown value

public bool isTutorial = false; // Set to true for level 1 in the inspector or from your level loader

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
{

     GameObject floatObj = GameObject.Find("FloatingScoreText");
    if (floatObj != null)
    {
        floatingScoreText = floatObj.GetComponent<TextMeshProUGUI>();
        floatStartPos = floatingScoreText.rectTransform.anchoredPosition;
    }

    if (scoreText == null)
    {
        GameObject scoreObj = FindInactiveObject("ScoreGlobal");
        if (scoreObj != null)
            scoreText = scoreObj.GetComponent<TMP_Text>();
            scoreTextNewRect = scoreObj.GetComponent<RectTransform>();
    }

    if (movesText == null)
    {
        GameObject movesObj = GameObject.Find("MovesGlobal");
        if (movesObj != null)
            movesText = movesObj.GetComponent<TMP_Text>();
    }

if (gameOverPanel == null)
{
    gameOverPanel = FindInactiveObject("GameOverMain");
}


if (gameWinPanel == null)
{
    gameWinPanel = FindInactiveObject("GameWinMain");
}


if (scoreTextRect == null)
    {
        GameObject scoreTextObj = FindInactiveObject("ScoreGloball");
        if (scoreTextObj != null)
        {
            scoreTextRect = scoreTextObj.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogWarning("Could not find ScoreGlobal!");
        }
    }

 // Find the floating image by name
        floatingImage = GameObject.Find("FloatingFeedbackText")?.GetComponent<Image>();
        if (floatingImage != null)
        {
            originalPosition = floatingImage.rectTransform.anchoredPosition;
            floatingImage.color = new Color(1, 1, 1, 0); // invisible initially
        }
             int remaining = Mathf.Max(0, targetScore - currentScore);
             
    displayedRemaining = remaining; // Set at startup
        if (scoreText != null)
    scoreText.text = $"<color=#00FF00>{targetScore - currentScore}</color>";
   UpdateUI();
}


    public void AddScore(int score)
    {
  int oldRemaining = Mathf.Max(0, targetScore - currentScore);
int scoreToAdd = Mathf.Min(score, oldRemaining); // Prevent overshoot
currentScore += scoreToAdd;

int newRemaining = Mathf.Max(0, targetScore - currentScore);
// Animate the remaining number (big, green)
AnimateRemainingScoreDown(oldRemaining, newRemaining);

HandleFloatingFeedback(scoreToAdd, score > oldRemaining); // "Too much" feedback if overshoot


    }

    public void DeductMove()
    {
      //  if (currentMoves >= totalMoves) return;

        currentMoves++;
        UpdateUI();
   //     CheckGameStatus();
    }

    public int GetRemainingMoves()
    {
        return totalMoves - currentMoves;
    }

    public void CheckGameStatus()
    {
        if (currentScore >= targetScore)
        {
            GameWin();
        }
        else if (currentMoves >= totalMoves && currentScore < targetScore)
    {
        GameOver();
    }
    }

    private void GameWin()
    {
        Debug.Log("🎉 Game Won! 🎯");
        gameWinPanel.SetActive(true);
        GameAnalyticsController.LevelBasedProgressionRelated.LogLevelEndEventWithTime(GameAnalyticsController.LevelBasedProgressionRelated.levelProgressTimeData);
        GameAnalyticsController.LevelBasedProgressionRelated.LogLevelEndWithMoves(LevelController.instance.CurrentLevelforAnalytics ,currentMoves);
         DartC.instance.isGameActive = false;
GameObject.Find("AudioManager")?.GetComponent<AudioManager>()?.PlayWin();
    }

    private void GameOver()
    {
        Debug.Log("💀 Game Over! 😞");
        gameOverPanel.SetActive(true);
        GameAnalyticsController.LevelBasedProgressionRelated.LogLevelFailEventWithTime(GameAnalyticsController.LevelBasedProgressionRelated.levelProgressTimeData);
         DartC.instance.isGameActive = false;
GameObject.Find("AudioManager")?.GetComponent<AudioManager>()?.PlayFail();
    }

    private void UpdateUI()
{

  
    if (movesText != null)
        movesText.text = $"x{GetRemainingMoves()}";
}

private GameObject FindInactiveObject(string name)
{
    Transform[] all = Resources.FindObjectsOfTypeAll<Transform>();
    foreach (Transform t in all)
    {
        if (t.hideFlags == HideFlags.None && t.name == name)
        {
            return t.gameObject;
        }
    }
    return null;
}
 public void HandleFloatingFeedback(int scoreAdded, bool isTooMuch = false)
{
    if (floatingImage == null) return;

    Sprite chosenSprite = null;

    if (isTooMuch)
        chosenSprite = spriteTooMuch; // <-- Use your new sprite
    else if (scoreAdded <= 0)
        chosenSprite = spriteOhNo;
    else if (scoreAdded >= 80)
        chosenSprite = spriteImpressive;
    else if (scoreAdded >= 40)
        chosenSprite = spriteBrilliant;
    else if (scoreAdded >= 20)
        chosenSprite = spriteGood;

    if (chosenSprite == null) return;

    floatingImage.sprite = chosenSprite;

    if (floatingRoutine != null)
        StopCoroutine(floatingRoutine);

    floatingRoutine = StartCoroutine(ShowFloatingFeedback());
}


    private IEnumerator ShowFloatingFeedback()
    {
        floatingImage.rectTransform.anchoredPosition = originalPosition;

        // Fade in
        Color color = floatingImage.color;
        color.a = 1f;
        floatingImage.color = color;
        floatingImage.gameObject.SetActive(true);

        float duration = 2.5f;
        float elapsed = 0f;
        Vector3 targetPos = originalPosition + new Vector3(0, 100f, 0);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            floatingImage.rectTransform.anchoredPosition = Vector3.Lerp(originalPosition, targetPos, t);

            // Fade out
            color.a = 1f - t;
            floatingImage.color = color;

            yield return null;
        }

        floatingImage.color = new Color(1, 1, 1, 0);
        floatingImage.rectTransform.anchoredPosition = originalPosition;
    }
    public void ShowFloatingScoreFromTo(int score, Vector2 startCanvasPos)
{
    if (floatingScoreText == null || scoreTextRect == null) return;

    floatingScoreText.text = $"-{score}";
    floatingScoreText.color = Color.white;
    floatingScoreText.rectTransform.anchoredPosition = startCanvasPos;
    floatingScoreText.gameObject.SetActive(true);

    StartCoroutine(FloatingScoreToTargetRoutine(score, startCanvasPos));
}

public void ShowFloatingScoreAtWorld(Vector3 worldPos, int score)
{
    // 1. Convert world position to screen point
    Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPos);

    // 2. Convert to local point on your UI canvas (assuming FloatingScoreText is under GameCanvas)
    RectTransform canvasRect = floatingScoreText.canvas.transform as RectTransform;
    Vector2 localPoint;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        canvasRect,
        screenPoint,
        Camera.main,
        out localPoint
    );

    // 3. Move the floating score text to that UI position
    floatingScoreText.rectTransform.anchoredPosition = localPoint;
    floatingScoreText.text = $"-{score}";
    floatingScoreText.color = Color.white;
    floatingScoreText.gameObject.SetActive(true);

    // 4. Animate it (move up, fade out, etc)
    StartCoroutine(FloatingScoreToTargetRoutine(score, localPoint));
}

private IEnumerator FloatingScoreToTargetRoutine(int score, Vector2 start)
{
    float duration = 0.6f;
    float elapsed = 0f;
    ///Vector2 start = floatStartPos;

    // The canvas and target rect
    Canvas canvas = scoreTextRect.GetComponentInParent<Canvas>();

    // World position of ScoreGlobal's pivot (center)
    Vector3 worldTargetPos = scoreTextRect.transform.position;

    // Convert to local point in the parent of floatingScoreText
    RectTransform parentRect = floatingScoreText.rectTransform.parent as RectTransform;
    Vector2 target;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        parentRect,
        RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, worldTargetPos),
        canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
        out target
    );

    // Animate movement
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        floatingScoreText.rectTransform.anchoredPosition = Vector2.Lerp(start, target, t);
        // (Optional: fade code here)
        yield return null;
    }

    floatingScoreText.rectTransform.anchoredPosition = target; // Snap exactly!
    floatingScoreText.gameObject.SetActive(false);

    // Start score countdown after animation
    int oldRemaining = Mathf.Max(0, targetScore - currentScore + score);
    int newRemaining = Mathf.Max(0, targetScore - currentScore);
AnimateCurrentScoreUp(oldCurrent, newCurrent);
}


public void AnimateRemainingScoreDown(int from, int to)
{
    if (scoreAnimateRoutine != null)
        StopCoroutine(scoreAnimateRoutine);

    scoreAnimateRoutine = StartCoroutine(AnimateRemainingRoutine(from, to));
}


//private Coroutine scoreAnimateRoutine;
private int displayedCurrent = 0; // Track displayed current score

public void AnimateCurrentScoreUp(int from, int to)
{
    if (scoreAnimateRoutine != null)
        StopCoroutine(scoreAnimateRoutine);

    scoreAnimateRoutine = StartCoroutine(AnimateCurrentScoreRoutine(from, to));
}

private IEnumerator AnimateCurrentScoreRoutine(int from, int to)
{
    yield return StartCoroutine(PumpScoreText());
    float duration = 1.2f;
    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        int val = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
        UpdateScoreUI(val); // Update UI with currentScore!
        displayedCurrent = val;
        yield return null;
    }

    UpdateScoreUI(to);
    displayedCurrent = to;
    scoreAnimateRoutine = null;
}

private void UpdateScoreUI(int current)
{
    if (scoreText != null)
        scoreText.text = $"<color=#00FF00>{targetScore - current}</color>";
}

private IEnumerator AnimateRemainingRoutine(int from, int to)
{
     yield return StartCoroutine(PumpScoreText());
    float duration = 2.5f; // duration in seconds (change as needed)
    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        int val = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
        UpdateRemainingUI(val);
        displayedRemaining = val;
        yield return null;
    }

    UpdateRemainingUI(to);
    displayedRemaining = to;
    scoreAnimateRoutine = null;
 //   CheckGameStatus();
}
private IEnumerator PumpScoreText()
{
    float baseScale = 4f;        // Default scale
    float popUpScale = 1.4f;     // Pump factor
    float duration = 0.12f;
    float elapsed = 0f;

    Vector3 startScale = Vector3.one * baseScale;
    Vector3 endScale = Vector3.one * (baseScale * popUpScale);

    // Set initial scale just in case
    scoreTextNewRect.localScale = startScale;

    // Scale up quickly
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        scoreTextNewRect.localScale = Vector3.Lerp(startScale, endScale, t);
        yield return null;
    }
    scoreTextNewRect.localScale = endScale;

    // Scale back to base quickly
    elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        scoreTextNewRect.localScale = Vector3.Lerp(endScale, startScale, t);
        yield return null;
    }
    scoreTextNewRect.localScale = startScale;
}



private void UpdateRemainingUI(int remaining)
{
    displayedRemaining = remaining;
    if (scoreText != null)
        scoreText.text = $"<color=#00FF00>{remaining}</color>";

    // Pop animation
    if (scoreTextRect != null)
    {
        if (scorePopRoutine != null)
            StopCoroutine(scorePopRoutine);
        scorePopRoutine = StartCoroutine(PopScoreTextRoutine());
    }
}

private IEnumerator PopScoreTextRoutine()
{
    float popUpScale = 1.4f;
    float duration = 0.1f;
    float elapsed = 0f;
    Vector3 startScale = Vector3.one;
    Vector3 endScale = Vector3.one * popUpScale;

    // Scale up quickly
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        scoreTextRect.localScale = Vector3.Lerp(startScale, endScale, t);
        yield return null;
    }
    scoreTextRect.localScale = endScale;

    // Scale back to normal
    elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        scoreTextRect.localScale = Vector3.Lerp(endScale, startScale, t);
        yield return null;
    }
    scoreTextRect.localScale = startScale;
}


}
