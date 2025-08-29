using System;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Gameplay.Script.InScene
{
    public class Achievement3DItem : MonoBehaviour
    {
        [SerializeField] private TextMeshPro achievementName;

        private string _achievementId = null;
        private AchievementInherentData _achievement;
        
        XRGrabInteractable _interactable;
        private Action<string> _leftPerformed;
        private Action<string> _rightPerformed;
        private void Start()
        {
            _interactable = GetComponentInChildren<XRGrabInteractable>();
            _interactable.hoverEntered.AddListener(OnHoverEntered);
            _interactable.hoverExited.AddListener(OnHoverExited);
            // EventDispatcher.Instance.Register();
        }

        private void OnDestroy()
        {
            _interactable.hoverEntered.RemoveListener(OnHoverEntered);
        }

        private void OnHoverEntered(HoverEnterEventArgs arg0)
        {
            if (arg0.interactorObject.transform.parent.transform.name.ToLower().Contains("left"))
            {
                _leftPerformed?.Invoke(_achievementId);
            }
            if (arg0.interactorObject.transform.parent.transform.name.ToLower().Contains("right"))
            {
                _rightPerformed?.Invoke(_achievementId);
            }
        }
        
        private void OnHoverExited(HoverExitEventArgs arg0)
        {
            if (arg0.interactorObject.transform.parent.transform.name.ToLower().Contains("left"))
            {
                _leftPerformed?.Invoke(null);
            }
            if (arg0.interactorObject.transform.parent.transform.name.ToLower().Contains("right"))
            {
                _rightPerformed?.Invoke(null);
            }
        }

        public void InitItem(string achievementId = null, 
            Action<string> leftPerformed = null, Action<string> rightPerformed = null)
        {
            _achievementId = achievementId;
            if (achievementId == null)
            {
                gameObject.SetActive(false);
                return;
            }
            _achievement = AchievementLogic.Instance.GetAchievement(_achievementId);
            if (_achievement != null)
                achievementName.text = _achievement.name;
            gameObject.SetActive(true);
            _leftPerformed = leftPerformed;
            _rightPerformed = rightPerformed;
        }
    }
}