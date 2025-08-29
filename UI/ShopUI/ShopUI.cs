using System.Collections.Generic;
using System.Linq;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Gameplay.Script.UI
{
    public class ShopUI : BasedUI
    {
        [SerializeField] TMP_Text coinText;
        [SerializeField] TMP_Text jewelText;
        
        [SerializeField] private Transform subscribeParent;
        [SerializeField] private Transform jewelParent;
        [SerializeField] private Transform coinParent;
        [SerializeField] private Transform treasureParent;
        [SerializeField] private Transform debrisParent;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            ShopLogic.Instance.RegisterExpendCallback(OnExpend);
            ShopLogic.Instance.RegisterShopListUpdate(OnShopUIUpdate);
            ShopLogic.Instance.QueryShopItems();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ShopLogic.Instance.UnRegisterExpendCallback(OnExpend);
            ShopLogic.Instance.UnRegisterShopListUpdate(OnShopUIUpdate);
        }

        private void OnExpend(UserDataJson json)
        {
            coinText.text = (json?.coin ?? 0).ToString();
            // jewelText.text = jewel.ToString();
        }
        
        private void OnShopUIUpdate(List<MiniWorldShopData> list)
        {
            if (list == null) list = new List<MiniWorldShopData>();
            List<MiniWorldShopData> subscribes = new List<MiniWorldShopData>();
            List<MiniWorldShopData> jewels = new List<MiniWorldShopData>();
            List<MiniWorldShopData> coins = new List<MiniWorldShopData>();
            List<MiniWorldShopData> treasures = new List<MiniWorldShopData>();
            List<MiniWorldShopData> debrises = new List<MiniWorldShopData>();
            list.ForEach(value =>
            {
                switch (value.itemType)
                {
                    case nameof(ShopType.subscribe):
                        subscribes.Add(value);
                        break;
                    case nameof(ShopType.jewel):
                        jewels.Add(value);
                        break;
                    case nameof(ShopType.coin):
                        coins.Add(value);
                        break;
                    case nameof(ShopType.treasure):
                        treasures.Add(value);
                        break;
                    case nameof(ShopType.debris):
                        debrises.Add(value);
                        break;
                    default:
                        Debug.LogError($"unknown type : {value.itemType}");
                        break;
                }
            });
            subscribeParent.GetComponent<GoodsItem>().
                InitItem(subscribes.Count > 0 ? subscribes[0] : null, OnClickGoodsItem);
            // SetGoodsItem(subscribes, subscribeParent);
            SetGoodsItem(jewels, jewelParent);
            SetGoodsItem(coins, coinParent);
            SetGoodsItem(treasures, treasureParent);
            SetGoodsItem(debrises, debrisParent);
        }

        void SetGoodsItem(List<MiniWorldShopData> list, Transform parent)
        {
            var items = parent.GetComponentsInChildren<GoodsItem>(true);
            var count = list.Count;
            for (int i = 0; i < items.Length; i++)
            {
                if (i < list.Count)
                {
                    var goods = list[i];
                    items[i].InitItem(goods, OnClickGoodsItem);
                }
                else 
                    items[i].InitItem(null, OnClickGoodsItem);
            }
        }

        private void OnClickGoodsItem(MiniWorldShopData goods)
        {
            if (goods.itemType == nameof(ShopType.subscribe))
            {
                bool subscribed = SubscribeLogic.Instance.HasSubscribed(goods.itemId);
                if (subscribed)
                {
                    SubscribeLogic.Instance.GetSubscribedGoods(goods);
                    return;
                }
            }
            bool canCost = ShopLogic.Instance.CanCost(goods);
            Debug.Log($"goods : {goods.itemName} , can cost : {canCost}");
            if (canCost)
                ShopLogic.Instance.Shop(goods);
        }
    }
}