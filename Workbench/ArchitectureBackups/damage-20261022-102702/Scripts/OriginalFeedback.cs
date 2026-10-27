using UnityEngine;

namespace GuJian
{
    public sealed partial class GuJianGame
    {
        [SerializeField] private AudioSource swingAudio, hitAudio, criticalAudio;
        private void PlayOriginalSound(int index)
        {
            if (SoundVolumeStep <= 0) return;
            var source = index == 0 ? swingAudio : index == 1 ? criticalAudio : hitAudio;
            if (source && !source.isPlaying) source.Play();
        }
    }
}
