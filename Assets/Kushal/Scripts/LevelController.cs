using UnityEngine;
using TMPro;
using System.Collections;
using GameAnalyticsSDK;

public class LevelController : MonoBehaviour
{
    public static LevelController instance;

    [Header("Level Setup")]
    public GameObject[] levelPrefabs; // Assign 25 level prefabs in Inspector
    public Transform levelParent;     // Empty parent to hold instantiated level

    [Header("UI References")]
    public TMP_Text levelText;
    public GameObject nextButton;
    public GameObject retryButton;

    private GameObject currentLevel;
    private int currentLevelIndex;

    public int CurrentLevelforAnalytics => currentLevelIndex + 1;


    private const string LEVEL_KEY = "CurrentLevel";
    public GameObject loadingOverlay; // assign in Inspector
// In your LevelController/loader, on new level start:
[SerializeField] LevelIntroController levelIntro;
[SerializeField] int levelTargetScore; // set this before start

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
         if (!PlayerPrefs.HasKey(LEVEL_KEY))
            {
                PlayerPrefs.SetInt(LEVEL_KEY, 0);
                GameAnalytics.NewDesignEvent("gamestart");
            }

        currentLevelIndex = PlayerPrefs.GetInt(LEVEL_KEY);
        LoadLevel(currentLevelIndex);      

     
    }

public void LoadLevel(int index)
{

    
GameAnalyticsController.LevelBasedProgressionRelated.LogLevelStartEventWithTime(CurrentLevelforAnalytics);

    Debug.Log("Level Start " + CurrentLevelforAnalytics);
    
    StartCoroutine(LoadLevelRoutine(index));
}
  private IEnumerator LoadLevelRoutine(int index)
{ if (loadingOverlay != null)
        loadingOverlay.SetActive(true); // Show

    // Destroy previous level
    if (currentLevel != null)
        Destroy(currentLevel);

    yield return null; // Wait one frame so Destroy happens

    // Loop the level
    int prefabIndex = index % levelPrefabs.Length;
    currentLevel = Instantiate(levelPrefabs[prefabIndex], levelParent);

    // Update UI
    levelText.text = " " + (index + 1);

    if (loadingOverlay != null)
        loadingOverlay.SetActive(false); // Hide after new level is ready!


         // ----> CALL LEVEL INTRO HERE!
    int targetScore = FindObjectOfType<GameManager>().targetScore; // Or however you set the level target
    levelIntro.PlayLevelIntro(targetScore, () => {
        // Optional: Any additional code when intro finishes (gameplay is ready)
    });
}
    public void OnNextLevel()
    {

    Debug.Log("Level Win " + CurrentLevelforAnalytics);

        currentLevelIndex++;
        PlayerPrefs.SetInt(LEVEL_KEY, currentLevelIndex);
        DestroyAllDarts();

        LoadLevel(currentLevelIndex);
    }

    public void OnRetryLevel()
    {
    Debug.Log("Level Fail " + CurrentLevelforAnalytics);
DestroyAllDarts();
        LoadLevel(currentLevelIndex);
    }

    void DestroyAllDarts()
{
    var allDarts = GameObject.FindGameObjectsWithTag("Dart");
    foreach (var dart in allDarts)
    {
        Destroy(dart);
    }
}

}
