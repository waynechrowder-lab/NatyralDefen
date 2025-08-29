using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    /// <summary>
    /// Handles all zombie related audio.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ZombieAudio : MonoBehaviour
    {
        [SerializeField] private AudioClip attackAudio;
        [SerializeField] private AudioClip walkAudio;
        [SerializeField] private AudioClip underAttackAudio;
        [SerializeField] private AudioClip[] deathAudios;

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 1f;
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip == null) return;
            _audioSource.clip = clip;
            _audioSource.Play();
        }

        public void PlayWalk() => PlayClip(walkAudio);
        public void PlayAttack() => PlayClip(attackAudio);
        public void PlayUnderAttack() => PlayClip(underAttackAudio);

        public void PlayDeath()
        {
            if (deathAudios != null && deathAudios.Length > 0)
            {
                PlayClip(deathAudios[Random.Range(0, deathAudios.Length)]);
            }
        }

        public void Stop() => _audioSource.Stop();
    }
}

