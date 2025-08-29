using System;
using System.Collections.Generic;
using System.Linq;
using Currency.Core.Run;
using Gameplay.Script.Data;
using Gameplay.Script.Gameplay;
using Gameplay.Script.Manager;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Script.Logic
{
    public class PlantUpgradeLogic : Single<PlantUpgradeLogic>
    {
        public UserPlantData UserPlantData { get; private set; }

        public UserDataJson GetUserData()
        {
            return UserDataManager.Instance.UserDataJson;
        }
        
        public List<string> GetPlantIds()
        {
            return PlantData.Instance.GetPlantIds();
        }
        
        public void SetSelectedPlant(UserPlantData plant)
        {
            UserPlantData = plant;
        }

        public PlantInherentData GetPlantData(string plantId)
        {
            return PlantData.Instance.GetPlantInherentData(plantId);
        }

        public UserPlantData[] GetUserPlantIds()
        {
            var userDataJson = UserDataManager.Instance.UserDataJson;
            var selfPlants = userDataJson.userPlants;
            return selfPlants;
        }
        
        public UserPlantData GetUserPlantData(int userPlantId)
        {
            if (userPlantId < 0)
            {
                Debug.LogWarning("Invalid userPlantId provided.");
                return null;
            }

            var userDataJson = UserDataManager.Instance?.UserDataJson;
            if (userDataJson == null || userDataJson.userPlants == null)
            {
                return null;
            }

            var selfPlantsList = userDataJson.userPlants.ToList();
            var plant = selfPlantsList.FirstOrDefault(value => value.userPlantId == userPlantId);
            return plant;
        }


        public PlantLevelData GetPlantLevelData(string plantId, int plantLevel)
        {
            return PlantData.Instance.GetPlantLevelData(plantId, plantLevel);
        }

        public PlantGradeData GetPlantGradeData(string plantId, int gradeLevel)
        {
            return PlantData.Instance.GetPlantGradeData(plantId, gradeLevel);
        }

        public PlantUITypeAsset GetPlantUITypeAsset(PlantType plantType)
        {
            return UIAssetsBindData.Instance.GetPlantUITypeAsset(plantType);
        }
        
        public PlantUIGradeAsset GetPlantUIGradeAsset(PlantGrade plantGrade)
        {
            return UIAssetsBindData.Instance.GetPlantUIGradeAsset(plantGrade);
        }
        
        public PlantIconAsset GetPlantIconAsset(string plantId)
        {
            return UIAssetsBindData.Instance.GetPlantIconAsset(plantId);
        }
        
        public void DoUplevel(int userPlantId, int coin)
        {
            UserDataManager.Instance.LevelUpPlant(userPlantId, coin);
        }

        public void DoUpgrade(int userPlantId, int debris)
        {
            UserDataManager.Instance.GradeUpPlant(userPlantId, debris);
        }

        public int Cost(int levelDataCoin)
        {
            var userData = UserDataManager.Instance.UserDataJson;
            return levelDataCoin <= userData?.coin ? levelDataCoin : 0;
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

        public GoodsItem GetStoreData(string plantDataDebrisId)
        {
            var userStore = UserDataManager.Instance.UserStoreJson;
            var itemList = userStore?.userGoodsJson?.goodsItem ?? Array.Empty<GoodsItem>();
            return itemList.ToList().FirstOrDefault(value => value.goodsId.Equals(plantDataDebrisId));
        }

        public UserElement GetUserElementDataList(string id)
        {
            var elementPlants = UserDataManager.Instance.UserDataJson?.userPlants ??
                                Array.Empty<UserPlantData>();
            var elementPlant = 
                elementPlants.ToList().FirstOrDefault(value => value.plantId.Equals(id));
            var element = elementPlant?.elementType ?? new UserElement();
            return element;
        }
        
        public ElementInherentData GetElementInherentData(string id)
        {
            return ElementData.Instance.GetElementInherentData(id);
        }

        public ElementLevelData GetElementLevelData(string id, int level)
        {
            return ElementData.Instance.GetElementLevelData(id, level);
        }
        
        public int ElementLevelUp(UserPlantData plant, UserElement element)
        {
            if (CanElementLevelUp(element.elementId, element.elementLevel))
            {
                UserDataManager.Instance.SetElementData(plant, element);
                return 1;
            }
            else
            {
                return 2;
            }
        }

        public bool CanElementLevelUp(string elementId, int level)
        {
            var inherentData = GetElementInherentData(elementId);
            if (int.TryParse(inherentData.maxLevel, out int maxLevel))
            {
                return level < maxLevel;
            }
            throw new Exception($"int parse failed : {inherentData.maxLevel}");
        }
    }
}