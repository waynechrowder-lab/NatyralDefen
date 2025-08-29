using System;
using Gameplay.Script.Gameplay;
using Gameplay.Script.Logic;
using UnityEngine;

namespace Gameplay.Script.UI
{
    public class GameLevelBagUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameLevelPlantSelectUI;
        [SerializeField] GameObject plantIntroductionUI;
        [SerializeField] private Transform bagParent;
        [SerializeField] private CanvasGroup tips;
        [SerializeField] GameObject modeNormal;
        [SerializeField] GameObject modeQuick;
        private float _tipShowTime;
        QuickGameMode _quickMode = QuickGameMode.Normal;
        private void OnEnable()
        {
            gameLevelPlantSelectUI.SetActive(false);
            plantIntroductionUI.SetActive(false);
            _quickMode = GameLevelLogic.Instance.QuickGameMode;
            SetBagItem();
            GameLevelLogic.Instance.RegisterOnBagItemChanged(OnBagItemChanged);
            modeNormal.SetActive(_quickMode == QuickGameMode.Normal);
            modeQuick.SetActive(_quickMode != QuickGameMode.Normal);
            //EventDispatcher.Instance.Register((int)EventID.BagItemChanged, OnBagItemChanged);
        }

        private void OnDisable()
        {
            GameLevelLogic.Instance.UnRegisterOnBagItemChanged(OnBagItemChanged);
            //EventDispatcher.Instance.UnRegister((int)EventID.BagItemChanged, OnBagItemChanged);
        }

        private void Update()
        {
            if (_tipShowTime > 0)
            {
                _tipShowTime -= Time.deltaTime;
                tips.alpha = _tipShowTime;
            }
        }

        void OnBagItemChanged()
        {
            SetBagItem();
        }

        void SetBagItem()
        {
            var gameBags = GameLevelLogic.Instance.GetUserGameBags();
            int count = gameBags?.Count ?? 0;
            for (int i = 0; i < bagParent.childCount; i++)
            {
                var item = bagParent.GetChild(i).gameObject;
                if (i < count)
                {
                    item.GetComponent<GameBagItem>().InitItem(false, gameBags[i], OnClickBag);
                }
                else
                {
                    item.GetComponent<GameBagItem>().InitItem(false, -1, OnClickBag);
                }
            }
        }

        void OnClickBag(int userPlantId, int index)
        {
            GameLevelLogic.Instance.SetIndex(index);
            Debug.Log($"Set Bag Index {index}");
            if (userPlantId == -1)
                gameLevelPlantSelectUI.SetActive(true);
            else
                GameLevelLogic.Instance.RemoveBagPlant(userPlantId);
        }
        
        public void OnClickReady()
        {
            gameObject.SetActive(false);
        }
        
        public void OnClickStartMRGameLevel()
        {
            bool b = GameLevelLogic.Instance.StartGameLevel(true);
            if (!b)
            {
                _tipShowTime = 3;
            }
        }

        public void OnClickBack()
        {
            gameObject.SetActive(false);
        }
    }
}