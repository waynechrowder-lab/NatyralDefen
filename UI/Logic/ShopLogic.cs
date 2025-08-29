using System;
using System.Collections.Generic;
using Currency.Core.Run;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Manager;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Script.Logic
{
    public class ShopLogic : Single<ShopLogic>
    {
        private Action<List<MiniWorldShopData>> _shopListCallback;
        
        List<MiniWorldShopData> _shopList;      
        
        public void RegisterShopListUpdate(Action<List<MiniWorldShopData>> callback)
        {
            _shopListCallback += callback;
            OnGetShopList(_shopList);
        }

        public void UnRegisterShopListUpdate(Action<List<MiniWorldShopData>> callback)
        {
            _shopListCallback -= callback;
        }

        public void QueryShopItems()
        {
            BmobManager.Instance.GetGameShop(OnGetShopList);
        }

        private void OnGetShopList(List<MiniWorldShopData> list)
        {
            _shopList = list;
            _shopListCallback?.Invoke(_shopList ?? new List<MiniWorldShopData>());
        }

        public bool CanCost(MiniWorldShopData goods)
        {
            var coin = goods.itemCoin?.Get() ?? 0;
            var jewel = goods.itemJewel?.Get() ?? 0;
            var userData = UserDataManager.Instance.UserDataJson;
            var userCoin = userData?.coin ?? 0;
            var userJewel = userData?.jewel ?? 0;
            return userCoin >= coin && userJewel >= jewel;
        }
        
        public void RegisterExpendCallback(UnityAction<UserDataJson> expend)
        {
            UserDataManager.Instance.RegisterUserExpendCallback(expend);
            var userData = UserDataManager.Instance.UserDataJson;
            expend?.Invoke(userData);
        }

        public void UnRegisterExpendCallback(UnityAction<UserDataJson> expend)
        {
            UserDataManager.Instance.UnRegisterUserExpendCallback(expend);
        }

        public void Shop(MiniWorldShopData goods)
        {
            Enum.TryParse(goods.itemType, out ShopType shopType);
            int coin = goods.itemCoin?.Get() ?? 0;
            int jewel = goods.itemJewel?.Get() ?? 0;
            if (coin > 0)
                UserDataManager.Instance.ExpendCoin(coin);
            if (jewel > 0)
                UserDataManager.Instance.ExpendJewel(jewel);
            int saved = goods.itemCount?.Get() ?? 0;
            if (saved <= 0) throw new Exception($"{goods.itemId} count is {saved}");
            switch (shopType)
            {
                case ShopType.subscribe:
                    SubscribeLogic.Instance.Subscribe(goods);
                    break;
                case ShopType.jewel:
                    UserDataManager.Instance.SaveJewel(saved);
                    break;
                case ShopType.coin:
                    UserDataManager.Instance.SaveCoin(saved);
                    break;
                case ShopType.treasure:
                    UserDataManager.Instance.SaveGoods(goods.itemId, saved);
                    break;
                case ShopType.debris:
                    UserDataManager.Instance.SaveGoods(goods.itemId, saved);
                    break;
            }
        }
    }

    public enum ShopType
    {
        subscribe,
        jewel,
        coin,
        treasure,
        debris
    }
}