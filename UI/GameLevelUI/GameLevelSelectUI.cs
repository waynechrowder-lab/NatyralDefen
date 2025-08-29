using System.Collections.Generic;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using UnityEngine;

namespace Gameplay.Script.UI
{
    public class GameLevelSelectUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameLevelSelectedUI;
        [SerializeField] private Transform gameLevelParent;
        private GameObject _gameLevelItem;
        private void OnEnable()
        {
            SetGameLevelItem();
        }

        void SetGameLevelItem()
        {
            if (!_gameLevelItem)
            {
                _gameLevelItem = gameLevelParent.GetChild(0).gameObject;
                _gameLevelItem.SetActive(false);
            }
            var gameLevels = GameLevelLogic.Instance.GetSingleGameLevels();
            var temp = new List<string>();
            temp.Add("MR");
            temp.AddRange(gameLevels);
            gameLevels = temp;
            int count = gameLevels?.Count ?? 0;
            //todo:配置剩余关卡
            count = Mathf.Min(count, 3);
            for (int i = 0; i < gameLevelParent.childCount; i++)
            {
                var item = gameLevelParent.GetChild(i).gameObject;
                if (i < count)
                {
                    item.GetComponent<GameLevelItem>().InitItem(gameLevels[i], OnLevelSelect);
                    item.SetActive(true);
                }
                else
                    item.SetActive(false);
            }

            for (int i = gameLevelParent.childCount; i < count; i++)
            {
                var item = Instantiate(_gameLevelItem, gameLevelParent);
                item.GetComponent<GameLevelItem>().InitItem(gameLevels[i], OnLevelSelect);
                item.SetActive(true);
            }
        }

        void OnLevelSelect(GameLevelInherentData data)
        {
            GameLevelLogic.Instance.SetSelectedGameLevel(data);
            gameLevelSelectedUI.SetActive(true);
        }
    }
}