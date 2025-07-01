using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FTUEManager : MonoBehaviour
{
    [Header("FTUE Sequences")]
    public List<List<FTUEStep>> ftueSequences = new List<List<FTUEStep>>();

    [Header("Global Options")]
    public bool autoStart = true;
    public bool showDebugLogs = false;

    private int currentSequenceIndex = 0;
    private int currentStepIndex = 0;

    private void Start()
    {
        if (autoStart)
            StartCoroutine(RunFTUESequence(currentSequenceIndex));
    }

    public void StartFTUE(int sequenceIndex)
    {
        StopAllCoroutines();
        StartCoroutine(RunFTUESequence(sequenceIndex));
    }

    private IEnumerator RunFTUESequence(int index)
    {
        if (index >= ftueSequences.Count)
        {
            if (showDebugLogs) Debug.Log("No FTUE sequence at index " + index);
            yield break;
        }

        var steps = ftueSequences[index];
        currentStepIndex = 0;

        foreach (var step in steps)
        {
            if (showDebugLogs) Debug.Log("FTUE Step: " + step.stepID);

            // Delay before start
            yield return new WaitForSeconds(step.delayBeforeStart);

            // Apply content
            if (step.textObject && !string.IsNullOrEmpty(step.message))
            {
                TMP_Text text = step.textObject.GetComponent<TMP_Text>();
                if (text) text.text = step.message;
                step.textObject.SetActive(true);
            }

            if (step.imageObject && step.image)
            {
                Image img = step.imageObject.GetComponent<Image>();
                if (img) img.sprite = step.image;
                step.imageObject.SetActive(true);
            }

            if (step.voiceOver && step.voiceOverObject)
            {
                AudioSource source = step.voiceOverObject.GetComponent<AudioSource>();
                if (source)
                {
                    source.clip = step.voiceOver;
                    source.Play();
                }
            }

            // Enable/Disable gameobjects
            foreach (var obj in step.objectsToEnable)
                if (obj) obj.SetActive(true);

            foreach (var obj in step.objectsToDisable)
                if (obj) obj.SetActive(false);

            // Wait for click or duration
            if (step.waitForUserClick)
            {
                bool clicked = false;
                void WaitClick() => clicked = true;
                InputManager.OnTap += WaitClick;
                yield return new WaitUntil(() => clicked);
                InputManager.OnTap -= WaitClick;
            }
            else
            {
                yield return new WaitForSeconds(step.stepDuration);
            }

            // Hide UI objects after step (if needed)
            if (step.textObject) step.textObject.SetActive(false);
            if (step.imageObject) step.imageObject.SetActive(false);
        }

        if (showDebugLogs) Debug.Log("FTUE Sequence Finished");
    }
}

// Example tap detector (replace with your system)
public static class InputManager
{
    public static System.Action OnTap;
    static bool tapEnabled = true;

    public static void Update()
    {
        if (tapEnabled && Input.GetMouseButtonDown(0))
        {
            OnTap?.Invoke();
        }
    }
}
