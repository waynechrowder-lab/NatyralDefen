using Gameplay.Script.Logic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.Gameplay
{
    public class SmoothCylindricalUI : MonoBehaviour
    {
        [SerializeField] private Transform center;
        [SerializeField] float radius = 2f;
        [SerializeField] float rotationSpeed = 5f;
        [SerializeField] private float stopValue = 6;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text timeCountDown;
        [SerializeField] TMP_Text timerText;
        [SerializeField] TMP_Text prepareCountDown;
        [SerializeField] private Material health;
        private ProtectPlantsGameplay _instance;
        Transform _userCamera;
        ZombieSystem _zombieSystem;
        private void Start()
        {
            if (SceneLoadManager.Instance.GetActiveSceneIndex() 
                == (int)SceneLoadManager.SceneIndex.MRScene)
            {
                _instance = ((ProtectPlantsGameplay)ProtectPlantsGameplay.Instance);
                _zombieSystem = ZombieSystem.Instance;
            }
            _userCamera = Camera.main.transform;
            canvasGroup.alpha = 0;
            EventDispatcher.Instance.Register((int)EventID.GAMESTART, OnGameStart);
            EventDispatcher.Instance.Register((int)EventID.UPDATEJUNGLEHEALTH, OnUpdateHealth);
            var mode = GameLevelLogic.Instance.QuickGameMode;
            if (title)
            {
                if (mode == QuickGameMode.Normal)
                {
                    title.text = "抵御时长：";
                }
                else
                {
                    title.text = "剩余僵尸：";
                }
            }
            if (prepareCountDown) prepareCountDown.text = "";
            timerText.gameObject.SetActive(mode != QuickGameMode.Normal);
        }

        private void OnDestroy()
        {
            EventDispatcher.Instance.UnRegister((int)EventID.GAMESTART, OnGameStart);
            EventDispatcher.Instance.UnRegister((int)EventID.UPDATEJUNGLEHEALTH, OnUpdateHealth);
        }
        void Update()
        {
            if (SceneLoadManager.Instance.GetActiveSceneIndex()
                == (int)SceneLoadManager.SceneIndex.MRScene && timeCountDown)
            {
                if (GameLevelLogic.Instance.QuickGameMode == QuickGameMode.Normal)
                    timeCountDown.text = _instance.CurrentSecond.ToString("0");
                else
                {
                    timeCountDown.text = _zombieSystem.ZombieRemaining.ToString();
                    timerText.text = _instance.CurrentSecond.ToString("0");
                    if (prepareCountDown)
                    {
                        if (_zombieSystem.ZombieWaitTime > 0)
                        {
                            prepareCountDown.text = $"敌方还有{_zombieSystem.ZombieWaitTime}秒到达战场\n碾碎他们！";
                        }
                        else
                        {
                            prepareCountDown.text = "";
                        }
                    }
                }
            }
            var pos1 = center.position;
            var pos2 = _userCamera.position;
            var pos3 = transform.position;
            pos1.y = pos2.y = pos3.y = 0;
            var target = center.position + (pos2 - pos1).normalized * radius;
            if (Vector3.Angle((pos3 - pos1).normalized, (pos2 - pos1).normalized) < stopValue)
                return;
            transform.rotation = Quaternion.LookRotation(pos1 - pos3);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * rotationSpeed);
        }
        
        private void OnGameStart(GameEventArg arg)
        {
            canvasGroup.alpha = 1;
            health.SetFloat("_clip",1f);
        }
        
        private void OnUpdateHealth(GameEventArg arg)
        {
            var value = arg.GetArg<float>(0);
            health.SetFloat("_clip", Mathf.Clamp01(value));
            //health.value = Mathf.Clamp01(value);
        }

    }
}