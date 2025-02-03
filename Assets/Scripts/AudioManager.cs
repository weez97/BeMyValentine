using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class SfxTrack
    {
        public string id;
        public AudioClip track;
    }

    public static AudioManager Instance { get; private set; }

    public AudioSource musicLoop;
    public AudioSource sfx;
    public SfxTrack[] tracks;

    public bool IsMusicOn { get; private set; }
    private const string AudioKey = "IS_AUDIO_ON";

    private Dictionary<string, AudioClip> sfxDict = new();

    void Awake()
    {
        Instance = this;

        foreach (SfxTrack track in tracks)
            sfxDict.TryAdd(track.id, track.track);
    }

    public void Init()
    {
        // load data
        IsMusicOn = PlayerPrefs.GetInt(AudioKey, 1) == 1 ? true : false;

        if (IsMusicOn)
            musicLoop.Play();
    }

    public void ToggleAudio(bool isOn)
    {
        IsMusicOn = !IsMusicOn;
        if (IsMusicOn)
            musicLoop.Play();
        else
            musicLoop.Stop();
        PlayerPrefs.SetInt(AudioKey, IsMusicOn ? 1 : 0);
    }

    public void PlaySfx(string id)
    {
        sfxDict.TryGetValue(id, out AudioClip clip);

        if (clip == null) return;

        sfx.clip = clip;
        sfx.Play();
    }
}
