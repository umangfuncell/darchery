using UnityEngine;
using GameAnalyticsSDK;

public class TutorialLevel1 : MonoBehaviour
{
    public GameObject holdToAimPanel;
    public GameObject aimAndReleasePanel;

    private bool hasHeld = false;
    private bool hasShot = false;
    private float holdTimer = 0f;
    private float holdDuration = 1.25f; // Require holding at least ~0.2 seconds

    // --- NEW: Track if intro actually started ---
    private bool tutorialStarted = false;

    void Start()
    {
        // Only show tutorial if it's not marked complete
        if (PlayerPrefs.GetInt("TutorialLevel1_Complete", 0) == 1)
        {
            // Hide all panels, disable this script
            if (holdToAimPanel) holdToAimPanel.SetActive(false);
            if (aimAndReleasePanel) aimAndReleasePanel.SetActive(false);
            enabled = false;
            return;
        }

        if (!GameManager.instance.isTutorial)
        {
            if (holdToAimPanel) holdToAimPanel.SetActive(false);
            if (aimAndReleasePanel) aimAndReleasePanel.SetActive(false);
            enabled = false;
            return;
        }

        // DO NOT DISABLE HERE -- wait for DartC ready in Update

        // Initially hide both panels, show only when DartC is ready
        if (holdToAimPanel) holdToAimPanel.SetActive(false);
        if (aimAndReleasePanel) aimAndReleasePanel.SetActive(false);
    }

    void Update()
    {
        // Still require tutorial level, otherwise do nothing
        if (!GameManager.instance.isTutorial) return;

        // Wait until DartC is active, THEN start the tutorial panels
        if (!tutorialStarted)
        {
            if (DartC.instance != null && DartC.instance.isGameActive)
            {
                if (holdToAimPanel) holdToAimPanel.SetActive(true);
                if (aimAndReleasePanel) aimAndReleasePanel.SetActive(false);
                GameAnalytics.NewDesignEvent("ftue_start");
                tutorialStarted = true; // Now start main tutorial logic
            }
            return; // Don't continue tutorial flow until DartC is active!
        }

        // --- From here on, regular tutorial logic (no changes) ---
        if (!hasHeld)
        {
            if (Input.GetMouseButton(0))
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= holdDuration)
                {
                    hasHeld = true;
                    if (holdToAimPanel) holdToAimPanel.SetActive(false);
                    if (aimAndReleasePanel) aimAndReleasePanel.SetActive(true);
                }
            }
            else
            {
                holdTimer = 0f; // Reset timer if not holding
            }
        }
        else if (!hasShot && Input.GetMouseButtonUp(0))
        {
            hasShot = true;
            if (aimAndReleasePanel) aimAndReleasePanel.SetActive(false);

            // Mark tutorial as completed!
            PlayerPrefs.SetInt("TutorialLevel1_Complete", 1);
            PlayerPrefs.Save();
            GameAnalytics.NewDesignEvent("ftue_complete");
        }
    }

    // --- REPLACE THESE WITH YOUR OWN GAME'S INPUT LOGIC ---
    private bool IsHoldingDart()
    {
        // Replace with your actual hold detection, e.g. DartC.instance.isHolding
        return Input.GetMouseButton(0); // For example: mouse/touch is held
    }
    private bool HasShotDart()
    {
        // Replace with your actual shot detection, e.g. DartC.instance.hasShot
        return Input.GetMouseButtonUp(0); // For example: mouse/touch released
    }
}
