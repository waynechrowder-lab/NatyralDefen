using Currency.Core.Run;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using Script.Core.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class ZombieSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class ZombieVulueSystem
        {
            [Header("初始数量")] public int originCount;
            [Header("每分钟增加僵尸数")] public int perMinuteAddCount;
            [Header("最大僵尸数")] public int maxZombieCount10Min;
            [Header("高级僵尸数")] public int maxSenorZombieCount10Min;
            [Header("准备时间")] public int prepareTime = 2;
            [Header("难度（数值越低难度越高）")]
            [Range(10, 300)] public int difficultyTimer;
            [Header("快速模式类型")] public QuickGameMode quickGameMode;
        }

        [SerializeField] ZombieVulueSystem[] quickModes;
        ZombieVulueSystem _current;
        private AutoLevel _normal;
        private AutoLevel _middle;
        private AutoLevel _senor;

        List<ZombieBehaviour> _zombieBehaviours = new();
        public List<ZombieBehaviour> ZombieBehaviours => _zombieBehaviours;
        private List<ZombieAsset> _zombieAssets = new();
        private LevelAsset _currentGameLevel;
        LevelZombieData[] _levelZombie;
        private int[] _spawnIndex;
        int _currentSpawnIndex;
        int _deadCount;
        int _zombieRemaining;
        public int ZombieRemaining => _zombieRemaining + _zombieBehaviours.Count;
        int _zombieWaitTime;
        public int ZombieWaitTime => _zombieWaitTime;

        public System.Action<ZombieAsset, ZombieBehaviour> OnZombieSpawned;

        private void Start()
        {
            EventDispatcher.Instance.Register((int)EventID.INITGAMELEVEL, OnGetGameLevel);
            EventDispatcher.Instance.Register((int)EventID.MONSTERDEAD, OnMonsterDead);
            EventDispatcher.Instance.Register((int)EventID.GAMEOVER, OnGameOver);
        }

        private void OnDestroy()
        {
            EventDispatcher.Instance.UnRegister((int)EventID.INITGAMELEVEL, OnGetGameLevel);
            EventDispatcher.Instance.UnRegister((int)EventID.MONSTERDEAD, OnMonsterDead);
            EventDispatcher.Instance.UnRegister((int)EventID.GAMEOVER, OnGameOver);
        }

        private void OnGetGameLevel(GameEventArg arg)
        {
            _currentGameLevel = arg.GetArg<LevelAsset>(0);
            _levelZombie = _currentGameLevel.zombieData;
            InitSpawnPool();
        }

        private void OnMonsterDead(GameEventArg arg)
        {
            _deadCount++;
        }

        private void OnGameOver(GameEventArg arg)
        {
            StopAllCoroutines();
            _zombieBehaviours.RemoveAll(item => item == null);
            _zombieBehaviours.Clear();
        }

        void InitSpawnPool()
        {
            var list = GameResourcesMgr.Instance.ZombieAssets;
            var zombiePools = new List<ZombieAsset>();
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.普通));
            _normal = new AutoLevel()
            {
                aliveCount = 2,
                zombiePools = zombiePools
            };
            zombiePools = new List<ZombieAsset>();
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.普通));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.普通));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.普通));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.中级));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.普通));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.中级));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.中级));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.中级));

            _middle = new AutoLevel()
            {
                aliveCount = 4,
                zombiePools = zombiePools
            };
            zombiePools = new List<ZombieAsset>();
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.普通));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.普通));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.普通));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.普通));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.中级));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.中级));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.高级));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.高级));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.中级));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.中级));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.中级));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.中级));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.高级));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.中级));
            zombiePools.AddRange(list.Where(value => value.zombieType == ZombieType.高级));

            _senor = new AutoLevel()
            {
                aliveCount = 6,
                zombiePools = zombiePools
            };

            _currentSpawnIndex = 0;
            _deadCount = 0;
            _zombieAssets.Clear();
            foreach (var t in _levelZombie)
                for (int j = 0; j < t.count; j++)
                    _zombieAssets.Add(t.zombieAsset);
            _spawnIndex = PublicTools.GetRandom(_zombieAssets.Count);
            StopCoroutine(nameof(CreateMonsterCoroutine));
            StartCoroutine(nameof(CreateMonsterCoroutine));
        }

        IEnumerator CreateMonsterCoroutine()
        {
            var gameMode = GameLevelLogic.Instance.QuickGameMode;
            var quickMode = quickModes.FirstOrDefault(value => value.quickGameMode == gameMode);
            _current = quickMode != null ? quickMode :
                new ZombieVulueSystem {
                    originCount = 2,
                    perMinuteAddCount = 2,
                    maxZombieCount10Min = 18,
                    maxSenorZombieCount10Min = 8,
                    quickGameMode = QuickGameMode.Normal,
            };
            gameMode = _current.quickGameMode;
            float t = 0;
            float timer = 0;
            _zombieWaitTime = _current.prepareTime;
            _zombieRemaining = (int)gameMode;
            while (_zombieWaitTime > 0)
            {
                yield return new WaitForSeconds(1f);
                _zombieWaitTime -= 1;
            }

            float checkTimer = 60;
            var list = GameResourcesMgr.Instance.ZombieAssets;
            var normal = list.Where(value => value.zombieType == ZombieType.普通).ToList();
            var middle = list.Where(value => value.zombieType == ZombieType.中级).ToList();
            var senor = list.Where(value => value.zombieType == ZombieType.高级).ToList();

            int maxSenorCount = 0;
            int aliveCount = _current.originCount;
            int monute = 0;
            while (true)
            {
                t += 0.1f;
                timer += 0.1f;
                checkTimer -= .1f;
                _zombieBehaviours.RemoveAll(item => item == null);

                float timeRatio = Mathf.Clamp01(timer / _current.difficultyTimer);
                float easyProb = Mathf.Lerp(1f, 0, timeRatio + .5f);
                float mediumProb = Mathf.SmoothStep(0f, 0.6f, timeRatio);
                float hardProb = Mathf.Clamp01((timeRatio - 0.6f) * 2f);

                float totalProb = easyProb + mediumProb + hardProb;
                easyProb /= totalProb;
                mediumProb /= totalProb;
                hardProb /= totalProb;

                if (checkTimer < 0)
                {
                    monute += 1;
                    checkTimer = 60;
                    aliveCount += _current.perMinuteAddCount;
                    aliveCount = Mathf.Min(aliveCount, _current.maxZombieCount10Min);
                    maxSenorCount = _current.maxSenorZombieCount10Min;
                }

                bool create = _zombieBehaviours.Count < aliveCount;
                if (create && gameMode != QuickGameMode.Normal)
                {
                    create = create && _zombieRemaining > 0;
                }
                if (create)
                {
                    if (t > 0.1f)
                    {
                        t = 0;
                        ZombieAsset zombieType;
                        float rand = Random.Range(0f, 1f);
                        int currentSenorCount = _zombieBehaviours.Count(value => value.ZombieType == ZombieType.高级);
                        if (rand < easyProb)
                            zombieType = normal[Random.Range(0, 100) % normal.Count];
                        else if (rand < easyProb + mediumProb)
                            zombieType = middle[Random.Range(0, 100) % middle.Count];
                        else if (currentSenorCount < maxSenorCount)
                            zombieType = senor[Random.Range(0, 100) % senor.Count];
                        else
                            zombieType = middle[Random.Range(0, 100) % middle.Count];
                        CreateMonster(zombieType, monute);
                        _currentSpawnIndex++;
                        _zombieRemaining--;
                    }
                }
                else if (gameMode != QuickGameMode.Normal && _zombieRemaining <= 0 && _zombieBehaviours.Count <= 0)
                {
                    ((ProtectPlantsGameplay)GameplayMgr.Instance).FinishGame(true);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        void CreateMonster(ZombieAsset asset, int minute)
        {
            var target = ((ProtectPlantsGameplay)GameplayMgr.Instance).Target;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = _currentGameLevel.zombieSpawnRadius;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

            Vector3 originPos = target.position + offset;
            Quaternion originRot = Quaternion.LookRotation(target.position - originPos);

            var obj = Instantiate(asset.zombiePrefab);
            obj.OnSpawn(asset, originPos, originRot, target, minute);
            _zombieBehaviours.Add(obj);
            OnZombieSpawned?.Invoke(asset, obj);
        }
    }

    [System.Serializable]
    public class AutoLevel
    {
        public int aliveCount;
        public List<ZombieAsset> zombiePools;
    }
}
