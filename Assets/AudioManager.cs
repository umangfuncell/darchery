using UnityEngine;
using Lofelt.NiceVibrations;

public class AudioManager : MonoBehaviour
{
    public AudioClip releaseClip;
    public AudioClip hitClip;

    public AudioClip winClip;
public AudioClip failClip;
public AudioClip stretchClip;


    private AudioSource audioSource;
public bool soundOn = true; // Default: true
public bool bgMusicOn = true;
public bool hapticsOn = true;

private AudioSource lastBGMusicSource;


public void SetSound(bool on)
{
    soundOn = on;
    audioSource.mute = !on;
    // Optional: mute all AudioSources in game if needed
}

public void SetBGMusic(bool on)
{
    bgMusicOn = on;
    ApplyBGMusicMute();
}
public void SetHaptics(bool on)
{
    hapticsOn = on;
    HapticController.hapticsEnabled = on;
}


private void ApplyBGMusicMute()
{
    // Always look for current BGMusic tag
    GameObject bgMusicObj = GameObject.FindWithTag("BGMusic");
    if (bgMusicObj != null)
    {
        var bgSource = bgMusicObj.GetComponent<AudioSource>();
        if (bgSource != null)
        {
            bgSource.mute = !bgMusicOn;
            lastBGMusicSource = bgSource;
        }
    }
}

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayRelease()
    {
        if (releaseClip != null)
            audioSource.PlayOneShot(releaseClip);
    }


    public void PlayHit()
    {
        if (hitClip != null)
            audioSource.PlayOneShot(hitClip);
    }

    public void PlayWin()
{
    if (winClip != null)
        audioSource.PlayOneShot(winClip);
}

public void PlayFail()
{
    if (failClip != null)
        audioSource.PlayOneShot(failClip);
}


private void Update()
{
    // This ensures that if a new BGMusic is spawned, it gets the mute state
    GameObject bgMusicObj = GameObject.FindWithTag("BGMusic");
    if (bgMusicObj != null)
    {
        var bgSource = bgMusicObj.GetComponent<AudioSource>();
        if (bgSource != null && bgSource != lastBGMusicSource)
        {
            bgSource.mute = !bgMusicOn;
            lastBGMusicSource = bgSource;
        }
    }
}
}
