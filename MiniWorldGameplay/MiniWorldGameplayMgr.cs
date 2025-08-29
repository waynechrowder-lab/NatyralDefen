using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Script.Data;
using Gameplay.Script.Manager;
using Gameplay.Script.TreasureHunter;
using TMPro;
using UnityEngine.UI;

namespace Gameplay.Script.MiniWorldGameplay
{
    public class MiniWorldGameplayMgr : GameplayMgr
    {
        [SerializeField] private GameLevelInherentData _gameLevelInherentData;
        [SerializeField] private List<GameLevelItemData> _gameLevelItemDatas;

        [SerializeField] Slider healthBar;
        [SerializeField] TMP_Text healthText;
        [SerializeField] TMP_Text waveText;
        private int _health, _originalHealth;
        protected override void Awake()
        {
            base.Awake();
            var canvas = FindObjectOfType<FaceToCamera>();
            if (canvas)
            {
                healthBar = canvas.GetComponentInChildren<Slider>();
                var texts = canvas.GetComponentsInChildren<TMP_Text>();
                if (texts is { Length: > 0 })
                    healthText = texts[0];
                if (texts is { Length: > 1 })
                    waveText = texts[1];
            }
            EventDispatcher.Instance.Register((int)EventID.KernelTakeDamage, OnKernelTakeDamage);
            EventDispatcher.Instance.Register((int)EventID.GAMERESTART, OnGameRestart);
            EventDispatcher.Instance.Register((int)EventID.GAMEOVER, OnGameOver);
            EventDispatcher.Instance.Register((int)EventID.GameWave, OnGameWave);
        }

        private void Start()
        {
            StartCoroutine(nameof(StartGamePlay));
        }
        
        protected void OnDestroy()
        {
            EventDispatcher.Instance.UnRegister((int)EventID.KernelTakeDamage, OnKernelTakeDamage);
            EventDispatcher.Instance.UnRegister((int)EventID.GAMERESTART, OnGameRestart);
            EventDispatcher.Instance.UnRegister((int)EventID.GAMEOVER, OnGameOver);
            EventDispatcher.Instance.UnRegister((int)EventID.GameWave, OnGameWave);
        }

        IEnumerator StartGamePlay()
        {
            SetGameplayState(GameplayState.None);
            (_gameLevelInherentData, _gameLevelItemDatas) = GameplayLogic.Instance.GetGameLevelData();
            _health = _originalHealth = _gameLevelInherentData.health;
            healthBar.value = 1;
            healthText.text = _health.ToString();
            yield return new WaitForSeconds(1f);
            if (!PlayerPrefs.HasKey("Tutorial"))
            {
                SetGameplayState(GameplayState.Tutorial);
                yield return new WaitUntil(() => GameplayState == GameplayState.Gaming);
            }
            else
                SetGameplayState(GameplayState.Gaming);
            GameplayLogic.Instance.StartGameplay();
        }

        private void OnKernelTakeDamage(GameEventArg arg)
        {
            if (GameplayState != GameplayState.Gaming) return;
            int value = arg.GetArg<int>(0);
            _health -= value;
            _health = Mathf.Max(0, _health);
            healthBar.value = (float)_health / _originalHealth;
            healthText.text = _health.ToString();
            if (_health <= 0)
            {
                GameEventArg arg1 = EventDispatcher.Instance.GetEventArg((int)EventID.GAMEOVER);
                arg1.SetArg(0, false);
                EventDispatcher.Instance.Dispatch((int)EventID.GAMEOVER);
            }
        }
        
        private void OnGameRestart(GameEventArg arg)
        {
            StopAllCoroutines();
            StartCoroutine(nameof(StartGamePlay));
        }
        
        private void OnGameOver(GameEventArg arg)
        {
            SetGameplayState(GameplayState.Finish);
        }
        
        private void OnGameWave(GameEventArg arg)
        {
            string waveStr = arg.GetArg<string>(0);
            waveText.text = waveStr;
        }
    }
}