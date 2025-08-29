using System;
using System.Collections.Generic;
using Currency.Core.Run;
using Gameplay.Script.ConstValue;
using Gameplay.Script.Data;
using Gameplay.Script.Manager;
using MicroWar.Platform;
using Pico.Platform;
using Pico.Platform.Models;
using UnityEngine;

namespace Gameplay.Script.Logic
{
    public class LoginLogic : Single<LoginLogic>
    { 
        public string LoginError { get; private set; }
        
        public string LoginDebug { get; private set; }
        public string AccessToken { get; private set; } = PicoPlatformConstValue.APPTOKEN;
        public bool PlatformInitialized { get; private set; }
        public List<string> UserPermissions { get; private set; } = new();
        
        public string CharacterId { get; set; }
    
        public string AvatarId { get; set; }

        public void Initialize()
        {
            LoginDebug = "初始化";
            try
            {
                CoreService.AsyncInitialize().OnComplete(m =>
                {
                    if (m.IsError)
                    {
                        Debug.LogError($"Async initialize failed: code={m.GetError().Code} message={m.GetError().Message}");
                        return;
                    }

                    if (m.Data != PlatformInitializeResult.Success && m.Data != PlatformInitializeResult.AlreadyInitialized)
                    {
                        Debug.LogError($"Async initialize failed: result={m.Data}");
                        return;
                    }

                    LoginDebug += "\n初始化成功";
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"Async Initialize Failed:{e}");
            }
        }
        
        public void Login(Action<string> callback)
        {
            LoginDebug += "\n异步初始化";
            PicoPlatformService.Instance.PicoServiceAsyncInitialize((error, num) =>
            {
                if (string.IsNullOrEmpty(error))
                {
                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        // GameEventArg arg = EventDispatcher.Instance.GetEventArg((int)EventID.NetworkUserLogin);
                        // arg.SetArg(0, true);
                        // EventDispatcher.Instance.Dispatch((int)EventID.NetworkUserLogin);
                        // StartGameInitialize();
                        // return;
                        LoginDebug += "\n异步初始化成功";
                        LoginDebug += "\n用户登录";
                        PicoPlatformService.Instance.UserLogin((error1, user) =>
                        {
                            if (string.IsNullOrEmpty(error1))
                            {
                                LoginDebug += "\n用户登录成功";
                                SetUser(user);
                                GameEventArg arg = EventDispatcher.Instance.GetEventArg((int)EventID.NetworkUserLogin);
                                arg.SetArg(0, true);
                                EventDispatcher.Instance.Dispatch((int)EventID.NetworkUserLogin);
                                StartGameInitialize();
                            }
                            else
                            {
                                LoginError += $"用户登录失败：{error1}";
                                callback?.Invoke($"用户登录失败：{error1}");
                                Debug.LogError(LoginError);
                            }
                        });
                        return;
                    }

                    LoginDebug += "\n异步初始化成功";
                    LoginDebug += "\n获取用户权限";
                    PicoPlatformService.Instance.GetUserPermissions(s =>
                    {
                        if (string.IsNullOrEmpty(s))
                        {
                            LoginDebug += "\n获取用户权限成功";
                            LoginDebug += "\n用户登录";
                            PicoPlatformService.Instance.UserLogin((error1, user) =>
                            {
                                if (string.IsNullOrEmpty(error1))
                                {
                                    LoginDebug += "\n用户登录成功";
                                    GetUserPermissionsRequested(callback);
                                    SetUser(user);
                                    GameEventArg arg = EventDispatcher.Instance.GetEventArg((int)EventID.NetworkUserLogin);
                                    arg.SetArg(0, true);
                                    EventDispatcher.Instance.Dispatch((int)EventID.NetworkUserLogin);
                                    StartGameInitialize();
                                }
                                else
                                {
                                    LoginError += $"用户登录失败：{error1}";
                                    callback?.Invoke($"用户登录失败：{error1}");
                                    Debug.LogError(LoginError);
                                }
                            });
                        }
                        else
                        {
                            LoginError += $"获取用户授权失败：{s}";
                            callback?.Invoke($"获取用户授权失败：{s}");
                            Debug.LogError(LoginError);
                        }
                    });
                    PicoPlatformService.Instance.EntitlementCheck((s, message) =>
                    {
                        if (string.IsNullOrEmpty(s))
                        {

                        }
                        else
                        {
                            LoginError += $"权限验证失败，请从Pico商店获取该应用：{s}";
                            callback?.Invoke($"权限验证失败，请从Pico商店获取该应用：{s}");
                            Debug.LogError(LoginError);
                        }
                    });
                }
                else
                {
                    LoginError += $"Pico服务初始化失败：{error}";
                    callback?.Invoke($"Pico服务初始化失败：{error}");
                    Debug.LogError(LoginError);
                }
            });
        }
        
        public void GetUserPermissionsRequested(Action<string> callback)
        {
            PicoPlatformService.Instance.RequestedUserPermissions((error, strings) =>
            {
                if (string.IsNullOrEmpty(error) && strings != null)
                    UserPermissions.AddRange(strings);
                else
                {
                    callback?.Invoke($"用户授权失败：{error}");
                    Debug.LogError(error);
                }
            });
        }

        public void StartGameInitialize()
        {
            PlatformInitialized = false;
            LoginDebug += "\n游戏云服务初始化";
            UserService.GetAccessToken().OnComplete(m =>
            {
                if (m.IsError)
                    Debug.LogError(m.Error.Message);
                else
                    AccessToken = m.Data;
                PicoPlatformService.Instance.UnInitialize();
                PicoPlatformService.Instance.GameInitialize(AccessToken, error =>
                {
                    if (string.IsNullOrEmpty(error))
                    {
                        LoginDebug += "\n游戏云服务初始化成功";
                        PlatformInitialized = true;
                        PlatformServiceManager.Instance.DelayInit();
                        Debug.Log("GameInitialize Success !");
                    }
                    else
                    {
                        Debug.LogError($"GameInitialize Failed:{error} !");
                        StartGameInitialize();
                    }
                });
            });
        }

        public void SetUser(User user)
        {
            UserData.Instance.SetUserData(user);
        }
        
        public void SetCharacterId(string character_id)
        {
            CharacterId = character_id;
        }
        public void SetAvatarId(string avatarId)
        {
            AvatarId = avatarId;
        }

        public void UpdateUserAvatarId()
        {
            //UserDataManager.Instance.UpdateAvatarMsg(CharacterId, AvatarId);
        }
    }
}