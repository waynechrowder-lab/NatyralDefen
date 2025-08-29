
using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Script.Data;
using Gameplay.Script.Gameplay;
using Gameplay.Script.Logic;
using UnityEngine;

namespace Gameplay.Script.UI
{
    public class GameLevelPlantSelectUI : MonoBehaviour
    {
        [SerializeField] private Transform plantParent;
        [SerializeField] private PlantIntroduction introducePanel;
        private GameObject _plantItem;
        private void OnEnable()
        {
            SetPlantItem();
        }

        void SetPlantItem()
        {
            if (!_plantItem)
            {
                _plantItem = plantParent.GetChild(0).gameObject;
                _plantItem.SetActive(false);
            }

            var userPlants = PlantUpgradeLogic.Instance.GetUserPlantIds() ?? Array.Empty<UserPlantData>();
            var temp = new List<UserPlantData>();
            for (int i = 0; i < (userPlants?.Length ?? 0) ; i++)
            {
                if (temp.Any(value => value.plantId == userPlants[i].plantId))
                    continue;
                temp.Add(userPlants[i]);
            }
            int count = temp?.Count ?? 0;
            for (int i = 0; i < plantParent.childCount; i++)
            {
                GameObject item = plantParent.GetChild(i).gameObject;
                if (i < count)
                {
                    var userPlant = temp[i];
                    item.SetActive(true);
                    item.GetComponent<PlantUpgradeItem>().InitItem(userPlant, OnSelect, OnClickIntroduce);
                }
                else 
                    item.SetActive(false);
            }

            for (int i = plantParent.childCount; i < count; i++)
            {
                GameObject item = Instantiate(_plantItem, plantParent);
                var userPlant = temp[i];
                item.SetActive(true);
                item.GetComponent<PlantUpgradeItem>().InitItem(userPlant, OnSelect, OnClickIntroduce);
            }
        }

        void OnSelect(UserPlantData userPlant)
        {
            GameLevelLogic.Instance.AddBagPlant(userPlant.userPlantId);
            gameObject.SetActive(false);
        }

        void OnClickIntroduce(string plantId)
        {
            introducePanel.gameObject.SetActive(true);
            introducePanel.LoadPlantData(plantId);
        }

        public void OnClickClose()
        {
            gameObject.SetActive(false);
        }
    }
}