using System.Collections;
using Currency.Core.Run;
using Gameplay.Script.Logic;
using UnityEngine;
using UnityEngine.Audio;

namespace Gameplay.Script.Manager
{
    public class AudioMgr : MonoSingle<AudioMgr>
    {
        private AudioSource _audioBgm;
        private AudioSource _audioSource;
        private AudioSource _audioSourceOneShot;
        private float _currentVelocity;
        private float _fadeDuration = .5f;
        protected override void Awake()
        {
            base.Awake();
            _audioBgm = new GameObject("_audioBgm").AddComponent<AudioSource>();
            _audioBgm.playOnAwake = false;
            _audioBgm.loop = true;
            _audioBgm.transform.SetParent(transform);
            _audioSource = new GameObject("_audioSource").AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.transform.SetParent(transform);
            _audioSourceOneShot = new GameObject("_audioSourceOneShot").AddComponent<AudioSource>();
            _audioSourceOneShot.playOnAwake = false;
            _audioSourceOneShot.loop = false;
            _audioSourceOneShot.transform.SetParent(transform);
        }

        public float PlaySound(AudioClip audioClip, float volume = 1)
        {
            if (!audioClip) return 0;
            _audioSource.clip = audioClip;
            _audioSource.volume = volume;
            _audioSource.Play();
            return audioClip.length;
        }

        public float PlayBGM(string clipName, float volume = .5f)
        {
            volume = SettingsLogic.Instance.AudioBackgroundVolume;
            var clip = Resources.Load<AudioClip>($"Audio/{clipName}");
            _audioBgm.Stop();
            if (clip)
            {
                _audioBgm.clip = clip;
                _audioBgm.volume = 0;
                _audioBgm.Play();
                StopAllCoroutines();
                StartCoroutine(AudioVolumeSmooth(volume));
                return clip.length;
            }
            return 0;
        }

        public float StopBGM()
        {
            StopAllCoroutines();
            if (_audioBgm.isPlaying)
            {
                StopAllCoroutines();
                StartCoroutine(AudioVolumeSmooth(0));
            }
            return _fadeDuration;
        }

        IEnumerator AudioVolumeSmooth(float target)
        {
            while (true)
            {
                _audioBgm.volume = Mathf.SmoothDamp(
                    _audioBgm.volume, target, ref _currentVelocity, _fadeDuration);
                yield return new WaitForEndOfFrame();
            }
        }
        
        public void PlaySoundRepeat(AudioClip audioClip, float repeatTime, float volume = 1)
        {
            if (!audioClip) return;
            _audioSource.clip = audioClip;
            _audioSource.volume = volume;
            InvokeRepeating(nameof(PlaySound), 0, repeatTime);
        }

        public void PlaySoundCancelRepeat()
        {
            if (!_audioSource.clip) return;
            _audioSource.Stop();
            CancelInvoke(nameof(PlaySound));
        }
        
        void PlaySound()
        {
            if (!_audioSource.clip)
                CancelInvoke(nameof(PlaySound));
            else
                _audioSource.Play();
        }
        
        public float PlaySoundOneShot(AudioClip audioClip, float volume = 1, float pitch = 1)
        {
            if (!audioClip) return 0;
            _audioSourceOneShot.clip = audioClip;
            _audioSourceOneShot.pitch = pitch;
            _audioSourceOneShot.PlayOneShot(audioClip, volume);
            return audioClip.length;
        }
        
    }
}