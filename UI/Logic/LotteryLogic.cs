using System;
using System.Collections.Generic;
using System.Linq;
using Currency.Core.Run;
using Gameplay.Script.Data;
using Gameplay.Script.Manager;
using UnityEngine;

namespace Gameplay.Script.Logic
{
    public class LotteryLogic : Single<LotteryLogic>
    {
        private List<string> _treasureList;

        public int GetLotteryCount(TreasureType type)
        {
            var goodsList = UserDataManager.Instance.UserStoreJson?.userGoodsJson?.goodsItem.ToList() ?? 
                       Array.Empty<GoodsItem>().ToList();
            if (type == TreasureType.NormalTreasure)
                return goodsList.FirstOrDefault(value => value.goodsId.Equals("ITEM_003"))?.goodsNum ?? 0;
            if (type == TreasureType.AdvancedTreasure)
                return goodsList.FirstOrDefault(value => value.goodsId.Equals("ITEM_004"))?.goodsNum ?? 0;
            return goodsList.FirstOrDefault(value => value.goodsId.Equals("ITEM_005"))?.goodsNum ?? 0;
        }

        public void ExpendStoreLottery(TreasureType type)
        {
            string id = "ITEM_003";
            if (type == TreasureType.AdvancedTreasure)
                id = "ITEM_004";
            else if (type == TreasureType.UltimateTreasure)
                id = "ITEM_005";
            StoreLogic.Instance.ExpendGoods(id, 1);
        }
        
        public bool CanCost(int cost)
        {
            var userJewel = UserDataManager.Instance.UserDataJson?.jewel ?? 0;
            return userJewel >= cost;
        }

        public AwardInherentData GetAward(string id)
        {
            return AwardData.Instance.GetAward(id);
        }
        
        public TreasureInherentData GetTreasure(TreasureType type, string treasureId)
        {
            return TreasureData.Instance.GetTreasure(type, treasureId);
        }

        public void SaveTreasure(int cost, TreasureType type, string id)
        {
            var treasure = GetTreasure(type, id);
            var shopType = treasure.type;
            if (GetLotteryCount(type) <= 0)
                UserDataManager.Instance.ExpendJewel(cost);
            else 
                ExpendStoreLottery(type);
            int saved = treasure.count;
            if (saved <= 0) throw new Exception($"{treasure.treasureId} count is {saved}");
            Debug.Log($"add {treasure.name} count {saved}");
            switch (shopType)
            {
                case ShopType.jewel:
                    UserDataManager.Instance.SaveJewel(saved);
                    break;
                case ShopType.coin:
                    UserDataManager.Instance.SaveCoin(saved);
                    break;
                case ShopType.treasure:
                    UserDataManager.Instance.SaveGoods(treasure.id, saved);
                    break;
                case ShopType.debris:
                    UserDataManager.Instance.SaveGoods(treasure.id, saved);
                    break;
            }

        }
        
        public List<string> GetTreasureList(TreasureType treasureType)
        {
            if (PlayerPrefs.HasKey($"{treasureType.ToString()}"))
            {
                var list = PlayerPrefs.GetString($"{treasureType.ToString()}").Split(',');
                _treasureList = list.ToList();
            }
            else
            {
                var list = TreasureData.Instance.GetTreasureInherentDataList(treasureType);
                var normalList = list.Where(value => value.quality == TreasureQuality.普通).ToList();
                var advanceList = list.Where(value => value.quality == TreasureQuality.高级).ToList();
                var ultimateList = list.Where(value => value.quality == TreasureQuality.终极).ToList();
                int normalMax = normalList.First().probability;
                int advanceMax = normalMax + advanceList.First().probability;
                string id;
                List<string> ids = new List<string>();
                for (int i = 0; i < 8; i++)
                {
                    int random = UnityEngine.Random.Range(0, 100);
                    if (random < normalMax)
                    {
                        int subRandom = UnityEngine.Random.Range(0, normalList.Count);
                        id = normalList[subRandom].treasureId;
                    }
                    else if (random < advanceMax)
                    {
                        int subRandom = UnityEngine.Random.Range(0, advanceList.Count);
                        id = advanceList[subRandom].treasureId;
                    }
                    else
                    {
                        int subRandom = UnityEngine.Random.Range(0, ultimateList.Count);
                        id = ultimateList[subRandom].treasureId;
                    }
                    ids.Add(id);
                }
                _treasureList = ids;
                PlayerPrefs.SetString($"{treasureType.ToString()}", string.Join(",", ids));
            }
            return _treasureList ?? new List<string>();
        }
    }
}