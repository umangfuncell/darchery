using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class FTUEStep
{
    [Header("FTUE Content")]
    public string stepID;
    public string message;
    public Sprite image;
    public AudioClip voiceOver;

    [Header("UI Elements")]
    public GameObject textObject;
    public GameObject imageObject;
    public GameObject voiceOverObject;

    [Header("Trigger Conditions")]
    public float delayBeforeStart = 0f;
    public float stepDuration = 5f;
    public bool waitForUserClick = false;

    [Header("Optional")]
    public GameObject[] objectsToEnable;
    public GameObject[] objectsToDisable;

    [Header("Logic")]
    public bool autoProceed = true;
}
