using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    [CreateAssetMenu(fileName = "AttackSkill", menuName = "GameSettings/PlantSkills/AttackSkill", order = 1)]
    public class AttackSkillAsset : PlantSkillAsset
    {
        public enum AttackMode { Melee, Ranged, Scope }

        public int weight;
        public string animatorName;
        public AttackMode mode = AttackMode.Ranged;
        public List<int> applyLevels;
        [Min(0f)] public float attackRange = 6f;
        public int attackValue = 10;
        [Tooltip("子弹或特效预制体（远程时使用）")] public GameObject projectilePrefab;
        [ColorUsage(true, true)] public Color attackColor = Color.white;
        public BuffAsset buffAsset;
        public AudioClip attackAudio;

        public override IPlantSkill AddTo(GameObject host)
        {
            var c = host.AddComponent<AttackSkill>();
            c.Data = this;
            return c;
        }
    }
}