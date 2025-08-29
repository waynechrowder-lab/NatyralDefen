using System.Collections.Generic;
using System.Linq;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Gameplay;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using TMPro;
using UnityEngine;

namespace Gameplay.Script.UI
{
    public class StoreUI : BasedUI
    {
        [SerializeField] private Transform plantParent;
        [SerializeField] private PlantIntroduction introducePanel;
        private GameObject _plantItem;
        private bool _isPrecessing;
        private List<MiniWorldShopData> _miniWorldShopDatas = new();
        protected override void OnEnable()
        {
            base.OnEnable();
            introducePanel.gameObject.SetActive(false);
            OnRefreshStoreAssets();
        }

        void OnRefreshStoreAssets()
        {
            if (!_plantItem)
            {
                _plantItem = plantParent.GetChild(0).gameObject;
                _plantItem.SetActive(false);
            }
            if (_isPrecessing) return;
            _isPrecessing = true;
            BmobManager.Instance.GetGameStoreAssets(OnGetGameStoreAssets);
        }

        private void OnGetGameStoreAssets(List<MiniWorldShopData> list)
        {
            _miniWorldShopDatas = list
                .OrderBy(value => value.index?.Get() ?? 0)
                .ToList();
            _isPrecessing = false;
            int count = _miniWorldShopDatas?.Count ?? 0;
            for (int i = 0; i < plantParent.childCount; i++)
            {
                GameObject item = plantParent.GetChild(i).gameObject;
                if (i < count)
                {
                    var userPlant = _miniWorldShopDatas[i];
                    var inhereData = PlantUpgradeLogic.Instance.GetPlantData(userPlant.otherId);
                    item.SetActive(true);
                    item.GetComponent<PlantUpgradeItem>().InitItem(inhereData, userPlant, OnClickIntroduce, OnClickBuy);
                }
                else
                    item.SetActive(false);
            }

            for (int i = plantParent.childCount; i < count; i++)
            {
                GameObject item = Instantiate(_plantItem, plantParent);
                var userPlant = _miniWorldShopDatas[i];
                var inhereData = PlantUpgradeLogic.Instance.GetPlantData(userPlant.otherId);
                item.SetActive(true);
                item.GetComponent<PlantUpgradeItem>().InitItem(inhereData, userPlant, OnClickIntroduce, OnClickBuy);
            }
        }

        void OnClickIntroduce(string plantId)
        {
            introducePanel.gameObject.SetActive(true);
            introducePanel.LoadPlantData(plantId);
        }

        private void OnClickBuy(UserPlantData plant)
        {
            var item = _miniWorldShopDatas.FirstOrDefault(value => value.otherId.Equals(plant.plantId));
            if (item != null)
            {
                var coin = item.itemCoin?.Get() ?? 99999;
                if (coin <= UserDataManager.Instance.UserDataJson.coin)
                {
                    UserDataManager.Instance.ExpendCoin(coin);
                    UserDataManager.Instance.AddPlant(plant.plantId);
                    OnRefreshStoreAssets();
                }
            }
        }
    }
}