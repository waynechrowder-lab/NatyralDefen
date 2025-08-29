using System;
using Newtonsoft.Json;
using Pico.Platform;
using Pico.Platform.Models;
using UnityEngine;

namespace Gameplay.Script.Manager
{
    public class IAPManager : MonoSingleton<IAPManager>
    {
        private Action<AssetDetailsList> _onDLCAssetGet;
        private AssetDetailsList _assetDetailsList;
        public AssetDetailsList AssetDetailsList => _assetDetailsList;
        private bool _isInTask;
        private void Start()
        {
            EventDispatcher.Instance.Register((int)EventID.NetworkUserLogin, OnUserLogin);
        }
        
        private void OnDisable()
        {
            EventDispatcher.Instance.UnRegister((int)EventID.NetworkUserLogin, OnUserLogin);
        }

        public void RegisterIAP_DLC(Action<AssetDetailsList> list)
        {
            _onDLCAssetGet += list;
        }

        public void UnRegisterIAP_DLC(Action<AssetDetailsList> list)
        {
            _onDLCAssetGet -= list;
        }
        
        private void OnUserLogin(GameEventArg arg)
        {
#if !UNITY_EDITOR
            InvokeRepeating(nameof(GetAllAssetFiles), 0, 3);
            // GetAllAdd_OnProducts();
            // GetOwnedAdd_OnProducts();
#endif
        }
        
        public void GetAllAdd_OnProducts()
        {
            GetAllAdd_OnProductsTask();
        }
        
        public void GetAllAssetFiles()
        {
            if (!_isInTask)
                GetAllAssetFilesTask();
        }

        async void GetAllAssetFilesTask()
        {
            _isInTask = true;
            AssetFileService.GetList().OnComplete(AssetFileHandler);
        }
        
        private void AssetFileHandler(Message<AssetDetailsList> message)
        {
            if (message.IsError)
            {
                _isInTask = false;
                _onDLCAssetGet?.Invoke(null);
                Debug.LogError($"AssetFileHandler:{message.Error}");
                return;
            }
            _isInTask = false;
            _assetDetailsList = message.Data;
            _onDLCAssetGet?.Invoke(message.Data);
        }
        
        async void GetAllAdd_OnProductsTask()
        {
            var task = IAPService.GetProductsBySKU(null);
            task.OnComplete(ProductsHandler);
        }

        private void ProductsHandler(Message<ProductList> message)
        {
            if (message.IsError)
            {
                Debug.LogError($"ProductsHandler:{message.Error}");
                return;
            }
            else
            {
                Debug.LogError($"Add-On Products Count:{message.Data.Count}");
            }
        }
        
        public void GetOwnedAdd_OnProducts()
        {
            GetOwnedAdd_OnProductsTask();
        }

        async void GetOwnedAdd_OnProductsTask()
        {
            var task = IAPService.GetViewerPurchases();
            task.OnComplete(OwnedHandler);
        }

        private void OwnedHandler(Message<PurchaseList> message)
        {
            if (message.IsError)
            {
                Debug.LogError($"OwnedHandler:{message.Error}");
                return;
            }
            else
            {
                Debug.LogError($"Add-On Owned Count:{message.Data.Count}");
            }
        }
    }
}