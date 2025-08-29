using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    [CreateAssetMenu(fileName = "ProduceSkill", menuName = "GameSettings/PlantSkills/ProduceSkill", order = 3)]
    public class ProduceSkillAsset : PlantSkillAsset
    {
        [Min(0)] public int produceValue = 25;
        [Min(0f)] public float throwRange = 2.5f;
        public UserBullet coinPrefab;


        public override IPlantSkill AddTo(GameObject host)
        {
            var c = host.AddComponent<ProduceSkill>();
            c.Data = this;
            return c;
        }
    }
}