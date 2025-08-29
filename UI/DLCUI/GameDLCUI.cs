using System;
using System.Linq;
using Gameplay.Script.Manager;
using Pico.Platform.Models;
using UnityEngine;

namespace Gameplay.Script.UI
{
    public class GameDLCUI : MonoBehaviour
    {
        [SerializeField] private Transform dlcParent;
        private GameObject _dlcItem;

        private void Awake()
        {
            _dlcItem = dlcParent.GetChild(0).gameObject;
            _dlcItem.SetActive(false);
        }

        private void OnEnable()
        {
            var list = IAPManager.Instance.AssetDetailsList;
            OnAssetGet(list);
            IAPManager.Instance.RegisterIAP_DLC(OnAssetGet);
        }

        private void OnDisable()
        {
            IAPManager.Instance.UnRegisterIAP_DLC(OnAssetGet);
        }

        private void OnAssetGet(AssetDetailsList list)
        {
            if (list == null)
                return;
            var purchasedList = list.ToList().Where(value => value.IapStatus == "entitled").ToList();
            for (int i = 0; i < purchasedList.Count; i++)
            {
                UserDataManager.Instance.PurchaseAsset(purchasedList[i].Filename);
            }
            for (int i = 0; i < dlcParent.childCount; i++)
            {
                var item = dlcParent.GetChild(i).gameObject;
                if (i < list.Count)
                {
                    item.SetActive(true);
                    var asset = list[i];
                    item.GetComponent<DLCItem>().InitItem(asset);
                }
                else
                    item.SetActive(false);
            }

            for (int i = dlcParent.childCount; i < list.Count; i++)
            {
                var item = Instantiate(_dlcItem, dlcParent);
                item.SetActive(true);
                var asset = list[i];
                item.GetComponent<DLCItem>().InitItem(asset);
            }
        }

        public void OnClickBack()
        {
            gameObject.SetActive(false);
        }
    }
}