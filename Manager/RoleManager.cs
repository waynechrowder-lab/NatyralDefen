using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Currency.Core.Run;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using Gameplay.Script.UI;
using Pico.Avatar;
using Pico.Avatar.Sample;
using Script.Core.Tools;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR.Interaction.Toolkit;

namespace BeatNote.Scripts.Manager
{
    using Debug = UnityEngine.Debug;
    public class RoleManager : MonoSingle<RoleManager>
    {
        [SerializeField] private ActionAvatar localAvatar;
        [SerializeField] private AvatarIKSettingsSample avatarIKSettings;
        public ActionAvatar CurrentAvatar => localAvatar;
        private BodyTrackingDeviceInputReader _bodyTrackingDeviceInputReader;
        public BodyTrackingDeviceInputReader BodyTrackingDeviceInputReader => _bodyTrackingDeviceInputReader;
        private DeviceInputReaderBuilderInputType _inputType = DeviceInputReaderBuilderInputType.PicoXR;
        public DeviceInputReaderBuilderInputType InputType => _inputType;
        private bool _isLoading;

        private XROrigin _xrOrigin;

        public Vector3 RightHandleOffset { get; private set; }
        
        private Action _backApplication;
        public Action AvatarLoadedCallback;
        private void Start()
        {
            EventDispatcher.Instance.Register((int)EventID.NetworkUserLogin, OnUserLogin);
            // GameInputManager.Instance.AddFocusStateAcquiredCallback(OnFocusStateAcquired);
        }
        
        protected override void OnDestroy()
        {
            try
            {
                EventDispatcher.Instance.UnRegister((int)EventID.NetworkUserLogin, OnUserLogin);
                PicoAvatarManager.instance.RemoveAvatarChangeListener(AvatarChanged);
            }
            catch
            {
                // ignored
            }
            // GameInputManager.Instance.RemoveFocusStateAcquiredCallback(OnFocusStateAcquired);
            base.OnDestroy();
        }

        void OnFocusStateAcquired()
        {
            // if ((int)SceneLoadManager.SceneIndex.Audio == SceneLoadManager.Instance.GetActiveSceneIndex() &&
            //     !_isLoading)
            // {
            //     LoadAvatar(true, null, _inputType);
            // }
        }

        public void AddAvatarChangedListener()
        {
            PicoAvatarManager.instance.AddAvatarChangeListener(AvatarChanged);     
        }

        void AvatarChanged(string data)
        {
            if(data == "0")
            {
                // 启动形象中心，可以隐藏应用自身的面板，避免二者重叠
    
            }
            else if(data == "1")
            {
                // 切换了 Avatar 形象，需要重新加载 Avatar
                // LoginLogic.Instance.SetAvatarId("1");
                // LoginLogic.Instance.UpdateUserAvatarId();
            }
            else if(data == "2")
            {
                // 更新了 Avatar 形象，需要重新加载 Avatar
                // LoginLogic.Instance.SetAvatarId("1");
                // LoginLogic.Instance.UpdateUserAvatarId();
            }
            else if(data == "3")
            {
                // 点击退出按钮，直接退出形象中心
                LoadAvatar(true, null, _inputType);
                _backApplication?.Invoke();
            }
            else if(data == "4")
            {
                // 保存形象变更并退出形象中心，需要重新加载 Avatar
                // LoginLogic.Instance.SetAvatarId("1");
                // LoginLogic.Instance.UpdateUserAvatarId();
                _backApplication?.Invoke();
            }
            else if(data == "5")
            {
                // 不保存形象变更，并点击退出按钮退出形象中心
                LoadAvatar(true, null, _inputType);
                _backApplication?.Invoke();
            }
            else if(data == "6")
            {
                // 点击形象中心 UI 以外的区域，形象中心退到后台
                LoadAvatar(true, null, _inputType);
                _backApplication?.Invoke();
            }
            else
            {
                Debug.LogError($"AvatarChangedUnknownCallback{data}");
            }
        }

        private void OnUserLogin(GameEventArg arg)
        {
            // LoadAvatarList();
            // CreateRobot();
        }

        public void LoadAvatar(bool forceLoad, Action<ActionAvatar> callback, 
            DeviceInputReaderBuilderInputType inputType)
        {
            _isLoading = true;
            StopAllCoroutines();
            if (SceneLoadManager.Instance.GetActiveSceneIndex() < (int)SceneLoadManager.SceneIndex.Lobby) return;
            // if (string.IsNullOrEmpty(UserDataManager.Instance.UserTempData.avatarId)) return;
            if (!avatarIKSettings) avatarIKSettings = FindObjectOfType<AvatarIKSettingsSample>();

            localAvatar.loadedFinishCall = avatar =>
            {
                // avatar.Avatar.entity.
                avatar.gameObject.SetActive(true);
                callback?.Invoke(avatar);
                AvatarLoadedCallback?.Invoke();
            };
            var xROrigin = avatarIKSettings.XRRoot.GetComponentInChildren<XROrigin>();
            if (inputType == DeviceInputReaderBuilderInputType.BodyTracking && 
                _inputType != inputType)
            {
                xROrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;
                xROrigin.transform.rotation = Quaternion.identity;
                xROrigin.CameraFloorOffsetObject.transform.localRotation = Quaternion.identity;
            }
            
            localAvatar.transform.SetPositionAndRotation(xROrigin.transform.position,
                xROrigin.transform.rotation);
            localAvatar.SetAvatarIKSettings(avatarIKSettings);
            localAvatar.deviceInputReaderType = inputType;
            // bool activeAvatar = (SceneLoadManager.SceneIndex)SceneLoadManager.Instance.GetActiveSceneIndex() != 
            ActiveAvatar(true);
            forceLoad = forceLoad || _inputType != inputType;
            _inputType = inputType;
            if (forceLoad)
                StartCoroutine(StartLoadAvatar());
            else
                StartReLoadAvatar();
        }

        // public void InitAvatarForceHandPose(params IDeviceInputReader.ControllerButtons[] btns)
        // {
        //     PicoAvatarManager.instance.ClearForceHandButtons();
        //     PicoAvatarManager.instance.AddButtonIndex(btns);
        // }
        //
        // public void ClearAvatarForceHandPose()
        // {
        //     PicoAvatarManager.instance.ClearForceHandButtons();
        // }

        public void ActiveAvatar(bool active) => localAvatar.gameObject.SetActive(active);
        
        IEnumerator StartLoadAvatar(Action callback = null)
        {
            string uId = UserData.Instance.CurrentUser.relevanceId;
            string avatarId = "1";//UserDataManager.Instance.UserTempData.avatarId;
            string userId;
            if (avatarId == "1")
            {
                avatarId = string.Empty;
                userId = uId;
            }
            else
                userId = string.Empty;
            localAvatar.StartAvatar(userId, null, avatarId);
            while (!localAvatar.Avatar.isAnyEntityReady)
                yield return null;
            callback?.Invoke();
            SetLocalAvatar();
        }

        void StartReLoadAvatar()
        {
            localAvatar.SetAvatarIKSettings(avatarIKSettings);
            localAvatar.resetXrRoot();
            // localAvatar.transform.SetPositionAndRotation(avatarIKSettings.transform.parent.position,
            //     avatarIKSettings.transform.parent.rotation);
            SetLocalAvatar();
        }

        void SetLocalAvatar()
        {
            _isLoading = false;
            localAvatar.Avatar.SetLayer(LayerMask.NameToLayer("Role"));
            float avatarHeight = 1.75f;
            // if (GameSettings.Instance.gameSetup.avatarHeight > 0)
            //     avatarHeight = GameSettings.Instance.gameSetup.avatarHeight;
            avatarIKSettings.XRRoot.GetComponentInChildren<XROrigin>().CameraYOffset = avatarHeight;
            avatarHeight -= .16f;
            localAvatar.Avatar.entity.bodyAnimController.SetAvatarHeight(avatarHeight, false);

            avatarHeight += .1f;
            var pos = avatarIKSettings.heightAutoFit.cameraOffsetTarget.localPosition;
            pos.y = avatarHeight;
            avatarIKSettings.heightAutoFit.cameraOffsetTarget.localPosition = pos;
            
            avatarIKSettings.isDirty = true;
            Renderer[] renderers = localAvatar.GetComponentsInChildren<Renderer>(true);
            renderers.ToList().ForEach(value =>
            {
                if (value.name.ToLower().Contains("face"))
                    value.gameObject.layer = LayerMask.NameToLayer("Face");
            });
            IDeviceInputReader deviceInputReader = localAvatar.Avatar.entity.deviceInputReader;
            if (deviceInputReader is BodyTrackingDeviceInputReader)
            {
                avatarIKSettings.heightAutoFit.enableAutoFitHeight = false;
                PXR_MotionTracking.StartBodyTracking(BodyTrackingMode.BTM_FULL_BODY_HIGH, new BodyTrackingBoneLength());
                _bodyTrackingDeviceInputReader = (BodyTrackingDeviceInputReader)deviceInputReader;
                _bodyTrackingDeviceInputReader.FitGround();
            }
            else PXR_MotionTracking.StopBodyTracking();
        }
        
        // public void SetAvatarHeight(float offset)
        // {
        //     float avatarHeight = GameSettings.Instance.gameSetup.avatarHeight;// + offset;
        //     avatarIKSettings.XRRoot.GetComponentInChildren<XROrigin>().CameraYOffset = avatarHeight;
        //     avatarHeight -= .1f;
        //     localAvatar.Avatar.entity.bodyAnimController.SetAvatarHeight(avatarHeight);
        // }
        
        public void OpenPicoAvatarCenter(Action backApplication = null)
        {
            _backApplication = backApplication;
            if (!LoginLogic.Instance.UserPermissions.Contains("avatar")
                && Application.platform != RuntimePlatform.WindowsEditor)
            {
                PicoPlatformService.Instance.GetUserAvatarPermissions(error =>
                {
                    if (string.IsNullOrEmpty(error))
                    {
                        PicoAvatarManager.instance.StartAvatarEditor("com.unity.AvatarDemo", b => 
                        { 
                            if (b) 
                                Debug.Log("TestClick avatareditor success"); 
                            else 
                                Debug.LogError("TestClick avatareditor failure"); 
                        }); 
                    }
                    else
                        Debug.LogError(error);
                });
                return;
            }
            PicoAvatarManager.instance.StartAvatarEditor("com.unity.AvatarDemo", b => 
            { 
                if (b) 
                    Debug.Log("TestClick avatareditor success"); 
                else 
                    Debug.LogError("TestClick avatareditor failure"); 
            }); 
        }

        public ActionAvatar LoadAvatar(string playerId, string msg)
        {
            AvatarCapabilities capabilities = new AvatarCapabilities();
            capabilities.manifestationType = AvatarManifestationType.Full;
            capabilities.controlSourceType = ControlSourceType.OtherPlayer;
            capabilities.inputSourceType = DeviceInputReaderBuilderInputType.RemotePackage;
            capabilities.recordBodyAnimLevel = RecordBodyAnimLevel.FullBone;
            capabilities.autoStopIK = true;
            capabilities.bodyCulling = true;

            var remoteAvatar = new GameObject($"RemoteActionAvatar-{playerId}").AddComponent<ActionAvatar>();
            remoteAvatar.AddComponent<DontDestroyOnLoad>();
            remoteAvatar.StartRemoteAvatar(playerId, null,"", capabilities);

            remoteAvatar.criticalJoints = new JointType[] { JointType.Head };

            return remoteAvatar;
        }
    }
    
}