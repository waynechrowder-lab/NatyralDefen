using System;
using System.Collections;
using Gameplay.Script.Logic;
using Gameplay.Scripts.Manager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Script.InScene
{
    public class Achievement3D : MonoBehaviour
    {
        [SerializeField] private Transform achievementParent;
        private string _leftSelectedId;
        private string _rightSelectedId;
        IEnumerator Start()
        {
            GameInputMgr.Instance.RegisterLeftTrigger(OnLeftTriggerPerformed, OnLeftTriggerCanceled);
            GameInputMgr.Instance.RegisterRightTrigger(OnRightTriggerPerformed, OnRightTriggerCanceled);
            yield return new WaitForSeconds(1);
            SetAchievementItem();
        }

        private void OnDestroy()
        {
            if (GameInputMgr.Instance)
            {
                GameInputMgr.Instance.UnRegisterLeftTrigger(OnLeftTriggerPerformed, OnLeftTriggerCanceled);
                GameInputMgr.Instance.UnRegisterRightTrigger(OnRightTriggerPerformed, OnRightTriggerCanceled);
            }
        }

        private void OnRightTriggerCanceled(InputAction.CallbackContext obj)
        {
 
        }

        private void OnRightTriggerPerformed(InputAction.CallbackContext obj)
        {
            if (string.IsNullOrEmpty(_rightSelectedId)) return;
            GameEventArg arg = EventDispatcher.Instance.GetEventArg((int)EventID.SelectAchievement);
            arg.SetArg(0, _rightSelectedId);
            EventDispatcher.Instance.Dispatch((int)EventID.SelectAchievement);
        }

        private void OnLeftTriggerCanceled(InputAction.CallbackContext obj)
        {

        }

        private void OnLeftTriggerPerformed(InputAction.CallbackContext obj)
        {
            if (string.IsNullOrEmpty(_leftSelectedId)) return;
            GameEventArg arg = EventDispatcher.Instance.GetEventArg((int)EventID.SelectAchievement);
            arg.SetArg(0, _leftSelectedId);
            EventDispatcher.Instance.Dispatch((int)EventID.SelectAchievement);
        }

        void SetAchievementItem()
        {
            var achievementIds = AchievementLogic.Instance.GetAllAchievementIds();
            for (int i = 0; i < achievementParent.childCount; i++)
            {
                if (i < achievementIds.Count)
                    achievementParent.GetChild(i).GetComponent<Achievement3DItem>().InitItem(
                        achievementIds[i], OnLeftSelected, OnRightSelected);
                else
                    achievementParent.GetChild(i).GetComponent<Achievement3DItem>().InitItem();
            }
        }

        private void OnRightSelected(string id)
        {
            _rightSelectedId = id;
        }

        private void OnLeftSelected(string id)
        {
            _leftSelectedId = id;
        }
    }
}