using System;
using System.Linq;
using UnityEngine;
using Gameplay.Script.Data;
using Gameplay.Script.Gameplay;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using TMPro;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class PlantUpgradeUI : BasedUI
    {
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private TMP_Text jewelText;
        [SerializeField] private TMP_Text plantGradeName;
        [SerializeField] private TMP_Text plantName;
        [SerializeField] private TMP_Text plantAttribute;
        [SerializeField] private TMP_Text plantLevel;
        [SerializeField] private TMP_Text plantLevel2;
        [SerializeField] private TMP_Text plantAttackValue;
        [SerializeField] private TMP_Text plantDefenseValue;
        [SerializeField] private TMP_Text plantHealthValue;
        [SerializeField] private TMP_Text plantAttackNewValue;
        [SerializeField] private TMP_Text plantDefenseNewValue;
        [SerializeField] private TMP_Text plantHealthNewValue;
        [SerializeField] private TMP_Text plantDebrisValue;
        [SerializeField] private TMP_Text plantCoinValue;
        [SerializeField] private Image plantGradeIcon;
        [SerializeField] private Image plantIcon;
        [SerializeField] private Transform plantGradeParent;
        [SerializeField] private Transform plantParent;

        [SerializeField] private PlantElementUI plantElementUI;
        private GameObject _plantItem;
        PlantUpgradeItem _selectedItem;
        UserPlantData _selectedUserPlant;
        private int _coinCost = 0;
        private int _debrisCost = 0;
        protected override void OnEnable()
        {
            base.OnEnable();
            SetPlantItem();
            InvokeRepeating(nameof(UpdateLayout), .1f, .3f);
            PlantUpgradeLogic.Instance.RegisterExpendCallback(OnExpend);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelInvoke();
            PlantUpgradeLogic.Instance.UnRegisterExpendCallback(OnExpend);
        }

        void OnExpend(UserDataJson json)
        {
            coinText.text = (json?.coin ?? 0).ToString();
            // jewelText.text = jewel.ToString();
        }

        void SetPlantItem()
        {
            if (!_plantItem)
            {
                _plantItem = plantParent.GetChild(0).gameObject;
                _plantItem.SetActive(false);
            }
            var userPlants = PlantUpgradeLogic.Instance.GetUserPlantIds();
            int count = userPlants?.Length ?? 0;
            for (int i = 0; i < plantParent.childCount; i++)
            {
                GameObject item = plantParent.GetChild(i).gameObject;
                if (i < count)
                {
                    var plant = userPlants[i];
                    item.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                    item.GetComponent<Toggle>().onValueChanged
                        .AddListener(b => OnValueChanged(item.GetComponent<PlantUpgradeItem>()));
                    item.SetActive(true);
                    item.GetComponent<PlantUpgradeItem>().InitItem(plant, OnSelect, null);
                }
                else 
                    item.SetActive(false);
            }

            for (int i = plantParent.childCount; i < count; i++)
            {
                GameObject item = Instantiate(_plantItem, plantParent);
                var plant = userPlants[i];
                item.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                item.GetComponent<Toggle>().onValueChanged
                    .AddListener(b => OnValueChanged(item.GetComponent<PlantUpgradeItem>()));
                item.SetActive(true);
                item.GetComponent<PlantUpgradeItem>().InitItem(plant, OnSelect, null);
            }

            if (!_selectedItem && count > 0)
            {
                _selectedItem = plantParent.GetChild(0).GetComponent<PlantUpgradeItem>();
                _selectedItem.GetComponent<Toggle>().isOn = true;
                _selectedItem.OnSelect();
            }
        }

        void OnValueChanged(PlantUpgradeItem selectedItem)
        {
            if (_selectedItem && selectedItem)
            {
                if (!Equals(selectedItem, _selectedItem))
                {
                    _selectedItem.OnRelease();
                    _selectedItem = selectedItem;
                    _selectedItem.OnSelect();
                }
                _selectedItem.GetComponent<Toggle>().isOn = true;
            }
        }
        
        private void OnSelect(UserPlantData plant)
        {
            _selectedUserPlant = plant;
            PlantUpgradeLogic.Instance.SetSelectedPlant(plant);
            LoadPlantData();
        }

        private void LoadPlantData()
        {
            _coinCost = _debrisCost = 0;
            var userPlant = _selectedUserPlant;
            plantElementUI.LoadElement();
            var plantData = PlantUpgradeLogic.Instance.GetPlantData(userPlant.plantId);
            Enum.TryParse<PlantType>(plantData.type, out var plantType);
            Enum.TryParse<PlantGrade>(plantData.quality, out var plantGrade);
            var typeAsset = PlantUpgradeLogic.Instance.GetPlantUITypeAsset(plantType);
            var gradeAsset = PlantUpgradeLogic.Instance.GetPlantUIGradeAsset(plantGrade);
            var plantAsset = PlantUpgradeLogic.Instance.GetPlantIconAsset(userPlant.plantId);

            plantGradeName.text = gradeAsset.plantGradeName;
            plantName.text = plantData.name;
            plantAttribute.text = plantData.attribute;
            plantLevel.text = userPlant.plantLevel.ToString();
            plantLevel2.text = $"等级:{userPlant.plantLevel}";
            var currentLevel = PlantUpgradeLogic.Instance.GetPlantLevelData(userPlant.plantId, userPlant.plantLevel);
            if (currentLevel == null) return;
            plantAttackValue.text = currentLevel.attackValue.ToString();
            plantDefenseValue.text = currentLevel.defenseValue.ToString();
            plantHealthValue.text = currentLevel.health.ToString();
            if (userPlant.plantGrade == plantData.maxGrade)
            {
                plantDebrisValue.text = "已达最大星级";
                if (userPlant.plantLevel == plantData.maxLevel)
                {
                    plantCoinValue.text = "已达最大等级";
                    plantAttackNewValue.text = "最大";
                    plantDefenseNewValue.text = "最大";
                    plantHealthNewValue.text = "最大";
                }
                else
                {
                    int nextLevel = userPlant.plantLevel + 1;
                    var levelData = PlantUpgradeLogic.Instance.GetPlantLevelData(userPlant.plantId, nextLevel);
                    if (levelData == null) return;
                    plantCoinValue.text = levelData.coin.ToString();
                    plantAttackNewValue.text = levelData.attackValue.ToString();
                    plantDefenseNewValue.text = levelData.defenseValue.ToString();
                    plantHealthNewValue.text = levelData.health.ToString();
                    _coinCost = PlantUpgradeLogic.Instance.Cost(levelData.coin);
                }
            }
            else
            {
                int nextGrade = userPlant.plantGrade + 1;
                var gradeData = PlantUpgradeLogic.Instance.GetPlantGradeData(userPlant.plantId, nextGrade);
                if (gradeData == null) return;
                Data.GoodsItem storeData = PlantUpgradeLogic.Instance.GetStoreData(plantData.debrisId);
                plantDebrisValue.text = $"{storeData?.goodsNum ?? 0}/{gradeData.debris}";
                if (userPlant.plantLevel >= userPlant.plantGrade * 10)
                {
                    plantCoinValue.text = "提升星级解锁";
                    plantAttackNewValue.text = gradeData.attackValue.ToString();
                    plantDefenseNewValue.text = gradeData.defenseValue.ToString();
                    plantHealthNewValue.text = gradeData.health.ToString();
                    _debrisCost = gradeData.debris <= (storeData?.goodsNum ?? 0) ? gradeData.debris : 0;
                }
                else
                {
                    int nextLevel = userPlant.plantLevel + 1;
                    var levelData = PlantUpgradeLogic.Instance.GetPlantLevelData(userPlant.plantId, nextLevel);
                    if (levelData == null) return;
                    plantCoinValue.text = levelData.coin.ToString();
                    plantAttackNewValue.text = levelData.attackValue.ToString();
                    plantDefenseNewValue.text = levelData.defenseValue.ToString();
                    plantHealthNewValue.text = levelData.health.ToString();
                    _coinCost = _coinCost = PlantUpgradeLogic.Instance.Cost(levelData.coin);
                }
            }

            plantGradeIcon.sprite = gradeAsset.plantGradeIcon;
            plantIcon.sprite = plantAsset.plantIcon;
            
            int grade = userPlant.plantGrade;
            for (int i = 0; i < plantGradeParent.childCount; i++)
                plantGradeParent.GetChild(i).GetChild(0).gameObject.SetActive(i < grade);
        }

        HorizontalLayoutGroup _plantCoinValueLayoutGroup;
        HorizontalLayoutGroup _plantDebrisValueLayoutGroup;
        VerticalLayoutGroup _plantElementValueLayoutGroup;
        void UpdateLayout()
        {
            if (!_plantCoinValueLayoutGroup)
                _plantCoinValueLayoutGroup = 
                    plantCoinValue.GetComponentInParent<HorizontalLayoutGroup>();
            if (_plantCoinValueLayoutGroup)
                _plantCoinValueLayoutGroup.enabled = !_plantCoinValueLayoutGroup.enabled;
            if (!_plantDebrisValueLayoutGroup)
                _plantDebrisValueLayoutGroup = 
                    plantDebrisValue.GetComponentInParent<HorizontalLayoutGroup>();
            if (_plantDebrisValueLayoutGroup) 
                _plantDebrisValueLayoutGroup.enabled = !_plantDebrisValueLayoutGroup.enabled;
            // if (!_plantElementValueLayoutGroup)
            //     _plantElementValueLayoutGroup = 
            //         elementParent.GetComponent<VerticalLayoutGroup>();
            // if (_plantElementValueLayoutGroup) 
            //     _plantElementValueLayoutGroup.enabled = !_plantElementValueLayoutGroup.enabled;
        }

        public void OnClickUpgrade()
        {
            if (_debrisCost <= 0) return;
            PlantUpgradeLogic.Instance.DoUpgrade(_selectedUserPlant.userPlantId, _debrisCost);
            _selectedItem.GetComponent<PlantUpgradeItem>().UpdateItem();
            OnSelect(_selectedUserPlant);
        }

        public void OnClickUplevel()
        {
            if (_coinCost <= 0) return;
            PlantUpgradeLogic.Instance.DoUplevel(_selectedUserPlant.userPlantId, _coinCost);
            _selectedItem.GetComponent<PlantUpgradeItem>().UpdateItem();
            OnSelect(_selectedUserPlant);
        }
    }
}