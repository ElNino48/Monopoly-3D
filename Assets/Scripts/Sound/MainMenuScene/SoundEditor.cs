//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;

//public class SoundEditor : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
//{
//    public  AudioClip[] sounds;
//    public  SoundArrays[] randomSound;
     
//    [SerializeField]  float volume = 1f;

//    private  AudioSource audioSource => GetComponent<AudioSource>();

//    public void PlaySound(int i, float volume = 1f, bool random = false)
//    {
//        AudioClip audioClip;
//        if (random)
//        {
//           audioClip = randomSound[i].soundArray[Random.Range(0, randomSound[i].soundArray.Length)];
//        }
//        else
//        {
//            audioClip = sounds[i];
//        }
//        audioSource.PlayOneShot(audioClip, volume);
//    }
//    public void OnPointerEnter(PointerEventData ped)
//    {
//        AudioClip audioClip; 
//        audioClip = sounds[0];
//    }
//    public void OnPointerDown(PointerEventData ped)
//    {
//        AudioClip audioClip;
//        audioClip = sounds[0];
//    }

//    [System.Serializable]
//    public class SoundArrays
//    {
//        public AudioClip[] soundArray;
//    }
//}
