using System;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class GameLevelItem : MonoBehaviour
    {
        [SerializeField] private Image levelTexture;
        [SerializeField] private TMP_Text levelName;
        [SerializeField] private TMP_Text levelCondition;
        [SerializeField] private TMP_Text levelDescription;
        GameLevelInherentData _gameLevelInherentData;
        private Action<GameLevelInherentData> _onClick;
        public void InitItem(string gameLevelId, Action<GameLevelInherentData> onClick)
        {
            if (gameLevelId == "MR")
            {
                levelName.text = "MR世界";
                levelCondition.text = "";
                _onClick = onClick;
                return;
            }
            var levelItem = GameLevelLogic.Instance.GetGameLevel(gameLevelId);
            _onClick = onClick;
            _gameLevelInherentData = levelItem;
            levelName.text = levelItem.name;
            if (string.IsNullOrEmpty(levelItem.condition))
                levelCondition.text = "";
            else
                levelCondition.text = $"解锁条件：{levelItem.condition}";
            levelTexture.sprite = UIAssetsBindData.Instance.GetGameLevelIconAsset(gameLevelId).gameLevelIcon;
        }

        public void OnClickLevel()
        {
            _onClick?.Invoke(_gameLevelInherentData);
        }
    }
}