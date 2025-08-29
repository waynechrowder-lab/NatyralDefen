using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using Gameplay.Script.MiniWorldGameplay;
using Gameplay.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Script.UI
{
    public class GameUI : BasedUI
    {
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject failedPanel;
        [SerializeField] private GameObject successPanel;

        [SerializeField] private Transform successAwardParent;
        [SerializeField] private Transform failedAwardParent;
        
        [SerializeField] private TMP_Text currentTimeUsage;
        [SerializeField] private TMP_Text bestTimeUsage;

        protected override void OnEnable()
        {
            base.OnEnable();
            tutorialPanel.SetActive(false);
            pausePanel.SetActive(false);
            failedPanel.SetActive(false);
            successPanel.SetActive(false);
            GameInputMgr.Instance.RegisterRightBTrigger(OnBPerformed, OnBCanceled);
            EventDispatcher.Instance.Register((int)EventID.GAMEOVER, OnGameOver);
            InvokeRepeating(nameof(RegisterGameplay), 0, .5f);
        }

        void RegisterGameplay()
        {
            if (!GameplayMgr.Instance) return;
            GameplayMgr.Instance.RegisterGameplayStateChange(OnGameplayStateChanged);
            CancelInvoke(nameof(RegisterGameplay));
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            GameInputMgr.Instance.UnRegisterRightBTrigger(OnBPerformed, OnBCanceled);
            EventDispatcher.Instance.UnRegister((int)EventID.GAMEOVER, OnGameOver);
            if (GameplayMgr.Instance)
                GameplayMgr.Instance.UnRegisterGameplayStateChange(OnGameplayStateChanged);
        }

        private void OnBCanceled(InputAction.CallbackContext obj)
        {
            
        }

        private void OnBPerformed(InputAction.CallbackContext obj)
        {
            if (GameplayMgr.Instance.GameplayState == GameplayState.Gaming)
            {
                GameplayMgr.Instance.SetGameplayState(GameplayState.Pause);
                pausePanel.SetActive(true);
            }
            else if (GameplayMgr.Instance.GameplayState == GameplayState.Pause)
            {
                GameplayMgr.Instance.SetGameplayState(GameplayState.Gaming);
                pausePanel.SetActive(false);
            }
        }
        
        private void OnGameOver(GameEventArg arg)
        {
            GameplayLogic.Instance.Save2UserStore();
            bool success = arg.GetArg<bool>(0);
            if (success)
            {
                successPanel.SetActive(true);
                float gameTime = GameplayLogic.Instance.GameTimer;
                currentTimeUsage.text = $"本次用时：{gameTime / 60:00}分{gameTime % 60:00}秒";
                bestTimeUsage.text = $"最佳记录：{gameTime / 60:00}{gameTime % 60:00}";
                SetAwards(successAwardParent);
            }
            else
            {
                failedPanel.SetActive(true);
                SetAwards(failedAwardParent);
            }
        }

        void SetAwards(Transform awardTrans)
        {
            var awards = new List<Data.GoodsItem>(GameplayLogic.Instance.AwardGoods);
            var coinCount = GameplayLogic.Instance.GameLevelGetCoin;
            bool hasCoinGoods = false;
            for (int i = 0; i < awards.Count; i++)
            {
                if (awards[i].goodsId.Equals("ITEM_001"))
                {
                    awards[i].goodsNum += coinCount;
                    hasCoinGoods = true;
                    break;
                }
            }
            if (!hasCoinGoods)
                awards.Add(new Data.GoodsItem("ITEM_001", coinCount));
            GameObject awardObj = awardTrans.GetChild(0).gameObject;
            for (int i = 0; i < awardTrans.childCount; i++)
            {
                GameObject item = awardTrans.GetChild(i).gameObject;
                if (i < awards.Count)
                {
                    item.GetComponent<GameLevelAwardItem>().InitItem(awards[i]);
                    item.SetActive(true);
                }
                else
                    item.SetActive(false);
            }

            for (int i = awardTrans.childCount; i < awards.Count; i++)
            {
                GameObject item = Instantiate(awardObj, awardTrans);
                item.GetComponent<GameLevelAwardItem>().InitItem(awards[i]);
                item.SetActive(true);
            }
        }

        private void OnGameplayStateChanged(GameplayState last, GameplayState current)
        {
            if (current == GameplayState.Tutorial)
            {
                tutorialPanel.SetActive(true);
            }
            else if (current == GameplayState.Gaming)
            {
                tutorialPanel.SetActive(false);
            }
        }
        
        public void OnClickBack()
        {
            if (MultiplayerLogic.Instance.IsHost())
            {
                MultiplayerLogic.Instance.QuitMultiplayerGame();
                return;
            }
            SceneLoadManager.Instance.OnLoadScene(SceneLoadManager.SceneIndex.Lobby);
        }

        public void OnClickUnPause()
        {
            OnBPerformed(new InputAction.CallbackContext());
        }

        public void OnClickReStart()
        {
            pausePanel.SetActive(false);
            failedPanel.SetActive(false);
            successPanel.SetActive(false);
            EventDispatcher.Instance.Dispatch((int)EventID.GAMERESTART);
        }

        public void OnClickConfirm()
        {
            PlayerPrefs.SetInt("Tutorial", 1);
            GameplayMgr.Instance.SetGameplayState(GameplayState.Gaming);
        }
    }
}