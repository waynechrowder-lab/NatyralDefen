using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Script.Data;
using Gameplay.Script.Gameplay;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using Gameplay.Script.UI;
using UnityEngine;

public class PlantHandbookUI : MonoBehaviour
{

    [SerializeField] private Transform plantParent;
    [SerializeField] private PlantIntroduction introducePanel;
    private GameObject _plantItem;
    private void OnEnable()
    {
        introducePanel.gameObject.SetActive(false);
        SetPlantItem();
    }

    void SetPlantItem()
    {
        if (!_plantItem)
        {
            _plantItem = plantParent.GetChild(0).gameObject;
            _plantItem.SetActive(false);
        }

        var assets = GameResourcesMgr.Instance.PlantAssets;
        assets = assets.OrderBy(value => value.plantGrade).ToList();
        int count = assets?.Count ?? 0;
        for (int i = 0; i < plantParent.childCount; i++)
        {
            GameObject item = plantParent.GetChild(i).gameObject;
            if (i < count)
            {
                var userPlant = assets[i];
                var inhereData = PlantUpgradeLogic.Instance.GetPlantData(userPlant.plantName);
                item.SetActive(true);
                item.GetComponent<PlantUpgradeItem>().InitItem(inhereData, OnClickIntroduce);
            }
            else
                item.SetActive(false);
        }

        for (int i = plantParent.childCount; i < count; i++)
        {
            GameObject item = Instantiate(_plantItem, plantParent);
            var userPlant = assets[i];
            var inhereData = PlantUpgradeLogic.Instance.GetPlantData(userPlant.plantName);
            item.SetActive(true);
            item.GetComponent<PlantUpgradeItem>().InitItem(inhereData, OnClickIntroduce);
        }
    }

    void OnSelect(UserPlantData userPlant)
    {

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
