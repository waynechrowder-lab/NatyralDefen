using System;
using System.Linq;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.Gameplay
{
    public class PlantIntroduction : MonoBehaviour
    {
        [SerializeField] private TMP_Text plantGradeName;
        [SerializeField] private TMP_Text plantName;
        [SerializeField] private TMP_Text plantAttribute;
        [SerializeField] private TMP_Text plantIntroduce;

        [SerializeField] private TMP_Text plantAttackTitle;
        [SerializeField] private TMP_Text plantIntervalTitle;
        
        [SerializeField] private TMP_Text plantAttackValue;
        [SerializeField] private TMP_Text plantIntervalValue;
        [SerializeField] private TMP_Text plantHealthValue;
        [SerializeField] private TMP_Text plantCoinValue;
        
        [SerializeField] private Image plantGradeIcon;
        [SerializeField] private Image plantIcon;

        public void LoadPlantData(string plantId)
        {
            var plantData = PlantUpgradeLogic.Instance.GetPlantData(plantId);
            Enum.TryParse<PlantType>(plantData.type, out var plantType);
            Enum.TryParse<PlantGrade>(plantData.quality, out var plantGrade);
            var typeAsset = PlantUpgradeLogic.Instance.GetPlantUITypeAsset(plantType);
            var gradeAsset = PlantUpgradeLogic.Instance.GetPlantUIGradeAsset(plantGrade);
            var plantAsset = PlantUpgradeLogic.Instance.GetPlantIconAsset(plantId);

            if (plantType == PlantType.资源)
            {
                plantAttackTitle.GetComponent<TMP_Text>().enabled = false;
                plantIntervalTitle.GetComponent<TMP_Text>().enabled = false;
                plantAttackTitle.transform.GetChild(0).GetComponent<TMP_Text>().enabled = true;
                plantIntervalTitle.transform.GetChild(0).GetComponent<TMP_Text>().enabled = true;
            }
            else
            {
                plantAttackTitle.GetComponent<TMP_Text>().enabled = true;
                plantIntervalTitle.GetComponent<TMP_Text>().enabled = true;
                plantAttackTitle.transform.GetChild(0).GetComponent<TMP_Text>().enabled = false;
                plantIntervalTitle.transform.GetChild(0).GetComponent<TMP_Text>().enabled = false;
            }
            
            plantGradeName.text = gradeAsset.plantGradeName;
            plantName.text = plantData.name;
            plantAttribute.text = plantData.attribute;
            plantIntroduce.text = plantData.description;

            var plantLocalAsset = GameResourcesMgr.Instance.PlantAssets.FirstOrDefault(
                value => value.plantName.Equals(plantData.id));
            plantAttackValue.text = plantLocalAsset.attackValue.ToString();
            plantIntervalValue.text =
                $"{1 / (plantLocalAsset.attackInterval <= 0 ? 1 : plantLocalAsset.attackInterval):f2}/s";
            plantHealthValue.text = plantLocalAsset.health.ToString();
            plantCoinValue.text = plantLocalAsset.cost.ToString();
            
            plantGradeIcon.sprite = gradeAsset.plantGradeIcon;
            plantIcon.sprite = plantAsset.plantIcon;
        }

        public void OnClickClose()
        {
            gameObject.SetActive(false);
        }
    }
    
    
}