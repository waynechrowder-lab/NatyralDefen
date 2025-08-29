using System;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class GoodsItem : MonoBehaviour
    {
        [SerializeField] TMP_Text goodsName;
        [SerializeField] private TMP_Text goodsCost;
        [SerializeField] private TMP_Text goodsCount;
        [SerializeField] TMP_Text goodsDesc;
        [SerializeField] Image goodsIcon;
        
        private Action<MiniWorldShopData> _callback;
        private MiniWorldShopData _goods;
        public void InitItem(MiniWorldShopData goods, Action<MiniWorldShopData> callback)
        {
            if (!TryGetComponent(out CanvasGroup canvasGroup))
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            if (goods == null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                return;
            }
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            goodsName.text = goods.itemName;
            var coin = goods.itemCoin?.Get() ?? 0;
            var jewel = goods.itemJewel?.Get() ?? 0;
            goodsCost.text = coin == 0 ? jewel.ToString() : coin.ToString();
            goodsCount.text = goods.itemCount?.Get().ToString() ?? "0";
            if (goods.itemType is nameof(ShopType.debris) or nameof(ShopType.treasure))
            {
                var awardAsset = UIAssetsBindData.Instance.GetAwardIconAsset(goods.itemId);
                goodsIcon.sprite = awardAsset.awardIcon;
            }
            _goods = goods;
            _callback = callback;
            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(OnClickGoods);
            if (goods.itemType.Equals(nameof(ShopType.subscribe)))
            {
                bool subscribed = SubscribeLogic.Instance.HasSubscribed(goods.itemId);
                if (subscribed)
                {
                    goodsCost.text = "已订阅";
                    bool get = SubscribeLogic.Instance.HasGetSubscribedGoods(goods.itemId);
                    if (get)
                        goodsCount.text = "已领取";
                }
            }
        }

        void OnClickGoods()
        {
            _callback?.Invoke(_goods);
            if (_goods.itemType.Equals(nameof(ShopType.subscribe)))
            {
                bool subscribed = SubscribeLogic.Instance.HasSubscribed(_goods.itemId);
                if (subscribed)
                {
                    goodsCost.text = "已订阅";
                    bool get = SubscribeLogic.Instance.HasGetSubscribedGoods(_goods.itemId);
                    if (get)
                        goodsCount.text = "已领取";
                }
            }
        }
    }
}