using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    [CreateAssetMenu(fileName = "New Buff", menuName = "GameSettings/CreateAsset/BuffAsset", order = 1)]
    public class BuffAsset : ScriptableObject
    {
        [TextArea] public string buffName;
        [TextArea] public string description;
        
        public BuffType plantType;
        public float value;
        public float duration;
        [HideInInspector] public GameObject target;
    }

    public enum BuffType
    {
        植物减益,
        植物增伤,
        僵尸减益,
        僵尸增伤,
        僵尸击退,
        自爆,
        僵尸聚集,
        僵尸中毒,
        僵尸禁锢
    }
}