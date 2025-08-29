using cn.bmob.api;
using cn.bmob.exception;
using cn.bmob.io;
using cn.bmob.response;
using Currency.Core.Run;
using Gameplay.Script.ConstValue;
using Gameplay.Script.Data;
using Gameplay.Script.Gameplay;
using Script.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Script.Bmob
{
    public class BmobManager : MonoSingle<BmobManager>
    {
        private static BmobUnity _bmob;
        private MiniWorldUser _bmobUser;
        public MiniWorldUser MiniWorldUser => _bmobUser;
        
        private void Start()
        {
            _bmob = gameObject.GetComponent<BmobUnity>();
        }
        
        public void OnUserLogin(UserPicoJson userPico, Action loginCallback) 
        {
            BmobQuery query = new BmobQuery();
            query.Limit(1);
            query.WhereEqualTo("relevanceId", $"{userPico.relevanceId}");
            _bmob.Find<MiniWorldUser>(PicoPlatformConstValue.UserTable, query, FindUserCallback);

            void FindUserCallback(QueryCallbackData<MiniWorldUser> resp, Exception exception)
            {
                Debug.Log($"用户登录成功");
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    return;
                }
                List<MiniWorldUser> list = resp.results;
                if (list == null || list.Count < 1)
                {
                    MiniWorldUser data = new MiniWorldUser();
                    data.relevanceId = userPico.relevanceId;
                    data.name = userPico.displayName;
                    // data.uuid = ;
                    data.gender = new BmobInt(userPico.gender);
                    data.headUrl = userPico.smallImageUrl;
                    data.second = 0;
                    data.point = 0;
                    data.bagPlants = "";
                    _bmobUser = data;
                    _bmob.Create(PicoPlatformConstValue.UserTable, data, CreateUserCallback);
                }
                else
                {
                    MiniWorldUser soulTopiaUser = list[0];
                    _bmobUser = soulTopiaUser;
                    MiniWorldUser data = new MiniWorldUser();
                    bool needUpdate = false;
                    if (!soulTopiaUser.name.Equals(userPico.displayName))
                    {
                        needUpdate = true;
                        data.name = userPico.displayName;
                    }
                    if (soulTopiaUser.gender != null && !soulTopiaUser.gender.Get().Equals(userPico.gender))
                    {
                        needUpdate = true;
                        data.gender = userPico.gender;
                    }
                    if (!soulTopiaUser.headUrl.Equals(userPico.smallImageUrl))
                    {
                        needUpdate = true;
                        data.headUrl = userPico.smallImageUrl;
                    }
                    if (needUpdate)
                        _bmob.Update(PicoPlatformConstValue.UserTable, soulTopiaUser.objectId, data, UpdateUserCallback);
                    if (string.IsNullOrEmpty(soulTopiaUser.regionId) || soulTopiaUser.regionId == "0")
                        loginCallback?.Invoke();
                }
            
                void CreateUserCallback(CreateCallbackData resp, Exception exception)
                {
                    if (exception != null)
                    {
                        Debug.LogError("保存失败, 失败原因为： " + exception.Message);
                        return;
                    }
                    _bmob.Get<MiniWorldUser>(PicoPlatformConstValue.UserTable, resp.objectId, FindCallback);
                    loginCallback?.Invoke();
                }

                void UpdateUserCallback(UpdateCallbackData resp, Exception exception)
                {
                    if (exception != null)
                    {
                        Debug.LogError("保存失败, 失败原因为： " + exception.Message);
                        return;
                    }
                }

                void FindCallback(MiniWorldUser data, Exception exception)
                {
                    _bmobUser = data;
                }
            }
        }

        public void UpdateUuid(string objectId, int uuid)
        {
            MiniWorldUser user = new MiniWorldUser();
            user.uuid = uuid;
            _bmob.Update(PicoPlatformConstValue.UserTable, objectId, user, UpdateUserCallback);

            void UpdateUserCallback(UpdateCallbackData resp, Exception exception)
            {
                if (exception != null)
                {
                    Debug.LogError("保存失败, 失败原因为： " + exception.Message);
                    return;
                }
                Debug.Log($"UUID:{uuid}");
            }
        }

        public void QueryUser(int limit, Action<List<MiniWorldUser>> onQueryCallback)
        {
            BmobQuery query = new BmobQuery();
            query.OrderBy("uuid");
            query.WhereEqualTo("uuid", null);
            query.Limit(limit);
            _bmob.Find<MiniWorldUser>(PicoPlatformConstValue.UserTable, query, FindPackCallback);

            void FindPackCallback(QueryCallbackData<MiniWorldUser> resp, Exception exception)
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    onQueryCallback?.Invoke(new List<MiniWorldUser>());
                    return;
                }
                Debug.Log($"查询用户数量：{resp.results?.Count}");
                onQueryCallback?.Invoke(resp.results ?? new List<MiniWorldUser>());
            }
        }

        public void QueryRank(Action<List<MiniWorldUser>> onQueryCallback)
        {
            BmobQuery query = new BmobQuery();
            query.Limit(50);
            query.WhereGreaterThan("second", 0);
            query.ThenByDescending("second");
            _bmob.Find<MiniWorldUser>(PicoPlatformConstValue.UserTable, query, FindPackCallback);

            void FindPackCallback(QueryCallbackData<MiniWorldUser> resp, Exception exception)
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    onQueryCallback?.Invoke(new List<MiniWorldUser>());
                    return;
                }
                onQueryCallback?.Invoke(resp.results ?? new List<MiniWorldUser>());
            }
        }
        
        public void UpdateRank(int second, int point, List<string> list)
        {
            if (UserData.Instance.CurrentUser != null && _bmobUser != null)
            {
                if (_bmobUser != null && !string.IsNullOrEmpty(_bmobUser.objectId))
                {
                    var se = _bmobUser.second?.Get() ?? 0;
                    if (se <= 0)
                        _bmobUser.second = second;
                    else
                        _bmobUser.second = Mathf.Max(se, second);
                    if (se < second)
                    {
                        _bmobUser.bagPlants = string.Join(",", list);
                        MiniWorldUser data = new MiniWorldUser();
                        data.second = _bmobUser.second;
                        data.bagPlants = _bmobUser.bagPlants;
                        _bmob.Update(PicoPlatformConstValue.UserTable, _bmobUser.objectId, data, UpdateCallback);
                    }
                }
            }
            void UpdateCallback(UpdateCallbackData response, BmobException exception)
            {
                if (exception != null)
                {
                    Debug.LogError("保存失败, 失败原因为： " + exception.Message);
                    return;
                }
                Debug.Log("更新成功");
            }
        }

        public void QueryRankQuickMode(QuickGameMode quickGameMode, Action<List<MiniWorldUser>> onQueryCallback)
        {
            BmobQuery query = new BmobQuery();
            string secondColumn = $"second{quickGameMode.GetDescByEnum()}";
            query.Limit(50);
            query.WhereGreaterThan(secondColumn, 0);
            query.ThenBy(secondColumn);
            _bmob.Find<MiniWorldUser>(PicoPlatformConstValue.UserTable, query, FindPackCallback);

            void FindPackCallback(QueryCallbackData<MiniWorldUser> resp, Exception exception)
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    onQueryCallback?.Invoke(new List<MiniWorldUser>());
                    return;
                }
                onQueryCallback?.Invoke(resp.results ?? new List<MiniWorldUser>());
            }
        }

        public void UpdateRankQuickMode(QuickGameMode quickGameMode, int second, int point, List<string> list)
        {
            string secondColumn = $"second{quickGameMode.GetDescByEnum()}";
            if (UserData.Instance.CurrentUser != null && _bmobUser != null)
            {
                if (_bmobUser != null && !string.IsNullOrEmpty(_bmobUser.objectId))
                {
                    var se = 0;
                    if (quickGameMode == QuickGameMode.Mode1)
                    {
                        se = _bmobUser.secondmode1?.Get() ?? 0;
                        if (se <= 0)
                            _bmobUser.secondmode1 = second;
                        else
                            _bmobUser.secondmode1 = Mathf.Min(se, second);
                    }
                    else if (quickGameMode == QuickGameMode.Mode2)
                    {
                        se = _bmobUser.secondmode2?.Get() ?? 0;
                        if (se <= 0)
                            _bmobUser.secondmode2 = second;
                        else
                            _bmobUser.secondmode2 = Mathf.Min(se, second);
                    }
                    else if (quickGameMode == QuickGameMode.Mode3)
                    {
                        se = _bmobUser.secondmode3?.Get() ?? 0;
                        if (se <= 0)
                            _bmobUser.secondmode3 = second;
                        else
                            _bmobUser.secondmode3 = Mathf.Min(se, second);
                    }
                    if ((se <= 0) || (se > 0 && se > second))
                    {
                        MiniWorldUser data = new MiniWorldUser();
                        if (quickGameMode == QuickGameMode.Mode1)
                            data.secondmode1 = _bmobUser.secondmode1;
                        else if (quickGameMode == QuickGameMode.Mode2)
                        {
                            data.secondmode2 = _bmobUser.secondmode2;
                            _bmobUser.bagPlantsMode2 = string.Join(",", list);
                            data.bagPlantsMode2 = _bmobUser.bagPlantsMode2;
                        }
                        else if (quickGameMode == QuickGameMode.Mode3)
                            data.secondmode3 = _bmobUser.secondmode3;
                        _bmob.Update(PicoPlatformConstValue.UserTable, _bmobUser.objectId, data, UpdateCallback);
                    }
                }
            }
            void UpdateCallback(UpdateCallbackData response, BmobException exception)
            {
                if (exception != null)
                {
                    Debug.LogError("保存失败, 失败原因为： " + exception.Message);
                    return;
                }
                Debug.Log("更新成功");
            }
        }

        public void GetMiniWorldUserData(string id, UserDataJson jsonData, Action<MiniWorldUserGameData> getCallback,
            Action<string> createCallback)
        {
            BmobQuery query = new BmobQuery ();
            query.Limit(1);
            query.WhereEqualTo("relevanceId", $"{id}");
            _bmob.Find<MiniWorldUserGameData>(PicoPlatformConstValue.UserGameDataTable, query, (resp, exception)=>
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    return;
                }
                List<MiniWorldUserGameData> list = resp.results;
                MiniWorldUserGameData data = new MiniWorldUserGameData();
                if (list.Count < 1)
                {
                    data.relevanceId = id;
                    data.version = 0;
                    _bmob.Create(PicoPlatformConstValue.UserGameDataTable, data, (subresp, subexception) =>
                    {
                        if (subexception != null)
                        {
                            Debug.LogError("保存失败, 失败原因为： " + subexception.Message);
                            return;
                        }
                        createCallback?.Invoke(subresp.objectId);
                    });
                }
                else
                {
                    data = list[0];
                    if ((data.version?.Get() ?? 0) < jsonData.version)
                    {
                        // AndroidJosnTool.WriteJsonString(jsonData.userPlantJson, out string json);
                        data.userPlantJson = jsonData.UserPlantToJsonString();
                        data.version = jsonData.version;
                        data.coin = jsonData.coin;
                        data.jewel = jsonData.jewel;
                        data.exp = jsonData.exp;
                        data.userSubscribe = jsonData.SubscribeToJsonString();
                        data.signIn = string.Join(",", jsonData.signIn);
                        data.userAchievements = string.Join(",", jsonData.userAchievements);
                        data.userBagPlants = string.Join(",", jsonData.userBagPlants);
                        data.userBagPlantsM2 = string.Join(",", jsonData.userBagPlantsM2);
                        UpdateMiniWorldUserData(data.objectId, jsonData, null);
                    }
                    getCallback?.Invoke(data);
                }
            });
        }
        
        public void UpdateMiniWorldUserData(string objectId, UserDataJson jsonData, Action<bool> callback)
        {
            // AndroidJosnTool.WriteJsonString(jsonData.userPlantJson, out string json);
            MiniWorldUserGameData data = new MiniWorldUserGameData();
            data.userPlantJson = jsonData.UserPlantToJsonString();
            data.version = jsonData.version;
            data.coin = jsonData.coin;
            data.exp = jsonData.exp;
            data.jewel = jsonData.jewel;
            data.userSubscribe = jsonData.SubscribeToJsonString();
            data.signIn = string.Join(",", jsonData.signIn);
            data.userAchievements = string.Join(",", jsonData.userAchievements);
            data.userBagPlants = string.Join(",", jsonData.userBagPlants);
            data.userBagPlantsM2 = string.Join(",", jsonData.userBagPlantsM2);
            _bmob.Update(PicoPlatformConstValue.UserGameDataTable, objectId, data, (resp, exception) =>
            {
                callback?.Invoke(exception == null);
                if (exception != null)
                {
                    Debug.LogError("保存失败, 失败原因为： " + exception.Message);
                    return;
                }
            });
        }
        
        public void GetMiniWorldUserStoreData(string id, UserStoreJson jsonData, Action<MiniWorldUserStoreData> getCallback,
            Action<string> createCallback)
        {
            BmobQuery query = new BmobQuery ();
            query.Limit(1);
            query.WhereEqualTo("relevanceId", $"{id}");
            _bmob.Find<MiniWorldUserStoreData>(PicoPlatformConstValue.UserStoreTable, query, (resp, exception)=>
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    return;
                }
                List<MiniWorldUserStoreData> list = resp.results;
                MiniWorldUserStoreData data = new MiniWorldUserStoreData();
                if (list.Count < 1)
                {
                    data.relevanceId = id;
                    data.version = 0;
                    data.purchaseIds = "";
                    _bmob.Create(PicoPlatformConstValue.UserStoreTable, data, (subresp, subexception) =>
                    {
                        if (subexception != null)
                        {
                            Debug.LogError("保存失败, 失败原因为： " + subexception.Message);
                            return;
                        }
                        createCallback?.Invoke(subresp.objectId);
                    });
                }
                else
                {
                    data = list[0];
                    if ((data.version?.Get() ?? 0) < jsonData.version)
                    {
                        data.goodsJson = jsonData.UserGoodsToJsonString();
                        data.unlockJson = jsonData.UserUnlockItemsToJsonString();
                        data.version = jsonData.version;
                        data.purchaseIds = jsonData.purchaseIds;
                        UpdateMiniWorldStoreData(data.objectId, jsonData, null);
                    }
                    getCallback?.Invoke(data);
                }
            });
        }
        
        public void UpdateMiniWorldStoreData(string objectId, UserStoreJson jsonData, Action<bool> callback)
        {
            MiniWorldUserStoreData data = new MiniWorldUserStoreData();
            data.goodsJson = jsonData.UserGoodsToJsonString();
            data.unlockJson = jsonData.UserUnlockItemsToJsonString();
            data.version = jsonData.version;
            data.purchaseIds = jsonData.purchaseIds;
            _bmob.Update(PicoPlatformConstValue.UserStoreTable, objectId, data, (resp, exception) =>
            {
                callback?.Invoke(exception == null);
                if (exception != null)
                {
                    Debug.LogError("保存失败, 失败原因为： " + exception.Message);
                    return;
                }
            });
        }        
        
        public void GetUserInfo(string userId, Action<MiniWorldUser> callback)
        {
            BmobQuery query = new BmobQuery();
            query.Limit(1);
            query.WhereEqualTo("relevanceId", $"{userId}");
            _bmob.Find<MiniWorldUser>(PicoPlatformConstValue.UserTable, query, FindUserCallback);
        
            void FindUserCallback(QueryCallbackData<MiniWorldUser> resp, Exception exception)
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    return;
                }
                List<MiniWorldUser> list = resp.results;
                if (list.Count < 1)
                    callback?.Invoke(null);
                else
                    callback?.Invoke(list[0]);
            }
        }
        
        public void GetMiniWorldUserRankingList(string orderColumn, int limit, 
            Action<List<MiniWorldUserRankingData>> callback)
        {
            BmobQuery query = new BmobQuery();
            query.Limit(limit);
            query.WhereNotEqualTo(orderColumn, 0);
            query.ThenByDescending(orderColumn);
            string table = PicoPlatformConstValue.UserRankingTable;
            _bmob.Find<MiniWorldUserRankingData>(table, query, FindPackCallback);

            void FindPackCallback(QueryCallbackData<MiniWorldUserRankingData> resp, Exception exception)
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    return;
                }
                callback?.Invoke(resp.results ?? new List<MiniWorldUserRankingData>());
            }
        
        }
        
        public void GetMiniWorldUserRanking(string relevanceId, Action<MiniWorldUserRankingData> action)
        {
            BmobQuery query = new BmobQuery();
            query.Limit(1);
            query.WhereEqualTo("relevanceId", $"{relevanceId}");
            string table = PicoPlatformConstValue.UserRankingTable;
            _bmob.Find<MiniWorldUserRankingData>(table, query, (resp, exception) =>
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    return;
                }
                action?.Invoke(resp.results is { Count: > 0 } ? resp.results[0] : null);
            });
        }

        public void CreateMiniWorldUserRanking(MiniWorldUserRankingData data, Action<string> action)
        {
            string table = PicoPlatformConstValue.UserRankingTable;
            _bmob.Create(table, data, (response, exception) =>
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    return;
                }
                action?.Invoke(response.objectId);
            });
        }
    
        public void UpdateMiniWorldUserRanking(string objectId, MiniWorldUserRankingData data, Action action)
        {
            string table = PicoPlatformConstValue.UserRankingTable;
            _bmob.Update(table, objectId, data, (response, exception) =>
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    return;
                }
                action?.Invoke();
            });
        }

        public void GetGameNotice(Action<List<MiniWorldUNotice>> action)
        {
            string table = PicoPlatformConstValue.NoticeTable;
            BmobQuery query = new BmobQuery();
            query.Limit(100);
            query.WhereEqualTo("valid", 1);
            _bmob.Find<MiniWorldUNotice>(table, query, MiniWorldUNoticeCallback);
            void MiniWorldUNoticeCallback(QueryCallbackData<MiniWorldUNotice> response, BmobException exception)
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    return;
                }
                List<MiniWorldUNotice> list = response.results;
                if (list is { Count: > 0 })
                    action(list);
            }
        }
        
        public void GetGameShop(Action<List<MiniWorldShopData>> action)
        {
            string table = PicoPlatformConstValue.ShopTable;
            BmobQuery query = new BmobQuery();
            query.Limit(100);
            query.WhereEqualTo("visible", 1);
            _bmob.Find<MiniWorldShopData>(table, query, MiniWorldUShopCallback);
            void MiniWorldUShopCallback(QueryCallbackData<MiniWorldShopData> response, BmobException exception)
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    return;
                }
                List<MiniWorldShopData> list = response.results;
                if (list is { Count: > 0 })
                    action(list);
            }
        }
        
        public void GetGameUnlockItems(Action<List<MiniWorldUnlockItemData>> action)
        {
            string table = PicoPlatformConstValue.UnlockTable;
            BmobQuery query = new BmobQuery();
            query.Limit(50);
            // query.WhereEqualTo("visible", 1);
            _bmob.Find<MiniWorldUnlockItemData>(table, query, MiniWorldUShopCallback);
            void MiniWorldUShopCallback(QueryCallbackData<MiniWorldUnlockItemData> response, BmobException exception)
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    return;
                }
                List<MiniWorldUnlockItemData> list = response.results;
                if (list is { Count: > 0 })
                {
                    var version = Application.version;
                    List<MiniWorldUnlockItemData> newList = new List<MiniWorldUnlockItemData>();
                    list.ForEach(value =>
                    {
                        var contains = value.versions.Split(',').ToList().Contains(version);
                        if (contains)
                        {
                            newList.Add(value);
                        }
                    });
                    action(newList);
                }
            }
        }

        public void GetGameStoreAssets(Action<List<MiniWorldShopData>> action)
        {
            string table = PicoPlatformConstValue.ShopTable;
            BmobQuery query = new BmobQuery();
            query.Limit(50);
            query.WhereEqualTo("visible", 1);
            _bmob.Find<MiniWorldShopData>(table, query, MiniWorldUShopCallback);
            
            void MiniWorldUShopCallback(QueryCallbackData<MiniWorldShopData> response, BmobException exception)
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    return;
                }
                List<MiniWorldShopData> list = response.results;
                action?.Invoke(list);
            }
        }
        
        public void GetGameActivities(Action<List<MiniWorldActivity>> action)
        {
            string table = PicoPlatformConstValue.ActivityTable;
            BmobQuery query = new BmobQuery();
            var version = Application.version;
            query.WhereEqualTo("version", version);
            query.WhereEqualTo("visible", 1);
            query.Limit(50);
            _bmob.Find<MiniWorldActivity>(table, query, MiniWorldUShopCallback);
            
            void MiniWorldUShopCallback(QueryCallbackData<MiniWorldActivity> response, BmobException exception)
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    return;
                }
                List<MiniWorldActivity> list = response.results;
                action?.Invoke(list);
            }
        }
        
        public void GetGamePlantAssets(Action<List<MiniWorldPlant>> action)
        {
            string table = PicoPlatformConstValue.ActivityTable;
            BmobQuery query = new BmobQuery();
            query.Limit(100);
            _bmob.Find<MiniWorldPlant>(table, query, MiniWorldUShopCallback);
            
            void MiniWorldUShopCallback(QueryCallbackData<MiniWorldPlant> response, BmobException exception)
            {
                if (exception != null)
                {
                    Debug.LogError("查询失败, 失败原因为： " + exception.Message);
                    return;
                }
                List<MiniWorldPlant> list = response.results;
                action?.Invoke(list);
            }
        }
    }
}