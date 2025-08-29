using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public abstract class PlantSkillAsset : ScriptableObject
    {
        public string skillName;
        public string originName;
        [Min(0f)] public float cooldown = 1f;
        [Min(0f)] public float duration = 0f;
        [Min(0f)] public float damageDelayVisual;
        public abstract IPlantSkill AddTo(GameObject host);
    }
}