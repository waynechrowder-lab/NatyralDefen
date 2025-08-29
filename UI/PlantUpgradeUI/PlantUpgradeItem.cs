using System;
using System.Linq;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Gameplay;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class PlantUpgradeItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text plantName;
        [SerializeField] private TMP_Text plantDescription;
        [SerializeField] private TMP_Text plantLevel;
        [SerializeField] private Image plantIcon;
        [SerializeField] private Image plantTypeIcon;
        [SerializeField] private Image plantBackground;
        [SerializeField] private Image plantGlow;
        [SerializeField] private Image plantGradient;
        [SerializeField] private Transform plantGradeParent;
        [SerializeField] private GameObject plantIntroduceTip;
        [SerializeField] private GameObject inUse;
        private UserPlantData _userPlant = null;
        private PlantInherentData _plantData = null;
        private Action<UserPlantData> _onSelect;
        private Action<string> _onClickIntroduce;

        public void InitItem(PlantInherentData plantData, Action<string> onClickIntroduce)
        {
            if (plantData == null)
            {
                gameObject.SetActive(false);
                return;
            }
            _plantData = plantData;
            _onClickIntroduce = onClickIntroduce;
            inUse.SetActive(false);
            var plants = GameLevelLogic.Instance.GetUserPlants();

            if (!plants.Any(value => value.plantId.Equals(plantData.id)))
            {
                inUse.SetActive(true);
            }

            plantName.text = plantData.name;
            plantDescription.text = plantData.description;
            plantLevel.text = "1";

            Enum.TryParse<PlantType>(plantData.type, out var plantType);
            Enum.TryParse<PlantGrade>(plantData.quality, out var plantGrade);
            var typeAsset = PlantUpgradeLogic.Instance.GetPlantUITypeAsset(plantType);
            var gradeAsset = PlantUpgradeLogic.Instance.GetPlantUIGradeAsset(plantGrade);
            var plantAsset = PlantUpgradeLogic.Instance.GetPlantIconAsset(plantData.id);

            plantIcon.sprite = plantAsset.plantIcon;
            plantTypeIcon.sprite = typeAsset.plantTypeIcon;
            plantBackground.sprite = gradeAsset.plantGradeBackground;
            plantGlow.color = gradeAsset.plantGradeColorGlow;
            plantGradient.color = gradeAsset.plantGradeColorGradient;

            int grade = 1;
            for (int i = 0; i < plantGradeParent.childCount; i++)
                plantGradeParent.GetChild(i).GetChild(0).gameObject.SetActive(i < grade);
        }
        
        public void InitItem(PlantInherentData plantData, MiniWorldShopData shopData, Action<string> onClickIntroduce, Action<UserPlantData> onSelect)
        {
            if (plantData == null)
            {
                gameObject.SetActive(false);
                return;
            }
            _plantData = plantData;
            _onClickIntroduce = onClickIntroduce;
            _onSelect = onSelect;
            _userPlant = new UserPlantData(plantData.id, 0);
            inUse.SetActive(true);
            var bagPlants = UserDataManager.Instance.UserDataJson.userPlants;
            bool active = !bagPlants.Any(value => value.plantId.Equals(shopData.otherId));
            if (!active)
            {
                inUse.GetComponentInChildren<TMP_Text>().text = "已拥有";
            }
            else
            {
                inUse.GetComponentInChildren<TMP_Text>().text = shopData.itemCoin.ToString();
            }
            plantName.text = plantData.name;
            plantDescription.text = plantData.description;
            plantLevel.text = "1";

            Enum.TryParse<PlantType>(plantData.type, out var plantType);
            Enum.TryParse<PlantGrade>(plantData.quality, out var plantGrade);
            var typeAsset = PlantUpgradeLogic.Instance.GetPlantUITypeAsset(plantType);
            var gradeAsset = PlantUpgradeLogic.Instance.GetPlantUIGradeAsset(plantGrade);
            var plantAsset = PlantUpgradeLogic.Instance.GetPlantIconAsset(plantData.id);

            plantIcon.sprite = plantAsset.plantIcon;
            plantTypeIcon.sprite = typeAsset.plantTypeIcon;
            plantBackground.sprite = gradeAsset.plantGradeBackground;
            plantGlow.color = gradeAsset.plantGradeColorGlow;
            plantGradient.color = gradeAsset.plantGradeColorGradient;

            int grade = 1;
            for (int i = 0; i < plantGradeParent.childCount; i++)
                plantGradeParent.GetChild(i).GetChild(0).gameObject.SetActive(i < grade);
            GetComponentInChildren<Button>().interactable = active;
        }

        public void InitItem(UserPlantData userPlant, Action<UserPlantData> onSelect, Action<string> onClickIntroduce)
        {
            _userPlant = userPlant;
            _onSelect = onSelect;
            _onClickIntroduce = onClickIntroduce;
            UpdateItem();
        }

        public void UpdateItem()
        {
            if (_userPlant.userPlantId < 0)
            {
                gameObject.SetActive(false);
                return;
            }
            if (_userPlant == null)
            {
                Debug.LogError($"userPlantId : {_userPlant.userPlantId},{_userPlant.plantId} is null");
                gameObject.SetActive(false);
            }
            var plantData = PlantUpgradeLogic.Instance.GetPlantData(_userPlant.plantId);
            if (plantData == null)
            {
                gameObject.SetActive(false);
                return;
            }
            _plantData = plantData;
            inUse.SetActive(false);
            var bag = GameLevelLogic.Instance.GetUserGameBags();
            if (_userPlant.userPlantId >= 0 && bag.Contains(_userPlant.userPlantId))
            {
                inUse.SetActive(true);
            }
            
            plantName.text = plantData.name;
            plantDescription.text = plantData.description;
            plantLevel.text = _userPlant.plantLevel.ToString();

            Enum.TryParse<PlantType>(plantData.type, out var plantType);
            Enum.TryParse<PlantGrade>(plantData.quality, out var plantGrade);
            var typeAsset = PlantUpgradeLogic.Instance.GetPlantUITypeAsset(plantType);
            var gradeAsset = PlantUpgradeLogic.Instance.GetPlantUIGradeAsset(plantGrade);
            var plantAsset = PlantUpgradeLogic.Instance.GetPlantIconAsset(_userPlant.plantId);

            plantIcon.sprite = plantAsset.plantIcon;
            plantTypeIcon.sprite = typeAsset.plantTypeIcon;
            plantBackground.sprite = gradeAsset.plantGradeBackground;
            plantGlow.color = gradeAsset.plantGradeColorGlow;
            plantGradient.color = gradeAsset.plantGradeColorGradient;

            int grade = _userPlant.plantGrade;
            for (int i = 0; i < plantGradeParent.childCount; i++)
                plantGradeParent.GetChild(i).GetChild(0).gameObject.SetActive(i < grade);
        }
        
        public void OnSelect()
        {
            _onSelect?.Invoke(_userPlant);
        }
        
        public void OnRelease()
        {
            
        }

        bool _pointerEnter = false;
        float _pointerTime = 0f;

        private void Start()
        {
            if (plantIntroduceTip != null)
                plantIntroduceTip.SetActive(false);
        }

        private void Update()
        {
            if (!plantIntroduceTip) return;
            plantIntroduceTip.SetActive(false);
            if (_pointerEnter)
            {
                _pointerTime += Time.deltaTime;
                if (_pointerTime > 0.5f)
                {
                    plantIntroduceTip.SetActive(true);
                }
            }
        }

        public void OnPointerEnter()
        {
            _pointerTime = 0;
            _pointerEnter = true;
        }

        public void OnPointerExit()
        {
            _pointerEnter = false;
        }

        public void OnPointerClick()
        {
            _pointerEnter = false;
            _onClickIntroduce?.Invoke(_plantData.id);
        }
    }
}