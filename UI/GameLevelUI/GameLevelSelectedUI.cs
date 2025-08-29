using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using TMPro;
using UnityEngine;

namespace Gameplay.Script.UI
{
    public class GameLevelSelectedUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameLevelBagUI;

        [SerializeField] private TMP_Text levelName;
        [SerializeField] private Transform levelEnemyParent;
        [SerializeField] private Transform levelAwardParent;
        [SerializeField] private CanvasGroup tips;
        [SerializeField] private GameObject btnStart;
        [SerializeField] private GameObject btnMrStart;
        private float _tipShowTime;

        private GameObject _levelEnemyItem;
        private GameObject _levelAwardItem;
        private void OnEnable()
        {
            var data = GameLevelLogic.Instance.SelectedInherentData;
            if (data != null)
            {
                SetGameLevelEnemyItem();
                SetGameLevelAwardItem();
            }
            btnStart.SetActive(data != null);
            btnMrStart.SetActive(data == null);
            _tipShowTime = 0;
            tips.alpha = 0;
        }

        private void Update()
        {
            if (_tipShowTime > 0)
            {
                _tipShowTime -= Time.deltaTime;
                tips.alpha = _tipShowTime;
            }
        }

        void SetGameLevelEnemyItem()
        {
            if (!_levelEnemyItem)
            {
                _levelEnemyItem = levelEnemyParent.GetChild(0).gameObject;
                _levelEnemyItem.SetActive(false);
            }
            var gameEnemies = GameLevelLogic.Instance.GetGameEnemies();
            int count = gameEnemies?.Count ?? 0;
            for (int i = 0; i < levelEnemyParent.childCount; i++)
            {
                var item = levelEnemyParent.GetChild(i).gameObject;
                if (i < count)
                {
                    item.GetComponent<GameLevelEnemyItem>().InitItem(gameEnemies[i], OnEnemySelect);
                    item.SetActive(true);
                }
                else
                    item.SetActive(false);
            }

            for (int i = levelEnemyParent.childCount; i < count; i++)
            {
                var item = Instantiate(_levelEnemyItem, levelEnemyParent);
                item.GetComponent<GameLevelEnemyItem>().InitItem(gameEnemies[i], OnEnemySelect);
                item.SetActive(true);
            }
        }

        void SetGameLevelAwardItem()
        {
            if (!_levelAwardItem)
            {
                _levelAwardItem = levelAwardParent.GetChild(0).gameObject;
                _levelAwardItem.SetActive(false);
            }
            var gameAwards = GameLevelLogic.Instance.GetGameAwards();
            int count = gameAwards?.Count ?? 0;
            for (int i = 0; i < levelAwardParent.childCount; i++)
            {
                var item = levelAwardParent.GetChild(i).gameObject;
                if (i < count)
                {
                    item.GetComponent<GameLevelAwardItem>().InitItem(gameAwards[i], OnAwardSelect);
                    item.SetActive(true);
                }
                else
                    item.SetActive(false);
            }

            for (int i = levelAwardParent.childCount; i < count; i++)
            {
                var item = Instantiate(_levelAwardItem, levelAwardParent);
                item.GetComponent<GameLevelAwardItem>().InitItem(gameAwards[i], OnAwardSelect);
                item.SetActive(true);
            }
        }

        void OnEnemySelect(EnemyInherentData enemy)
        {
            
        }
        
        void OnAwardSelect(AwardInherentData award)
        {
            
        }

        public void OnClickClose()
        {
            gameObject.SetActive(false);
        }

        public void OnClickOpenBag()
        {
            gameLevelBagUI.SetActive(true);
        }
        
        public void OnClickStartGameLevel()
        {
            bool b = GameLevelLogic.Instance.StartGameLevel(false);
            if (!b)
            {
                _tipShowTime = 3;
            }
        }
        
        public void OnClickStartMRGameLevel()
        {
            bool b = GameLevelLogic.Instance.StartGameLevel(true);
            if (!b)
            {
                _tipShowTime = 3;
            }
        }
    }
}