using System;
using System.Collections.Generic;
using EventControl;
using UnityEngine;
using UnityEngine.Audio;

namespace ManagerControl
{
    [Serializable]
    public class PopBubbleSound
    {
        public SoundEvent soundEvent;
        public AudioClip clip;
    }

    public class SoundController : MonoBehaviour
    {
        [SerializeField] private AudioSource effectSource;
        [SerializeField] private PopBubbleSound[] popBubbleSounds;
        private Dictionary<SoundEvent, AudioClip> _popSoundDict = new();

        private void Awake()
        {
            _popSoundDict = new Dictionary<SoundEvent, AudioClip>();
            for (int i = 0; i < popBubbleSounds.Length; i++)
            {
                _popSoundDict.Add(popBubbleSounds[i].soundEvent, popBubbleSounds[i].clip);
            }
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable() { }

        private void PopSound(SoundEvent soundEvent)
        {
            var clip = _popSoundDict[soundEvent];
            effectSource.PlayOneShot(clip);
        }
    }
}