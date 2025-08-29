
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    [CreateAssetMenu(fileName = "New Zombie", menuName = "GameSettings/CreateAsset/ZombieAsset", order = 1)]
    public class ZombieAsset : ScriptableObject
    {
        [TextArea] public string zombieName;
        public ZombieType zombieType;
        public ZombieBehaviour zombiePrefab;
        
        public MoveType moveType;
        public float moveSpeed;
        
        public AttackType attackType;
        public float attackRange;
        public int attackValue;
        public float attackInterval;
        
        public int health;
        
        public int score;

        public int coin;
        public int exp;

        [Header("技能（可选）")]
        public List<ZombieSkillAsset> skills = new List<ZombieSkillAsset>();
    }

    public enum ZombieType
    {
        普通,
        中级,
        高级
    }

    public enum MoveType
    {
        爬,
        行走,
        跳舞
    }

    public enum AttackType
    {
        咬,
        踢
    }
}