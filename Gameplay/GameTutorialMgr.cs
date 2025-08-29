using System;
using System.Linq;
using Currency.Core.Run;
using Gameplay.Script.Manager;
using UnityEngine;
using UnityEngine.Video;

namespace Gameplay.Script.Gameplay
{
    public class GameTutorialMgr : MonoSingle<GameTutorialMgr>
    {
        [SerializeField] private VideoPlayer videoPlayer;
        // [SerializeField] private AudioSource audioPlayer;
        [SerializeField] private GameTutorialItem[] gameTutorialItems;

        private void Start()
        {
            videoPlayer.gameObject.SetActive(false);
            // audioPlayer.gameObject.SetActive(false);
            gameTutorialItems.ToList().ForEach(value => value.panel.alpha = 0);
        }

        public bool StartGameTutorial(int index, out GameTutorialItem tutorialItem)
        {
            gameTutorialItems.ToList().ForEach(value => value.panel.alpha = 0);
            if (index < gameTutorialItems.Length)
            {
                tutorialItem = gameTutorialItems[index];

                // audioPlayer.gameObject.SetActive(true);
                if (tutorialItem.video)
                {
                    videoPlayer.gameObject.SetActive(true);
                    videoPlayer.clip = tutorialItem.video;
                    videoPlayer.Play();
                }
                else
                    videoPlayer.gameObject.SetActive(false);
                AudioMgr.Instance.PlaySoundCancelRepeat();
                AudioMgr.Instance.PlaySoundRepeat(tutorialItem.audio, tutorialItem.audio.length + 2);
                tutorialItem.panel.alpha = 1;
                return true;
            }
            else
            {
                tutorialItem = null;
                videoPlayer.Stop();
                AudioMgr.Instance.PlaySoundCancelRepeat();
                videoPlayer.gameObject.SetActive(false);
                return false;
            }
        }
    }

    [System.Serializable]
    public class GameTutorialItem
    {
        public CanvasGroup panel;
        public AudioClip audio;
        public VideoClip video;
        public bool skip;
    }
}