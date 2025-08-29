using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Currency.Core.Run;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Gameplay;
using Gameplay.Script.Logic;
using Script.Core.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Script.Manager
{
    public class UserDataManager : MonoSingle<UserDataManager>
    {
        [SerializeField] private bool useCustomUser;
        [SerializeField] private UserPicoJson customUser;
        private UnityAction<UserDataJson> _expendCallback;
        private UnityAction<GoodsItem> _storeCallback;
        public string UserHeadFolder { get; private set; }
        private int _cloudStoreVersion;
        private int _cloudUserVersion;
        private float _timer = 5;
        public UserDataJson UserDataJson { get; private set; }
        public UserStoreJson UserStoreJson { get; private set; }
        
        private string _localUserDataFilePath;
        private string _localUserStoreFilePath;
        private Dictionary<string, MiniWorldUser> _cacheUser = new Dictionary<string, MiniWorldUser>();
        private void Start()
        {
            UserHeadFolder = $"{Application.persistentDataPath}/user/head/";
            EventDispatcher.Instance.Register((int)EventID.NetworkUserLogin, OnUserLogin);
        }
        
        private void OnDisable()
        {
            EventDispatcher.Instance.UnRegister((int)EventID.NetworkUserLogin, OnUserLogin);
        }

        public void RegisterUserExpendCallback(UnityAction<UserDataJson> callback)
        {
            _expendCallback += callback;
        }
        
        public void UnRegisterUserExpendCallback(UnityAction<UserDataJson> callback)
        {
            _expendCallback -= callback;
        }
        
        public void RegisterUserStoreCallback(UnityAction<GoodsItem> callback)
        {
            _storeCallback += callback;
        }
        
        public void UnRegisterUserStoreCallback(UnityAction<GoodsItem> callback)
        {
            _storeCallback -= callback;
        }

        void OnUserLogin(GameEventArg arg)
        {
            bool loginSuccess = arg.GetArg<bool>(0);
            if (useCustomUser && Application.isEditor)
                UserData.Instance.SetUserData(customUser);
            if (loginSuccess)
                BmobManager.Instance.OnUserLogin(UserData.Instance.CurrentUser, null);
            Init(loginSuccess);
            StartCoroutine(nameof(UploadData));
        }
        
        public void Init(bool loginSuccess)
        {
            string id = UserData.Instance.CurrentUser.relevanceId;
            _localUserDataFilePath = $"{Application.persistentDataPath}/user/ud{id}.udat";
            UserDataJson = new UserDataJson().Create();
            if (IOHelper.IsExistFile(_localUserDataFilePath))
            {
                try
                {
                    UserDataJson = AndroidJsonTool.ReadJson<UserDataJson>(_localUserDataFilePath);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }

            // UserDataJson.userPlantJson ??= new UserPlantDataJson().Create();
            _localUserStoreFilePath = $"{Application.persistentDataPath}/user/us{id}.udat";
            UserStoreJson = new UserStoreJson().Create();
            if (IOHelper.IsExistFile(_localUserStoreFilePath))
            {
                try
                {
                    UserStoreJson = AndroidJsonTool.ReadJson<UserStoreJson>(_localUserStoreFilePath);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }
            GetCloudData();
        }
        
        void GetCloudData()
        {
            string id = UserData.Instance.CurrentUser.relevanceId;
            BmobManager.Instance.GetMiniWorldUserData(id, UserDataJson, data =>
            {
                UserDataJson = data.ToJson();
                _cloudUserVersion = UserDataJson.version;
                if (UserDataJson.userPlants == null || UserDataJson.userPlants.Length == 0)
                {
                    UserDataJson.userPlants = new[]
                    {
                        new UserPlantData("P001", 0), 
                        new UserPlantData("P003", 1),
                        new UserPlantData("P002", 2),
                        new UserPlantData("P004", 3),
                        new UserPlantData("P005", 4),
                        new UserPlantData("P006", 5),
                    };
                    UserDataJson.version++;
                }
                AndroidJsonTool.WriteJsonFile(UserDataJson, _localUserDataFilePath);
            }, objid =>
            {
                UserDataJson.objectId = objid;
            });
            
            BmobManager.Instance.GetMiniWorldUserStoreData(id, UserStoreJson, data =>
            {
                UserStoreJson = data.ToJson();
                _cloudStoreVersion = UserStoreJson.version;
                AndroidJsonTool.WriteJsonFile(UserStoreJson, _localUserStoreFilePath);
            }, objid =>
            {
                UserStoreJson.objectId = objid;
            });
        }

        public void LevelUpPlant(int userPlantId, int coin)
        {
            var plant = UserDataJson.userPlants.ToList().
                First(value => value.userPlantId.Equals(userPlantId));
            plant.plantLevel++;
            ExpendCoin(coin);
            UpdateFightingCapacity();
        }
        
        public void GradeUpPlant(int userPlantId, int debris)
        {
            var plant = UserDataJson.userPlants.ToList().
                First(value => value.userPlantId.Equals(userPlantId));
            plant.plantGrade++;
            var inherentData = PlantData.Instance.GetPlantInherentData(plant.plantId);
            var item = UserStoreJson.userGoodsJson.goodsItem
                .FirstOrDefault(value => value.goodsId.Equals(inherentData.debrisId));
            item.goodsNum -= debris;
            UserDataJson.version++;
            UserStoreJson.version++;
            _storeCallback?.Invoke(item);
            UpdateFightingCapacity();
        }
        
        public void ExpendCoin(int coin)
        {
            UserDataJson.coin -= coin;
            UserDataJson.version++;
            _expendCallback?.Invoke(UserDataJson);
        }
        
        public void ExpendJewel(int jewel)
        {
            UserDataJson.jewel -= jewel;
            UserDataJson.version++;
            _expendCallback?.Invoke(UserDataJson);
        }
        
        public void SaveCoin(int coin)
        {
            UserDataJson.coin += coin;
            UserDataJson.version++;
            _expendCallback?.Invoke(UserDataJson);
        }

        public void SaveExp(int exp)
        {
            UserDataJson.exp += exp;
            UserDataJson.version++;
            _expendCallback?.Invoke(UserDataJson);
        }

        public void SaveCoinAndExp(int coin, int exp)
        {
            UserDataJson.coin += coin;
            UserDataJson.exp += exp;
            UserDataJson.version++;
            _expendCallback?.Invoke(UserDataJson);
        }
        
        public void SaveJewel(int jewel)
        {
            UserDataJson.jewel += jewel;
            UserDataJson.version++;
            _expendCallback?.Invoke(UserDataJson);
        }

        public List<int> GetBagItems(QuickGameMode mode)
        {
            var bags = UserDataJson?.userBagPlants?.ToList();
            if (mode == QuickGameMode.Mode2)
            {
                bags = UserDataJson?.userBagPlantsM2?.ToList();
            }
            return bags ?? new List<int>();
        }

        public List<UserPlantData> GetUserPlants()
        {
            return UserDataJson?.userPlants?.ToList() ?? new List<UserPlantData>();
        }

        public void ChangeBagItems(QuickGameMode mode, List<int> items)
        {
            if (mode == QuickGameMode.Normal)
            {
                UserDataJson.userBagPlants = items.ToArray();
            }
            else if (mode == QuickGameMode.Mode2)
            {
                UserDataJson.userBagPlantsM2 = items.ToArray();
            }
            UserDataJson.version++;
            //UpdateFightingCapacity();
        }

        void UpdateFightingCapacity()
        {
            if (UserDataJson?.userBagPlants == null || UserDataJson?.userBagPlants.Length == 0)
                return;
            var userPlants = UserDataJson?.userPlants.ToList() ?? new List<UserPlantData>();
            var items = UserDataJson?.userBagPlants.ToList();
            int fightingCapacity = 0;
            for (int i = 0; i < items.Count; i++)
            {
                var userPlantId = items[i];
                UserPlantData userData = null;
                if (userPlants.Any(value =>
                    {
                        if (value.userPlantId.Equals(userPlantId))
                        {
                            userData = value;
                            return true;
                        }
                        return false;
                    }))
                {
                    if (userData != null)
                    {
                        int baseAttack = PlantData.Instance.GetPlantLevelData(userData.plantId, 1)?.attackValue ?? 0;
                        if (baseAttack > 0)
                        {
                            int itemFight = FightingCapacityLogic.GetFightingCapacity(
                                baseAttack, userData.plantLevel, userData.plantGrade);
                            fightingCapacity += itemFight;
                        }

                    }
                }
            }
            //todo:update ranking
            RankingLogic.Instance.UpdateUserRanking(fightingCapacity);
        }

        IEnumerator UploadData()
        {
            while (true)
            {
                if (!string.IsNullOrEmpty(UserDataJson?.objectId))
                {
                    _timer -= .1f;
                    if (_timer < 0)
                    {
                        _timer = 5;
                        SaveUserData();
                        SaveUserStore();
                    }
                }
                yield return new WaitForSeconds(.1f);
            }
        }

        void SaveUserData()
        {
            if (UserDataJson.version == 0)
                UserDataJson.version++;
            if (_cloudUserVersion < UserDataJson.version)
            {
                BmobManager.Instance.UpdateMiniWorldUserData(UserDataJson.objectId, UserDataJson, b =>
                {
                    if (b)
                    {
                        _cloudUserVersion = UserDataJson.version;
                        Debug.Log("更新成功");
                    }
                    else Debug.Log("更新失败");
                });
            }
            AndroidJsonTool.WriteJsonFile(UserDataJson, _localUserDataFilePath);
        }

        void SaveUserStore()
        {
            if (UserStoreJson.version == 0)
                UserStoreJson.version++;
            if (_cloudStoreVersion < UserStoreJson.version)
            {
                BmobManager.Instance.UpdateMiniWorldStoreData(UserStoreJson.objectId, UserStoreJson, b =>
                {
                    if (b)
                    {
                        _cloudStoreVersion = UserStoreJson.version;
                        Debug.Log("更新成功");
                    }
                    else Debug.Log("更新失败");
                });
            }
            AndroidJsonTool.WriteJsonFile(UserStoreJson, _localUserStoreFilePath);
        }
        
        public void GetUserInfo(string relevanceId, Action<MiniWorldUser> callback)
        {
            if (_cacheUser.TryGetValue(relevanceId, out var value))
            {
                callback(value);
                return;
            }
            BmobManager.Instance.GetUserInfo(relevanceId, user =>
            {
                callback(user);
                if (_cacheUser.Count > 100)
                {
                    var keys = _cacheUser.Keys;
                    _cacheUser.Remove(keys.ToList()[UnityEngine.Random.Range(0, keys.Count - 1)]);
                }

                if (user != null)
                    _cacheUser.TryAdd(user.relevanceId, user);
            });
        }
        
        public void ExpendGoods(string goodsItemId, int used)
        {
            UserStoreJson.userGoodsJson ??= new UserGoodsJson();
            var jsonList = UserStoreJson.userGoodsJson.goodsItem?.ToList() ?? new List<GoodsItem>();
            var existingItem = jsonList.FirstOrDefault(value => value.goodsId.Equals(goodsItemId));
            if (existingItem != null)
                existingItem.goodsNum -= used;
            UserStoreJson.userGoodsJson.goodsItem = jsonList.ToArray();
            UserStoreJson.version++;
            _storeCallback?.Invoke(existingItem);
        }

        public void SaveGoods(string goodsItemId, int saved)
        {
            if (goodsItemId.Equals("ITEM_001"))
                SaveCoin(saved);
            else if (goodsItemId.Equals("ITEM_002"))
                SaveJewel(saved);
            else
            {
                UserStoreJson.userGoodsJson ??= new UserGoodsJson();
                var jsonList = UserStoreJson.userGoodsJson.goodsItem?.ToList() ?? new List<GoodsItem>();
                var existingItem = jsonList.FirstOrDefault(value => value.goodsId.Equals(goodsItemId));
                if (existingItem != null)
                    existingItem.goodsNum += saved;
                else
                {
                    existingItem = new GoodsItem(goodsItemId, saved);
                    jsonList.Add(existingItem);
                }
                UserStoreJson.userGoodsJson.goodsItem = jsonList.ToArray();
                UserStoreJson.version++;
                _storeCallback?.Invoke(existingItem);
            }
        }
        
        public void SaveGoods(GoodsItem goods)
        {
            string goodsItemId = goods.goodsId;
            int saved = goods.goodsNum;
            SaveGoods(goodsItemId, saved);
        }

        public void Subscribe(string goodsItemId)
        {
            List<UserSubscribeDataJson> subscribeList = new List<UserSubscribeDataJson>();
            if (UserDataJson?.userSubscribeJson != null)
                subscribeList = UserDataJson.userSubscribeJson.ToList();
            subscribeList.Add(new UserSubscribeDataJson()
            {
                subscribeId = goodsItemId,
                subscribedTime = DateTime.Now.ToString("yy-MM-dd"),
                getSubscribedItemIndex = Array.Empty<int>()
            });
            UserDataJson!.userSubscribeJson = subscribeList.ToArray();
            UserDataJson.version++;
        }

        public void SetSignInData()
        {
            var days = UserDataManager.Instance.UserDataJson?.signIn ?? Array.Empty<string>();
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            var list = days.ToList();
            list.Add(today);
            var json = UserDataManager.Instance.UserDataJson;
            if (json != null)
                json.signIn = list.ToArray();
            UserDataJson.version++;
        }

        public void SetElementData(UserPlantData plant, UserElement type)
        {
            if (UserDataJson == null)
            {
                Debug.LogError("UserDataJson is null.");
                return;
            }
            var selfPlants = UserDataJson.userPlants;
            if (selfPlants == null)
            {
                Debug.LogError("User plants data is null.");
                return;
            }
            if (plant == null || type == null)
            {
                Debug.LogError("Plant or type is null.");
                return;
            }
            plant.elementType = type;
            UserDataJson.version++;
        }

        public void UpdateUnlockItem(UnlockItem unlockItem)
        {
            UserStoreJson.userGoodsJson ??= new UserGoodsJson();
            var jsonList = UserStoreJson.userUnlockItemsJson.goodsItem?.ToList() ?? new List<UnlockItem>();
            bool contain = false;
            for (int i = 0; i < jsonList.Count; i++)
            {
                if (jsonList[i].id.Equals(unlockItem.id))
                {
                    contain = true;
                    jsonList[i] = unlockItem;
                    break;
                }
            }

            if (!contain) jsonList.Add(unlockItem);
            UserStoreJson.userUnlockItemsJson.goodsItem = jsonList.ToArray();
            UserStoreJson.version++;
        }

        public void AddPlant(string id)
        {
            var plants = UserDataJson.userPlants ?? new UserPlantData[] { };
            int index = 0;
            if (UserDataJson.userPlants != null)
            {
                UserDataJson.userPlants.ToList().ForEach(value =>
                {
                    if (value.userPlantId > index)
                    {
                        index = value.userPlantId;
                    }
                });
            }
            index++;
            var list = plants.ToList();
            list.Add(new UserPlantData(id, index));
            plants = list.ToArray();
            UserDataJson.userPlants = plants;
            UserDataJson.version++;
        }

        public void PurchaseAsset(string assetFilename)
        {
            if (assetFilename.Contains("CN_DLC_001"))
            {
                var ids = UserStoreJson.purchaseIds ?? "";
                var idList = ids.Split(',').ToList();
                if (!idList.Contains(assetFilename))
                {
                    idList.Add(assetFilename);
                    UserStoreJson.purchaseIds = string.Join(",", idList);
                    UserStoreJson.version++;
                    AddPlant("P008");
                }
            }
            else if (assetFilename.Contains("CN_DLC_002"))
            {
                var ids = UserStoreJson.purchaseIds ?? "";
                var idList = ids.Split(',').ToList();
                if (!idList.Contains(assetFilename))
                {
                    idList.Add(assetFilename);
                    UserStoreJson.purchaseIds = string.Join(",", idList);
                    UserStoreJson.version++;
                    AddPlant("P007");
                }
            }
            else if (assetFilename.Contains("CN_DLC_003"))
            {
                var ids = UserStoreJson.purchaseIds ?? "";
                var idList = ids.Split(',').ToList();
                if (!idList.Contains(assetFilename))
                {
                    idList.Add(assetFilename);
                    UserStoreJson.purchaseIds = string.Join(",", idList);
                    UserStoreJson.version++;
                    AddPlant("P007");
                    AddPlant("P008");
                }
            }
        }
    }
}