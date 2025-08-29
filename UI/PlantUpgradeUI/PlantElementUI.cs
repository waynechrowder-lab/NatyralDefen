using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class PlantElementUI : MonoBehaviour
    {
        [SerializeField] private GameObject elementPanel;
        [SerializeField] private GameObject elementParent;

        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text elementDebrisText;
        [SerializeField] private TMP_Text introduceText;
        
        [SerializeField] private Image iconImage;
        [SerializeField] private Image elementDebrisIconImage;
        [SerializeField] private VerticalLayoutGroup plantElementValueLayoutGroup;
        private List<Transform> _elementItems = new();
        private UserPlantData _selectedUserPlant;
        private void OnEnable()
        {
            for (int i = 0; i < elementParent.transform.childCount; i++)
                _elementItems.Add(elementParent.transform.GetChild(i));
            InvokeRepeating(nameof(UpdateLayout), .1f, .3f);
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        public void LoadElement()
        {
            elementPanel.SetActive(false);
            _elementItems.ForEach(value => value.gameObject.SetActive(false));
            _selectedUserPlant = PlantUpgradeLogic.Instance.UserPlantData;
            var userElement = _selectedUserPlant?.elementType;
            if (userElement == null || string.IsNullOrEmpty(userElement.elementId)) return;
            elementPanel.SetActive(true);
            var inherentData = PlantUpgradeLogic.Instance.GetElementInherentData(userElement.elementId);
            var levelData =
                PlantUpgradeLogic.Instance.GetElementLevelData(userElement.elementId, userElement.elementLevel);
            var nextLevelData =
                PlantUpgradeLogic.Instance.GetElementLevelData(userElement.elementId, userElement.elementLevel + 1);
            var plantLevelData =
                PlantUpgradeLogic.Instance.GetPlantLevelData(_selectedUserPlant.plantId, _selectedUserPlant.plantLevel);
            levelText.text = $"Lv{levelData.level}";
            var asset = UIAssetsBindData.Instance.GetElementIconAsset(userElement.elementId);
            iconImage.sprite = asset.elementIcon;
            nameText.text = inherentData.name;
            introduceText.text = inherentData.introduce;
            elementDebrisIconImage.sprite = asset.elementDebrisIcon;
            elementDebrisText.text = "0/100";
            if (userElement.elementId == "Element_01")
                SetFireElementValue(levelData, nextLevelData, plantLevelData);
            else if (userElement.elementId == "Element_02")
                SetElectricityElementValue(levelData, nextLevelData, plantLevelData);
            else if (userElement.elementId == "Element_03")
                SetPoisonElementValue(levelData, nextLevelData, plantLevelData);
            else if (userElement.elementId == "Element_04")
                SetIceElementValue(levelData, nextLevelData, plantLevelData);
        }

        void SetFireElementValue(ElementLevelData levelData, ElementLevelData nextLevelData, PlantLevelData plantLevelData)
        {
            var selectedItemList = _elementItems.Where(value 
                => value.gameObject.name.ToLower().Contains("fire")).ToList();
            selectedItemList.ForEach(value => value.gameObject.SetActive(true));
            if (selectedItemList is { Count: > 0 })
            {
                var item = selectedItemList[0];
                var textList = item.GetComponentsInChildren<TMP_Text>().ToList()
                    .Where(value => value.name.ToLower().Contains("value")).ToList();
                if (textList is { Count: > 0 })
                    textList[0].text = levelData.range.ToString("f2");
                if (textList is { Count: > 1 })
                    textList[1].text = nextLevelData.range.ToString("f2");
            }
                
            if (selectedItemList is { Count: > 1 })
            {
                var item = selectedItemList[1];
                var textList = item.GetComponentsInChildren<TMP_Text>().ToList()
                    .Where(value => value.name.ToLower().Contains("value")).ToList();
                if (textList is { Count: > 0 })
                    textList[0].text = (levelData.damage * plantLevelData.attackValue).ToString("f0");
                if (textList is { Count: > 1 })
                    textList[1].text = (nextLevelData.damage * plantLevelData.attackValue).ToString("f0");
            }
        }
        
        void SetElectricityElementValue(ElementLevelData levelData, ElementLevelData nextLevelData, PlantLevelData plantLevelData)
        {
            var selectedItemList = _elementItems.Where(value 
                => value.gameObject.name.ToLower().Contains("electricity")).ToList();
            selectedItemList.ForEach(value => value.gameObject.SetActive(true));
            if (selectedItemList is { Count: > 0 })
            {
                var item = selectedItemList[0];
                var textList = item.GetComponentsInChildren<TMP_Text>().ToList()
                    .Where(value => value.name.ToLower().Contains("value")).ToList();
                if (textList is { Count: > 0 })
                    textList[0].text = levelData.range.ToString("f2");
                if (textList is { Count: > 1 })
                    textList[1].text = nextLevelData.range.ToString("f2");
            }
                
            if (selectedItemList is { Count: > 1 })
            {
                var item = selectedItemList[1];
                var textList = item.GetComponentsInChildren<TMP_Text>().ToList()
                    .Where(value => value.name.ToLower().Contains("value")).ToList();
                if (textList is { Count: > 0 })
                    textList[0].text = levelData.range.ToString("f0");
                if (textList is { Count: > 1 })
                    textList[1].text = nextLevelData.range.ToString("f0");
            }
            
            if (selectedItemList is { Count: > 2 })
            {
                var item = selectedItemList[2];
                var textList = item.GetComponentsInChildren<TMP_Text>().ToList()
                    .Where(value => value.name.ToLower().Contains("value")).ToList();
                if (textList is { Count: > 0 })
                    textList[0].text = (levelData.damage * plantLevelData.attackValue).ToString("f0");
                if (textList is { Count: > 1 })
                    textList[1].text = (nextLevelData.damage * plantLevelData.attackValue).ToString("f0");
            }
        }
        
        void SetPoisonElementValue(ElementLevelData levelData, ElementLevelData nextLevelData, PlantLevelData plantLevelData)
        {
            var selectedItemList = _elementItems.Where(value 
                => value.gameObject.name.ToLower().Contains("poison")).ToList();
            selectedItemList.ForEach(value => value.gameObject.SetActive(true));
            if (selectedItemList is { Count: > 0 })
            {
                var item = selectedItemList[0];
                var textList = item.GetComponentsInChildren<TMP_Text>().ToList()
                    .Where(value => value.name.ToLower().Contains("value")).ToList();
                if (textList is { Count: > 0 })
                    textList[0].text = levelData.duration.ToString("f2");
                if (textList is { Count: > 1 })
                    textList[1].text = nextLevelData.duration.ToString("f2");
            }
            
            if (selectedItemList is { Count: > 1 })
            {
                var item = selectedItemList[1];
                var textList = item.GetComponentsInChildren<TMP_Text>().ToList()
                    .Where(value => value.name.ToLower().Contains("value")).ToList();
                if (textList is { Count: > 0 })
                    textList[0].text = (levelData.damage * plantLevelData.attackValue).ToString("f0");
                if (textList is { Count: > 1 })
                    textList[1].text = (nextLevelData.damage * plantLevelData.attackValue).ToString("f0");
            }
        }
        
        void SetIceElementValue(ElementLevelData levelData, ElementLevelData nextLevelData, PlantLevelData plantLevelData)
        {
            var selectedItemList = _elementItems.Where(value 
                => value.gameObject.name.ToLower().Contains("ice")).ToList();
            selectedItemList.ForEach(value => value.gameObject.SetActive(true));
            if (selectedItemList is { Count: > 0 })
            {
                var item = selectedItemList[0];
                var textList = item.GetComponentsInChildren<TMP_Text>().ToList()
                    .Where(value => value.name.ToLower().Contains("value")).ToList();
                if (textList is { Count: > 0 })
                    textList[0].text = levelData.duration.ToString("f2");
                if (textList is { Count: > 1 })
                    textList[1].text = nextLevelData.duration.ToString("f2");
            }
            
            if (selectedItemList is { Count: > 1 })
            {
                var item = selectedItemList[1];
                var textList = item.GetComponentsInChildren<TMP_Text>().ToList()
                    .Where(value => value.name.ToLower().Contains("value")).ToList();
                if (textList is { Count: > 0 })
                    textList[0].text = levelData.damage.ToString("f2");
                if (textList is { Count: > 1 })
                    textList[1].text = nextLevelData.damage.ToString("f2");
            }
            
            if (selectedItemList is { Count: > 2 })
            {
                var item = selectedItemList[2];
                var textList = item.GetComponentsInChildren<TMP_Text>().ToList()
                    .Where(value => value.name.ToLower().Contains("value")).ToList();
                if (textList is { Count: > 0 })
                    textList[0].text = levelData.count.ToString("f2");
                if (textList is { Count: > 1 })
                    textList[1].text = nextLevelData.count.ToString("f2");
            }
        }

        VerticalLayoutGroup _plantElementValueLayoutGroup;
        void UpdateLayout()
        {
            if (!_plantElementValueLayoutGroup)
                _plantElementValueLayoutGroup = 
                    elementPanel.GetComponentInChildren<VerticalLayoutGroup>();
            if (_plantElementValueLayoutGroup) 
                _plantElementValueLayoutGroup.enabled = !_plantElementValueLayoutGroup.enabled;
        }
    }
}