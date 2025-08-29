using System;
using Gameplay.Script.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class AchievementUI : BasedUI
    {
        [SerializeField] private TMP_Text achievementName;
        [SerializeField] private TMP_Text achievementCondition;
        [SerializeField] private TMP_Text achievementDescription;
        [SerializeField] private TMP_Text achievementAcquisitionTime;
        [SerializeField] private Image achievementIcon;
        [SerializeField] private CanvasGroup canvasGroup;

        private void Awake()
        {
            OnClickClose();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EventDispatcher.Instance.Register((int)EventID.SelectAchievement, OnSelectAchievement);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventDispatcher.Instance.UnRegister((int)EventID.SelectAchievement, OnSelectAchievement);
        }

        private void OnSelectAchievement(GameEventArg arg)
        {
            var achievementId = arg.GetArg<string>(0);
            if (achievementId == null)
            {
                return;
            }
            var achievement = AchievementLogic.Instance.GetAchievement(achievementId);
            if (achievement == null) return;
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            achievementName.text = achievement.name;
            achievementCondition.text = achievement.condition;
            achievementDescription.text = achievement.description;
        }

        public void OnClickClose()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}