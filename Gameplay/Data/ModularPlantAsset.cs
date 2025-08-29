using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    [CreateAssetMenu(fileName = "New Modular Plant", menuName = "GameSettings/CreateAsset/ModularPlant", order = 50)]
    public class ModularPlantAsset : ScriptableObject
    {
        public string plantName;
        public Sprite icon;
        public int baseHealth = 100;
        public int baseCost = 50;

        [Header("技能（支持多个）")]
        public List<PlantSkillAsset> skills = new List<PlantSkillAsset>();


        [Header("升级形态（可选）")]
        public List<UpgradeStage> upgradeStages = new List<UpgradeStage>();


        [Serializable]
        public class UpgradeStage
        {
            public int requiredLevel = 2;
            public string replacePath;
        }
    }
}