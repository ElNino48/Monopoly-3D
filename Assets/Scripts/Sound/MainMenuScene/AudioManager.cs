using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioClip[] musicTracks;
    public AudioClip[] sfxSounds;
    public AudioMixer audioMixer;
    private AudioSource musicAudioSource;
    private AudioSource sfxAudioSource;

    [SerializeField] GameObject sfxManager;

    private int currentTrackIndex = 0;
    public int GetCurrentTrackIndex => currentTrackIndex;

    // Àﬁ◊» ƒÀﬂ ”œ–¿¬À≈Õ»ﬂ √–ŒÃ Œ—“‹ﬁ
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    public float musicVolume = 0.5f;
    public float SFXVolume = 0.5f;
    public TMP_Text musicVolumeText;
    public TMP_Text SFXVolumeText;

    //public TMP_Text trackNameText;

    private float initialMusicVolume;
    private float initialSFXVolume;
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        musicAudioSource = GetComponent<AudioSource>();
        sfxAudioSource = sfxManager.GetComponent<AudioSource>();
        PlayTrack(currentTrackIndex);

        //Õ¿◊¿À‹Õ€… ”–Œ¬≈Õ‹ √–ŒÃ Œ—“» (Ã”«€ ») »« ‘¿…À¿:
        initialMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.5f);
        float musicVolume = initialMusicVolume;
        SetMusicVolume(musicVolume);
        UpdateMusicVolumeText();

        //Õ¿◊¿À‹Õ€… ”–Œ¬≈Õ‹ √–ŒÃ Œ—“» (SFX) »« ‘¿…À¿:
        initialSFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 0.5f);
        float SFXVolume = initialSFXVolume;
        Debug.Log(initialMusicVolume + "=music");
        Debug.Log(initialSFXVolume + "=sfx");
        SetSFXVolume(SFXVolume);
        UpdateSFXVolumeText();
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
        UpdateTrackName(index);
    }
    public void NextTrack()
    {
        currentTrackIndex++;
        if (currentTrackIndex >= musicTracks.Length)
        {
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
        PlayTrack(currentTrackIndex); // ¬ÓÒÔÓËÁ‚ÂÒÚË ÚÂÍÛ˘ËÈ ÚÂÍ
    }
    private void UpdateTrackName(int index)
    {
        //trackNameText.text = musicTracks[index].name; // Œ·ÌÓ‚ÎÂÌËÂ Ì‡Á‚‡ÌËˇ 
    }
    public void SetMusicVolume(float volume)
    {
        //”—“¿ÕŒ¬»“‹ «Õ¿◊≈Õ»≈ √–ŒÃ Œ—“» ¬ AudioMixer:
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);//‚ ƒÂˆË¡ÂÎ˚
        musicAudioSource.volume = volume;
        musicVolume = volume;
        UpdateMusicVolumeText();
    }
    public void SetSFXVolume(float volume)
    {
        //”—“¿ÕŒ¬»“‹ «Õ¿◊≈Õ»≈ √–ŒÃ Œ—“» ¬ AudioMixer sfxManager'a:
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);//‚ ƒÂˆË¡ÂÎ˚
        sfxAudioSource.volume = volume;
        SFXVolume = volume;
        UpdateSFXVolumeText();
    }
    public void IncreaseMusicVolume()
    {
        musicVolume = Mathf.Clamp(musicVolume + 0.1f, 0f, 1f);
        SetMusicVolume(musicVolume);
        Debug.Log(musicAudioSource.volume);
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
