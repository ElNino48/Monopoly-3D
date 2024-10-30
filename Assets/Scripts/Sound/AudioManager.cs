using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] musicTracks;
    private AudioSource audioSource;
    private int currentTrackIndex = 0;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PlayTrack(currentTrackIndex);
    }

    void PlayTrack(int index)
    {
        audioSource.clip = musicTracks[index];
        audioSource.Play();
    }
    void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;
        PlayTrack(currentTrackIndex);
    }
    void PreviousTrack()
    {
        currentTrackIndex = (currentTrackIndex - 1 + musicTracks.Length) % musicTracks.Length;
        PlayTrack(currentTrackIndex);
    }
    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }
}
