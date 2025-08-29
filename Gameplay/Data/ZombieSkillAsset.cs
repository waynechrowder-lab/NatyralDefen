using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public abstract class ZombieSkillAsset : ScriptableObject
    {
        public string skillName;
        [Min(0f)] public float cooldown = 1f;
        [Min(0f)] public float duration = 0f;
        public abstract IZombieSkill AddTo(GameObject host);
    }
}
