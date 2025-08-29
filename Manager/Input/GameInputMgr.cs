using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace Gameplay.Scripts.Manager
{
    using Debug = UnityEngine.Debug;
    public class GameInputMgr : MonoSingleton<GameInputMgr>
    {
        [SerializeField] InputActionAsset gameInputAsset;
        [SerializeField] InputActionReference leftTriggerAction;
        [SerializeField] InputActionReference rightTriggerAction;
        // [SerializeField] InputActionReference leftGrabAction;
        // [SerializeField] InputActionReference rightGrabAction;
        [SerializeField] InputActionReference rightAAction;
        [SerializeField] InputActionReference rightBAction;
        [SerializeField] InputActionReference rightTouchPadAction;

        
        [SerializeField] InputActionReference leftTriggerValueAction;
        [SerializeField] InputActionReference rightTriggerValueAction;
        [SerializeField] InputActionReference leftGrabValueAction;
        [SerializeField] InputActionReference rightGrabValueAction;
        [SerializeField] InputActionReference leftXValueAction;
        [SerializeField] InputActionReference rightAValueAction;

        private UnityAction _abaAction = null;
        private float _abaTriggerTime = 0;
        private int _abaTriggerCount = -1;
        private bool _isPressedA;
        private float _aPressedTime;
        
        private bool _isPressedB;
        private float _bPressedTime;
        private void Start()
        {
            gameInputAsset.Enable();
            leftTriggerAction.action.Enable();
            rightTriggerAction.action.Enable();
            // leftGrabAction.action.Enable();
            // rightGrabAction.action.Enable();
            rightAAction.action.Enable();
            rightBAction.action.Enable();
            rightTouchPadAction.action.Enable();
            RegisterRightATrigger(PerformedA, CanceledA);
            RegisterRightBTrigger(PerformedB, CanceledB);
            GameObject origin = null;
            PXR_Plugin.System.FocusStateLost += () =>
            {
                origin = FindObjectOfType<XROrigin>().gameObject;
                if (origin)
                {
                    List<ActionBasedControllerManager> controllers =
                        origin.GetComponentsInChildren<ActionBasedControllerManager>().ToList();
                    if (controllers.Count > 0)
                    {
                        controllers.ForEach(value =>
                        {
                            // value.enableInputTracking = false;
                            // value.enabled = false;
                            // value.gameObject.SetActive(false);
                        });
                    }
                }
            };
            PXR_Plugin.System.FocusStateAcquired += () =>
            {
                if (origin)
                {
                    //XRBaseController
                    List<ActionBasedControllerManager> controllers = 
                        origin.GetComponentsInChildren<ActionBasedControllerManager>().ToList();
                    if (controllers.Count > 0)
                    {
                        controllers.ForEach(value =>
                        {
                            // value.enableInputTracking = true;
                            // value.enabled = true;
                            // value.gameObject.SetActive(true);
                        });
                    }
                }
            };
        }

        protected void OnDestroy()
        {
            UnRegisterRightATrigger(PerformedA, CanceledA);
            UnRegisterRightBTrigger(PerformedB, CanceledB);
        }

        private void CanceledA(InputAction.CallbackContext obj)
        {
            _isPressedA = false;
        }

        private void PerformedA(InputAction.CallbackContext obj)
        {
            _aPressedTime = 0;
            _isPressedA = true;
        }
        
        private void CanceledB(InputAction.CallbackContext obj)
        {
            _isPressedB = false;
        }

        private void PerformedB(InputAction.CallbackContext obj)
        {
            _bPressedTime = 0;
            _isPressedB = true;
        }


        private void Update()
        {
            if (_isPressedA)
            {
                _aPressedTime += Time.deltaTime;
                if (_aPressedTime > 5)
                {
                    _abaAction?.Invoke();
                    _isPressedA = false;
                    _aPressedTime = 0;
                }
            }
            
            if (_isPressedB)
            {
                _bPressedTime += Time.deltaTime;
                if (_bPressedTime > 5)
                {
                    _isPressedB = false;
                    _bPressedTime = 0;
                    Application.Quit();
                    Debug.Log("Application Quit");
                }
            }

            //if (rightAAction.action.WasPressedThisFrame() && _abaTriggerCount < 0)
            //{
            //    _abaTriggerCount = 0;
            //}

            //if (_abaTriggerCount >= 0)
            //{
            //    _abaTriggerTime += Time.deltaTime;
            //    if (rightBAction.action.WasPressedThisFrame())
            //    {
            //        _abaTriggerCount = 1;
            //    }

            //    if (rightAAction.action.WasPressedThisFrame() && _abaTriggerCount == 1)
            //    {
            //        _abaAction?.Invoke();
            //        _abaTriggerCount = -1;
            //        _abaTriggerTime = 0;
            //    }
            //}

            //if (_abaTriggerTime > 1)
            //{
            //    _abaTriggerCount = -1;
            //    _abaTriggerTime = 0;
            //}
        }

        public void RegisterFocusStateLost(Action action)
        {
            PXR_Plugin.System.FocusStateLost += action;
        }

        public void RegisterFocusStateAcquired(Action action)
        {
            PXR_Plugin.System.FocusStateAcquired += action;
        }
        
        public void UnRegisterFocusStateLost(Action action)
        {
            PXR_Plugin.System.FocusStateLost -= action;
        }

        public void UnRegisterFocusStateAcquired(Action action)
        {
            PXR_Plugin.System.FocusStateAcquired -= action;
        }

        public void RegisterLeftTrigger(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            leftTriggerAction.action.performed += performed;
            leftTriggerAction.action.canceled += canceled;
        }
        
        public void RegisterRightTrigger(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            rightTriggerAction.action.performed += performed;
            rightTriggerAction.action.canceled += canceled;
        }
        
        public void UnRegisterLeftTrigger(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            leftTriggerAction.action.performed -= performed;
            leftTriggerAction.action.canceled -= canceled;
        }
        
        public void UnRegisterRightTrigger(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            rightTriggerAction.action.performed -= performed;
            rightTriggerAction.action.canceled -= canceled;
        }
        
        // public void RegisterLeftGrab(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        // {
        //     leftGrabAction.action.performed += performed;
        //     leftGrabAction.action.canceled += canceled;
        // }
        //
        // public void RegisterRightGrab(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        // {
        //     rightGrabAction.action.performed += performed;
        //     rightGrabAction.action.canceled += canceled;
        // }
        //
        // public void UnRegisterLeftGrab(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        // {
        //     leftGrabAction.action.performed -= performed;
        //     leftGrabAction.action.canceled -= canceled;
        // }
        //
        // public void UnRegisterRightGrab(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        // {
        //     rightGrabAction.action.performed -= performed;
        //     rightGrabAction.action.canceled -= canceled;
        // }

        public void RegisterRightATrigger(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            rightAAction.action.performed += performed;
            rightAAction.action.canceled += canceled;
        }

        public void UnRegisterRightATrigger(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            rightAAction.action.performed -= performed;
            rightAAction.action.canceled -= canceled;
        }
        
        public void RegisterRightBTrigger(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            rightBAction.action.performed += performed;
            rightBAction.action.canceled += canceled;
        }
        
        public void UnRegisterRightBTrigger(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            rightBAction.action.performed -= performed;
            rightBAction.action.canceled -= canceled;
        }
        
        public void RegisterRightTouchPad(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            rightTouchPadAction.action.performed += performed;
            rightTouchPadAction.action.canceled += canceled;
        }

        public void UnRegisterRightTouchPad(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            rightTouchPadAction.action.performed -= performed;
            rightTouchPadAction.action.canceled -= canceled;
        }
        
        public void RegisterInput_ABA_Action(UnityAction action)
        {
            _abaAction += action;
        }
        
        public void UnRegisterInput_ABA_Action(UnityAction action)
        {
            _abaAction -= action;
        }

        #region 按键数值事件

        public void RegisterLeftGrabValueAction(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            leftGrabValueAction.action.performed += performed;
            leftGrabValueAction.action.canceled += canceled;
        }
        public void UnRegisterLeftGrabValueAction(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            leftGrabValueAction.action.performed -= performed;
            leftGrabValueAction.action.canceled -= canceled;
        }
        public void RegisterRightGrabValueAction(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            rightGrabValueAction.action.performed += performed;
            rightGrabValueAction.action.canceled += canceled;
        }
        public void UnRegisterRightGrabValueAction(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            rightGrabValueAction.action.performed -= performed;
            rightGrabValueAction.action.canceled -= canceled;
        }
        
        public void RegisterLeftTriggerValueAction(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            leftTriggerValueAction.action.performed += performed;
            leftTriggerValueAction.action.canceled += canceled;
        }
        public void UnRegisterLeftTriggerValueAction(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            leftTriggerValueAction.action.performed -= performed;
            leftTriggerValueAction.action.canceled -= canceled;
        }
        public void RegisterRightTriggerValueAction(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            rightTriggerValueAction.action.performed += performed;
            rightTriggerValueAction.action.canceled += canceled;
        }
        public void UnRegisterRightTriggerValueAction(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            rightTriggerValueAction.action.performed -= performed;
            rightTriggerValueAction.action.canceled -= canceled;
        }
        
        public void RegisterLeftXValueAction(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            leftXValueAction.action.performed += performed;
            leftXValueAction.action.canceled += canceled;
        }
        public void UnRegisterLeftXValueAction(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            leftXValueAction.action.performed -= performed;
            leftXValueAction.action.canceled -= canceled;
        }
        public void RegisterRightAValueAction(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            rightAValueAction.action.performed += performed;
            rightAValueAction.action.canceled += canceled;
        }
        public void UnRegisterRightAValueAction(Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled)
        {
            rightAValueAction.action.performed -= performed;
            rightAValueAction.action.canceled -= canceled;
        }
        
        #endregion
    }
}
