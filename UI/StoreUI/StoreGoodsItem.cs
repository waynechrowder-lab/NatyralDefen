using System;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class StoreGoodsItem : MonoBehaviour
    {
        [SerializeField] TMP_Text goodsName;
        [SerializeField] TMP_Text goodsCount;
        [SerializeField] Image goodsIcon;
        
        public void InitItem(Data.GoodsItem goodsItem, Action<Data.GoodsItem> clickGoodsItem)
        {
            var item = StoreLogic.Instance.GetGoods(goodsItem.goodsId);
            goodsName.text = item.name;
            goodsCount.text = goodsItem.goodsNum.ToString();
            var awardAsset = UIAssetsBindData.Instance.GetAwardIconAsset(goodsItem.goodsId);
            goodsIcon.sprite = awardAsset.awardIcon;
            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(() =>
            {
                clickGoodsItem?.Invoke(goodsItem);
            });
        }

        public void InitItem(UserPlantData plant, Action<UserPlantData> clickPlantItem)
        {
            var itemInherent = StoreLogic.Instance.GetInherentPlant(plant.plantId);
            goodsName.text = itemInherent.name;
            goodsCount.text = $"lv{plant.plantLevel}";
            var awardAsset = UIAssetsBindData.Instance.GetAwardIconAsset(itemInherent.itemId);
            goodsIcon.sprite = awardAsset.awardIcon;
            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(() =>
            {
                clickPlantItem?.Invoke(plant);
            });
        }
    }
}