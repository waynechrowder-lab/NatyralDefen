using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    [CreateAssetMenu(fileName = "New Level", menuName = "GameSettings/CreateAsset/LevelAsset", order = 1)]
    public class LevelAsset : ScriptableObject
    {
        [TextArea] public string levelName;
        public int levelIndex;
        public int prepareTime = 10;
        public int aliveZombieCount;
        public float zombieSpawnRadius;
        public LevelZombieData[] zombieData;
    }

    [System.Serializable]
    public class LevelZombieData
    {
        public ZombieAsset zombieAsset;
        public int count;
    }
}