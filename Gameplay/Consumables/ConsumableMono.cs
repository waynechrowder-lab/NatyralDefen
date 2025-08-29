using System;
using Gameplay.Scripts.Manager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Script.Gameplay
{
    public abstract class ConsumableMono : MonoBehaviour
    {
        protected bool _triggerPerformed;
        protected int _cost;
        protected virtual void Start()
        {
            GameInputMgr.Instance.RegisterRightTrigger(OnTriggerPerformed, OnTriggerCanceled);
            GameInputMgr.Instance.RegisterRightBTrigger(OnBPerformed, OnBCanceled);
        }

        private void OnBCanceled(InputAction.CallbackContext obj)
        {
            OnBPerformed();
        }

        protected virtual void OnBPerformed()
        {
            
        }

        private void OnBPerformed(InputAction.CallbackContext obj)
        {

        }

        private void OnDestroy()
        {
            if (GameInputMgr.Instance)
            {
                GameInputMgr.Instance.UnRegisterRightTrigger(OnTriggerPerformed, OnTriggerCanceled);
                GameInputMgr.Instance.UnRegisterRightBTrigger(OnBPerformed, OnBCanceled);
            }
        }
        
        private void OnTriggerCanceled(InputAction.CallbackContext obj)
        {
            OnCanceled();
        }

        private void OnTriggerPerformed(InputAction.CallbackContext obj)
        {
            OnPerformed();
        }

        protected virtual void OnPerformed()
        {
            _triggerPerformed = true;
        }

        protected virtual void OnCanceled()
        {
            _triggerPerformed = false;
        }

        public void SetCost(int cost)
        {
            _cost = cost;
        }
    }
}