using System;
using System.Collections.Generic;
using System.Linq;
using Currency.Core.Run;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Manager;
using UnityEngine.Events;

namespace Gameplay.Script.Logic
{
    public class StoreLogic : Single<StoreLogic>
    {
        // private Action<List<GoodsItem>> _goodsListCallback;
        //
        // List<GoodsItem> _goodsList;   

        public List<GoodsItem> GetStoreList()
        {
            var list = 
                UserDataManager.Instance.UserStoreJson?.userGoodsJson?.goodsItem ?? Array.Empty<GoodsItem>();
            return list.ToList().Where(value => value.goodsNum > 0).ToList();
        }

        public List<UserPlantData> GetUserPlantList()
        {
            var list = 
                UserDataManager.Instance.UserDataJson?.userPlants ?? Array.Empty<UserPlantData>();
            return list.ToList();
        }
        
        public void RegisterStoreListUpdate(UnityAction<GoodsItem> callback)
        {
            // _goodsListCallback += callback;
            UserDataManager.Instance.RegisterUserStoreCallback(callback);
            
            // OnGetStoreList(goodsList.ToList());
        }

        public void UnRegisterStoreListUpdate(UnityAction<GoodsItem> callback)
        {
            UserDataManager.Instance.UnRegisterUserStoreCallback(callback);
            // _goodsListCallback -= callback;
        }
        //
        // private void OnGetStoreList(List<GoodsItem> list)
        // {
        //     _goodsList = list;
        //     _goodsListCallback?.Invoke(_goodsList);
        // }

        public AwardInherentData GetGoods(string goodsItemGoodsId)
        {
            return AwardData.Instance.GetAward(goodsItemGoodsId);
        }

        public PlantInherentData GetInherentPlant(string plantId)
        {
            return PlantData.Instance.GetPlantInherentData(plantId);
        }
        
        public void ExpendGoods(string id, int count)
        {
            UserDataManager.Instance.ExpendGoods(id, count);
        }
    }
}