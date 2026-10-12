using UnityEngine;

namespace Horsecremental.Racing
{
    public enum StableSound { CropClick, GrowthStage, Harvest, Pickup, TrainingLap, TrainingUpgrade, NewHorse, RaceTransition }
    [RequireComponent(typeof(AudioSource))]
    public sealed class StableAudioHooks : MonoBehaviour
    {
        [SerializeField] private AudioClip[] _clips = new AudioClip[8];
        private AudioSource _source;
        private void Awake() => TryGetComponent(out _source);
        public void Play(StableSound sound)
        {
            int index = (int)sound;
            if (index < _clips.Length && _clips[index] != null) _source.PlayOneShot(_clips[index], 0.45f);
        }
    }
}

