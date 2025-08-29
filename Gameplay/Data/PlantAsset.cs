
using Gameplay.Script.Data;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    [CreateAssetMenu(fileName = "New Plant", menuName = "GameSettings/CreateAsset/PlantAsset", order = 1)]
    public class PlantAsset : ScriptableObject
    {
        [TextArea] public string plantName;
        public PlantType plantType;
        public PlantGrade plantGrade;
        public Sprite icon;
        public PlantBehaviour plantPrefab;
        
        public int cost;
        public float coolingTime;
        
        public float attackRange;
        public int attackValue;
        public float attackInterval;
        public float attackDuration;
        [ColorUsage(true, true)] public Color attackColor;

        public int health;
        public BuffAsset buffAsset;
    }

    // public enum PlantType
    // {
    //     资源型,
    //     远程攻击型,
    //     防御型,
    //     消耗型,
    //     近战型,
    // }
}