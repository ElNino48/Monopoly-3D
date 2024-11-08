using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class GameSceneAudioManager : MonoBehaviour
{
    public AudioClip[] musicTracks;
    public AudioClip[] sfxSounds;
    public AudioMixer audioMixer;
    private AudioSource musicAudioSource;
    private AudioSource sfxAudioSource;
    public GameObject gameSceneSFXManager;

    //ÊËÞ×È ÄËß ÓÏÐÀÂËÅÍÈß ÃÐÎÌÊÎÑÒÜÞ
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    public float musicVolume = 0.5f;
    public float SFXVolume = 0.5f;
    public TMP_Text musicVolumeText;
    public TMP_Text SFXVolumeText;

    public TMP_Text trackNameText;

    private float initialMusicVolume;
    private float initialSFXVolume;
    int currentTrackIndex;
    public int GetCurrentTrackIndex => currentTrackIndex;
    //void Awake()
    //{
    //    if (instance == null)
    //    {
    //        instance = this;
    //        DontDestroyOnLoad(gameObject);
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //    }
    //}
    void Start()
    {
        musicAudioSource = GetComponent<AudioSource>();
        sfxAudioSource = gameSceneSFXManager.GetComponent<AudioSource>();
        //Debug.Log(currentTrackIndex + "=current index2");
        PlayTrack(currentTrackIndex);

        //ÍÀ×ÀËÜÍÛÉ ÓÐÎÂÅÍÜ ÃÐÎÌÊÎÑÒÈ (ÌÓÇÛÊÈ) ÈÇ ÔÀÉËÀ:
        initialMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.5f);
        float musicVolume = initialMusicVolume;
        SetMusicVolume(musicVolume);
        UpdateMusicVolumeText();

        //ÍÀ×ÀËÜÍÛÉ ÓÐÎÂÅÍÜ ÃÐÎÌÊÎÑÒÈ (SFX) ÈÇ ÔÀÉËÀ:
        initialSFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 0.5f);
        float SFXVolume = initialSFXVolume;
        //Debug.Log(initialMusicVolume + "=musicGameScene");
        //Debug.Log(initialSFXVolume + "=sfxGameScene");
        SetSFXVolume(SFXVolume);
        UpdateSFXVolumeText();
        //UPDATE Íàçâàíèÿ òåêóùåãî òðêà
        UpdateTrackName();
    }
    private void UpdateTrackName()
    {
        // Óáåäèòåñü, ÷òî òåêóùèé òðåê äîñòóïåí
        if ( musicTracks.Length > 0)
        {
            //Debug.Log(currentTrackIndex + "=current index3");
            trackNameText.text = musicTracks[GetCurrentTrackIndex].name;
        }
    }

    public void PlaySFX(int index)
    {
        sfxAudioSource.clip = sfxSounds[index];
        sfxAudioSource.PlayOneShot(sfxSounds[index]);
    }


    public void PlayTrack(int index)
    {
        musicAudioSource.clip = musicTracks[index];
        musicAudioSource.Play();

        //Debug.Log(currentTrackIndex + "=current index4");
        UpdateTrackName(index);
    }
    public void NextTrack()
    {
        currentTrackIndex++;
        if (currentTrackIndex >= musicTracks.Length)
        {
            //Debug.Log(currentTrackIndex + "=current index5");
            currentTrackIndex = 0;
        }
        PlayTrack(currentTrackIndex);
    }
    public void PreviousTrack()
    {
        currentTrackIndex--;
        if (currentTrackIndex < 0)
        {
            currentTrackIndex = musicTracks.Length - 1;
        }
        PlayTrack(currentTrackIndex);
    }
    public void StartPlaying()
    {
        PlayTrack(currentTrackIndex); // Âîñïðîèçâåñòè òåêóùèé òðåê
    }
    private void UpdateTrackName(int index)
    {
        trackNameText.text = musicTracks[index].name; // Îáíîâëåíèå íàçâàíèÿ 
    }
    public void SetMusicVolume(float volume)
    {
        //ÓÑÒÀÍÎÂÈÒÜ ÇÍÀ×ÅÍÈÅ ÃÐÎÌÊÎÑÒÈ Â AudioMixer:
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);//â ÄåöèÁåëû
        musicAudioSource.volume = volume;
        musicVolume = volume;
        UpdateMusicVolumeText();
    }
    public void SetSFXVolume(float volume)
    {
        //ÓÑÒÀÍÎÂÈÒÜ ÇÍÀ×ÅÍÈÅ ÃÐÎÌÊÎÑÒÈ Â AudioMixer sfxManager'a:
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);//â ÄåöèÁåëû
        sfxAudioSource.volume = volume;
        SFXVolume = volume;
        UpdateSFXVolumeText();
    }
    public void IncreaseMusicVolume()
    {
        musicVolume = Mathf.Clamp(musicVolume + 0.1f, 0f, 1f);
        SetMusicVolume(musicVolume);
        //Debug.Log(musicAudioSource.volume);
        UpdateMusicVolumeText();
    }
    public void DecreaseMusicVolume()
    {
        musicVolume = Mathf.Clamp(musicVolume - 0.1f, 0f, 1f);
        SetMusicVolume(musicVolume);
        UpdateMusicVolumeText();
    }
    public void IncreaseSFXVolume()
    {
        SFXVolume = Mathf.Clamp(SFXVolume + 0.1f, 0f, 1f);
        SetSFXVolume(SFXVolume);
        UpdateSFXVolumeText();
    }
    public void DecreaseSFXVolume()
    {
        SFXVolume = Mathf.Clamp(SFXVolume - 0.1f, 0f, 1f);
        SetSFXVolume(SFXVolume);
        UpdateSFXVolumeText();
    }
    public void ApplySettings()
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.SetFloat(SFXVolumeKey, SFXVolume);
        PlayerPrefs.Save();
    }
    public void CancelSettings()
    {
        initialMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.5f);
        initialSFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 0.5f);
        SetMusicVolume(initialMusicVolume);
        SetSFXVolume(initialSFXVolume);
        //Debug.Log(initialMusicVolume + "=musicGameScene");
        //Debug.Log(initialSFXVolume + "=sfxGameScene");
    }
    private void UpdateMusicVolumeText()
    {
        musicVolumeText.text = Mathf.RoundToInt(musicVolume * 100) + "%";
    }
    private void UpdateSFXVolumeText()
    {
        SFXVolumeText.text = Mathf.RoundToInt(SFXVolume * 100) + "%";
    }
}

