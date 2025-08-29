using System;
using System.Collections;
using BeatNote.Scripts.Manager;
using Gameplay.Script.Logic;
using Pico.Avatar;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class LoginUI : BasedUI
    {
        [SerializeField] private TMP_Text debugText;
        private bool _isClickLogin = false;
        [SerializeField] private Button loginButton;
        [SerializeField] private CanvasGroup tips;
        private float _tipsTimer;
        private void Update()
        {
            debugText.text = LoginLogic.Instance.LoginDebug;
            _tipsTimer -= Time.deltaTime;
            _tipsTimer = Mathf.Clamp01(_tipsTimer);
            tips.alpha = _tipsTimer;
        }

        public void OnGetUserPermission(bool approve)
        {
            loginButton.interactable = approve;
            if (approve)
            {
                if (PlayerPrefs.HasKey("autoEnter"))
                {
                    OnClickLogin();
                }
            }
            else
            {
                if (PlayerPrefs.HasKey("autoEnter"))
                {
                    PlayerPrefs.DeleteKey("autoEnter");
                }
            }
                
        }

        public void OnClickLogin()
        {
            if (!CheckInternetConnection())
            {
                _isClickLogin = false;
                OnShowTips("网络连接异常");
                return;
            }
            if (_isClickLogin)
                return;
            _isClickLogin = true;
            LoginLogic.Instance.Login(OnShowTips);
            StartCoroutine(nameof(CheckUserLogin));
        }

        public bool CheckInternetConnection()
        {
            // 检查网络连接状态
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("无网络连接");
                return false;
            }
            else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                Debug.Log("使用移动数据网络");
                return true;
            }
            else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                Debug.Log("使用WiFi或以太网");
                return true;
            }
    
            return false;
        }
        
        void OnShowTips(string message)
        {
            _tipsTimer = 2;
            tips.GetComponentInChildren<TMP_Text>().text = message;
        }

        IEnumerator CheckUserLogin()
        {
            float timer = 0;
            while (!LoginLogic.Instance.PlatformInitialized && timer < 3)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (timer > 3)
            {
                OnShowTips("Pico云服务初始化超时，2s后进入游戏");
            }

            yield return new WaitForSeconds(2f);
            // PicoAvatarApp.instance.StartApp();
            // while (!PicoAvatarApp.isWorking)
            //     yield return null;
            // PicoAvatarAppStart();
            // while (!PicoAvatarManager.canLoadAvatar)
            //     yield return null;
            // RoleManager.Instance.AddAvatarChangedListener();
            EnterGame();
        }
        
        void PicoAvatarAppStart()
        {
            var avatarApp = PicoAvatarApp.instance;
            avatarApp.loginSettings.accessToken = LoginLogic.Instance.AccessToken;
            avatarApp.StartAvatarManager();
        }
        
        void EnterGame()
        {
            PlayerPrefs.SetInt("autoEnter", 1);
            SceneLoadManager.Instance.OnLoadSceneAsync(
                SceneLoadManager.SceneIndex.Lobby, true);
        }
    }
}