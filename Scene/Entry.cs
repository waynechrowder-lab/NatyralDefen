using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Gameplay.Script.Logic;
using Pico.Platform.Models;
using TMPro;
using UnityEngine.Android;
using UnityEngine.SceneManagement;

namespace Gameplay.Script.Scene
{
    public class Entry : BaseScene
    {
        [SerializeField] private bool clearCache = false;
        protected override void Awake()
        {
            base.Awake();
            if (Application.platform == RuntimePlatform.WindowsEditor && clearCache)
                PlayerPrefs.DeleteAll();
        }

        protected override void Start()
        {
            PicoManager.SetVideoSeeThroughForLayer(true);
            Application.runInBackground = true;
            LoginLogic.Instance.Initialize();
            Invoke(nameof(OnEnterApplication), 1f);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CancelInvoke();
        }

        public override void OnLoadScene()
        {
            
        }

        public override void LoadAvatar()
        {
            
        }

        public override void OnUnloadScene()
        {
            
        }

        private AndroidJavaObject javaObj = null;
        private AndroidJavaObject GetJavaObject(){
            if (javaObj == null){
                javaObj = new AndroidJavaObject("com.pico.vrsdk.SDKApi");
            }
            return javaObj;
        }
        
        private void Call_SetUnityActivity(){
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            GetJavaObject().Call("setUnityActivity", jo);
        }
        
        void OnEnterApplication()
        {
            // if (Application.platform == RuntimePlatform.Android) {
            //     Call_SetUnityActivity ();
            // }
            // GetJavaObject().Call("requestExternalStorage");
            RequestStoragePermission();
            SceneLoadManager.Instance.OnLoadScene(SceneLoadManager.SceneIndex.Login);
        }
        
        void RequestStoragePermission()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }
        }
    }
}