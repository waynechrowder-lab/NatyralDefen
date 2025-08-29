using System;
using Gameplay.Script.Data;
using Gameplay.Script.Gameplay;
using Gameplay.Script.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class GameBagItem : MonoBehaviour
    {
        [SerializeField] private GameObject opened;
        [SerializeField] private GameObject closed;
        [SerializeField] private GameObject empty;
        
        [SerializeField] private TMP_Text plantName;
        [SerializeField] private TMP_Text plantDescription;
        [SerializeField] private TMP_Text plantLevel;
        [SerializeField] private Image plantIcon;
        [SerializeField] private Image plantTypeIcon;
        [SerializeField] private Image plantBackground;
        [SerializeField] private Image plantGlow;
        [SerializeField] private Image plantGradient;
        [SerializeField] private Image plantRate;
        [SerializeField] private TMP_Text plantRateText;
        [SerializeField] private Transform plantGradeParent;
        private bool _isLock;
        private int _userPlantId = -1;
        private Action<int, int> _onClick;
        public void InitItem(bool isLock, int userPlantId, Action<int, int> onClick)
        {
            _isLock = isLock;
            _userPlantId = userPlantId;
            _onClick = onClick;
            UpdateItem();
        }

        void UpdateItem()
        {
            opened.SetActive(false);
            closed.SetActive(false);
            empty.SetActive(false);
            if (_isLock)
            {
                closed.SetActive(true);
                return;
            }

            if (_userPlantId == -1)
            {
                empty.SetActive(true);
                return;
            }
            opened.SetActive(true);
            var userPlant = PlantUpgradeLogic.Instance.GetUserPlantData(_userPlantId);
            if (userPlant == null)
            {
                GameLevelLogic.Instance.RemoveBagPlant(_userPlantId);
                _userPlantId = -1;
                empty.SetActive(true);
                return;
            }
            var plantData = PlantUpgradeLogic.Instance.GetPlantData(userPlant.plantId);
            if (plantData == null)
            {
                empty.SetActive(true);
                return;
            }
            plantName.text = plantData.name;
            plantDescription.text = plantData.description;
            plantLevel.text = userPlant.plantLevel.ToString();

            Enum.TryParse<PlantType>(plantData.type, out var plantType);
            Enum.TryParse<PlantGrade>(plantData.quality, out var plantGrade);
            var typeAsset = PlantUpgradeLogic.Instance.GetPlantUITypeAsset(plantType);
            var gradeAsset = PlantUpgradeLogic.Instance.GetPlantUIGradeAsset(plantGrade);
            var plantAsset = PlantUpgradeLogic.Instance.GetPlantIconAsset(userPlant.plantId);

            plantIcon.sprite = plantAsset.plantIcon;
            plantTypeIcon.sprite = typeAsset.plantTypeIcon;
            plantBackground.sprite = gradeAsset.plantGradeBackground;
            plantGlow.color = gradeAsset.plantGradeColorGlow;
            plantGradient.color = gradeAsset.plantGradeColorGradient;

            plantRate.sprite = gradeAsset.plantGradeIcon;
            plantRateText.text = plantGrade.ToString();
            
            int grade = userPlant.plantGrade;
            for (int i = 0; i < plantGradeParent.childCount; i++)
                plantGradeParent.GetChild(i).GetChild(0).gameObject.SetActive(i < grade);
        }
        
        public void OnClick()
        {
            if (_isLock) return;
            int index = transform.GetSiblingIndex();
            _onClick?.Invoke(_userPlantId, index);
            if (_userPlantId != -1)
            {
                _userPlantId = -1;
                UpdateItem();
            }
        }
    }
}