using System;
using System.Collections.Generic;
using System.Linq;
using Currency.Core.Run;
using Gameplay.Script.Data;
using Gameplay.Script.Manager;
using Gameplay.Script.MultiplayerModule;
using Pico.Avatar.Sample;
using UnityEngine;
using Random = System.Random;

namespace Gameplay.Script.MiniWorldGameplay
{
    public class GameplayLogic : Single<GameplayLogic>
    {
        private GameLevelInherentData _gameLevelInherentData;
        private List<GameLevelItemData> _gameLevelItemDatas;
        public int SunCoin {get; private set;}
        public float GameTimer {get; private set; }
        private int _gameLevelGetCoin;
        public int GameLevelGetCoin => _gameLevelGetCoin;
        private List<Data.GoodsItem> _awardGoods = new();
        public List<Data.GoodsItem> AwardGoods => _awardGoods;
        public (GameLevelInherentData, List<GameLevelItemData>) GetGameLevelData()
        {
            return (_gameLevelInherentData, _gameLevelItemDatas);
        }
        
        public int GetGameLevelWaveCount()
        {
            return _gameLevelItemDatas?.Count ?? 0;
        }

        public GameLevelItemData GetCurrentGameLevelWaveData(int waveIndex)
        {
            GameLevelItemData data = null;
            if (_gameLevelItemDatas != null)
            {
                _ = _gameLevelItemDatas.Any(value =>
                {
                    if (waveIndex == value.wave)
                    {
                        data = value;
                        return true;
                    }
                    return false;
                });
            }
            return data;
        }
            
        
        public void SetGameLevelData(GameLevelInherentData inherentData, List<GameLevelItemData> levelData)
        {
            _gameLevelInherentData = inherentData;
            _gameLevelItemDatas = levelData;
        }

        public void CoinChanged(int value)
        {
            SunCoin += value;
        }

        public bool CanCost(int cost)
        {
            return SunCoin >= cost;
        }
        
        public void EnterGameplay()
        {
            var scene = SceneLoadManager.SceneIndex.LEVEL_001;
            if (Enum.TryParse(_gameLevelInherentData.id, out SceneLoadManager.SceneIndex getScene))
            {
                scene = getScene;
            }
            SceneLoadManager.Instance.OnLoadScene(scene);
        }

        public void EnterMRGameplay()
        {
            SceneLoadManager.Instance.OnLoadScene(SceneLoadManager.SceneIndex.MRScene);
        }

        public void StartGameplay(int coin = 50)
        {
            SunCoin = coin;
            _gameLevelGetCoin = 0;
            _awardGoods.Clear();
            EventDispatcher.Instance.Dispatch((int)EventID.StartSpawnPlantOnHand);
            EventDispatcher.Instance.Dispatch((int)EventID.StartSpawnEnemy);
        }

        public void UpdateGameTimeUsage(float time)
        {
            GameTimer = time;
        }

        public void GetEnemyExp(string enemyId, int level, int exp)
        {
            int userCoin = Exp2Coin(exp);
            _gameLevelGetCoin += userCoin;
        }

        int Exp2Coin(int exp)
        {
            return exp;
        }

        public void GetGameLevelAward()
        {
            if (_gameLevelItemDatas.Count > 0)
            {
                // var plantIds = new[] { "ITEM_008", "ITEM_010", "ITEM_012", "ITEM_014", "ITEM_016", "ITEM_018" };
                var awardItem = _gameLevelItemDatas[^1];
                var awardIdList = awardItem.waveDropOutItemIds.ToList();
                var awardCountList = awardItem.waveDropOutItemCounts.ToList();
                var awardProbsList = awardItem.waveDropOutItemProbs.ToList();
                int count = Mathf.Min(awardIdList.Count, awardCountList.Count, awardProbsList.Count);
                for (int i = 0; i < count; i++)
                {
                    int random = UnityEngine.Random.Range(0, 100);
                    if (random < awardProbsList[i])// && !plantIds.Contains(awardIdList[i]))
                        _awardGoods.Add(new GoodsItem(awardIdList[i], awardCountList[i]));
                }
            }
        }

        public void Save2UserStore()
        {
            if (_gameLevelGetCoin > 0)
                UserDataManager.Instance.SaveCoin(_gameLevelGetCoin);
            if (_awardGoods.Count > 0)
                _awardGoods.ForEach(value =>
                {
                    UserDataManager.Instance.SaveGoods(value);
                });
        }
    }
}