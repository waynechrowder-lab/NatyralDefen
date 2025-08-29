using System;
using System.Linq;
using BNG;
using Gameplay.Scripts.Manager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Script
{
    public class CustomHandInputMgr : MonoBehaviour
    {
        [SerializeField] private HandPoseBlender leftBlender;
        [SerializeField] private HandPoseBlender rightBlender;

        private void Awake()
        {
            var blenders = FindObjectsOfType<HandPoseBlender>();
            if (blenders is { Length: > 0 })
                blenders.ToList().ForEach(value =>
                {
                    if (value.gameObject.name.ToLower().Contains("left"))
                        leftBlender = value;
                    else if (value.gameObject.name.ToLower().Contains("right"))
                        rightBlender = value;
                });
        }

        private void Start()
        {
            GameInputMgr.Instance.RegisterLeftGrabValueAction(OnLeftGrabPerformed, OnLeftGrabCanceled);
            GameInputMgr.Instance.RegisterLeftTriggerValueAction(OnLeftTriggerPerformed, OnLeftTriggerCanceled);
            GameInputMgr.Instance.RegisterLeftXValueAction(OnLeftXPerformed, OnLeftXCanceled);
            
            GameInputMgr.Instance.RegisterRightGrabValueAction(OnRightGrabPerformed, OnRightGrabCanceled);
            GameInputMgr.Instance.RegisterRightTriggerValueAction(OnRightTriggerPerformed, OnRightTriggerCanceled);
            GameInputMgr.Instance.RegisterRightAValueAction(OnRightAPerformed, OnRightACanceled);
        }

        private void OnDestroy()
        {
            GameInputMgr.Instance.UnRegisterLeftGrabValueAction(OnLeftGrabPerformed, OnLeftGrabCanceled);
            GameInputMgr.Instance.UnRegisterLeftTriggerValueAction(OnLeftTriggerPerformed, OnLeftTriggerCanceled);
            GameInputMgr.Instance.UnRegisterLeftXValueAction(OnLeftXPerformed, OnLeftXCanceled);
            
            GameInputMgr.Instance.UnRegisterRightGrabValueAction(OnRightGrabPerformed, OnRightGrabCanceled);
            GameInputMgr.Instance.UnRegisterRightTriggerValueAction(OnRightTriggerPerformed, OnRightTriggerCanceled);
            GameInputMgr.Instance.UnRegisterRightAValueAction(OnRightAPerformed, OnRightACanceled);
        }

        private void OnLeftTriggerCanceled(InputAction.CallbackContext obj) => leftBlender.IndexValue = 0;
        private void OnLeftTriggerPerformed(InputAction.CallbackContext obj) => leftBlender.IndexValue = obj.ReadValue<float>();
        private void OnLeftGrabCanceled(InputAction.CallbackContext obj) => leftBlender.GripValue = 0;
        private void OnLeftGrabPerformed(InputAction.CallbackContext obj) => leftBlender.GripValue = obj.ReadValue<float>();
        private void OnLeftXCanceled(InputAction.CallbackContext obj) => leftBlender.ThumbValue = 0;
        private void OnLeftXPerformed(InputAction.CallbackContext obj) => leftBlender.ThumbValue = obj.ReadValue<float>();
        
        private void OnRightTriggerCanceled(InputAction.CallbackContext obj) => rightBlender.IndexValue = 0;
        private void OnRightTriggerPerformed(InputAction.CallbackContext obj) => rightBlender.IndexValue = obj.ReadValue<float>();
        private void OnRightGrabCanceled(InputAction.CallbackContext obj) => rightBlender.GripValue = 0;
        private void OnRightGrabPerformed(InputAction.CallbackContext obj) => rightBlender.GripValue = obj.ReadValue<float>();
        private void OnRightACanceled(InputAction.CallbackContext obj) => rightBlender.ThumbValue = 0;
        private void OnRightAPerformed(InputAction.CallbackContext obj) => rightBlender.ThumbValue = obj.ReadValue<float>();
    }
}