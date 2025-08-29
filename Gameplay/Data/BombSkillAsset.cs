using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    [CreateAssetMenu(fileName = "BombSkill", menuName = "GameSettings/PlantSkills/BombSkill", order = 1)]
    public class BombSkillAsset : PlantSkillAsset
    {
        [Min(0f)] public float triggerDelay = 1;
        [Min(0f)] public float radius = 6f;
        public int damage = 10;
        public GameObject boomPrefab;
        [ColorUsage(true, true)] public Color attackColor = Color.white;
        public BuffAsset buffAsset;
        
        public override IPlantSkill AddTo(GameObject host)
        {
            var c = host.AddComponent<BombSkill>();
            c.Data = this;
            return c;
        }
    }
}