using Currency.Core.Run;
using DG.Tweening;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using Script.Core.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace Gameplay.Script.Gameplay
{
    public class ZombieSystem : MonoSingle<ZombieSystem>
    {
        [System.Serializable]
        public  class ZombieVulueSystem
        {
            [Header("初始数量")]
            public int originCount;
            [Header("每分钟增加僵尸数")]
            public int perMinuteAddCount;
            [Header("最大僵尸数")]
            public int maxZombieCount10Min;
            [Header("高级僵尸数")]
            public int maxSenorZombieCount10Min;
            [Header("准备时间")]
            public int prepareTime = 2;
            [Header("难度（数值越低难度越高）")]
            [Range(10, 300)]
            public int difficultyTimer;
            [Header("快速模式类型")]
            public QuickGameMode quickGameMode;
        }

        [SerializeField] ZombieVulueSystem[] quickModes;
        ZombieVulueSystem _current;

        [SerializeField] private Transform leftParent;
        [SerializeField] private Transform rightParent;
        [SerializeField] private GameObject moveIcon;
        [SerializeField] private float iconMoveDuration = 1.5f;
        private AutoLevel _normal;
        private AutoLevel _middle;
        private AutoLevel _senor;

        [Header("高级僵尸出现音效")]
        [SerializeField] private AudioClip seniorZombieAppearClip;  // 拖入你要播放的音效
        private bool _seniorAudioPlayed = false;                    // 标记是否已播放过
        private AudioSource _audioSource;                           // 播放音效的 AudioSource


        List<ZombieBehaviour> _zombieBehaviours = new ();
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
        private Camera _targetCamera;
        private void Start()
        {
            moveIcon.SetActive(false);
            _targetCamera = Camera.main;
            EventDispatcher.Instance.Register((int)EventID.INITGAMELEVEL, OnGetGameLevel);
            EventDispatcher.Instance.Register((int)EventID.MONSTERDEAD, OnMonsterDead);
            EventDispatcher.Instance.Register((int)EventID.GAMEOVER, OnGameOver);
            _audioSource = gameObject.GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
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
            // if (_deadCount == _spawnIndex.Length)
            //     EventDispatcher.Instance.Dispatch((int)EventID.CHANGEGAMELEVEL);
        }
        
        private void OnGameOver(GameEventArg arg)
        {
            StopAllCoroutines();
            _zombieBehaviours.RemoveAll(item => item == null);
            _zombieBehaviours.Clear();
            for (int i = 0; i < leftParent.childCount; i++)
                leftParent.GetChild(i).gameObject.SetActive(false);
            for (int i = 0; i < rightParent.childCount; i++)
                rightParent.GetChild(i).gameObject.SetActive(false);
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
            StopCoroutine(nameof(CheckVisibilityCoroutine));
            StartCoroutine(nameof(CheckVisibilityCoroutine));
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
                        // Debug.Log($"easyProb:{easyProb},mediumProb:{mediumProb},hardProb:{hardProb}");
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
            // 如果是第一个“高级”僵尸，且还没播过音效，就播放一次
            if (asset.zombieType == ZombieType.高级 && !_seniorAudioPlayed)
            {
                _audioSource.PlayOneShot(seniorZombieAppearClip);
                _seniorAudioPlayed = true;
            }
            var target = ((ProtectPlantsGameplay)GameplayMgr.Instance).Target;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = _currentGameLevel.zombieSpawnRadius;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

            Vector3 originPos = target.position + offset;
            Quaternion originRot = Quaternion.LookRotation(target.position - originPos);

            var obj = Instantiate(asset.zombiePrefab);
            obj.Initialize(GameplayMgr.Instance);
            obj.OnSpawn(asset, originPos, originRot, target, minute);
            _zombieBehaviours.Add(obj);
            //var mGameObject = asset;
            //float x = Random.Range(-_currentGameLevel.zombieSpawnRadius, _currentGameLevel.zombieSpawnRadius);
            //float z = Mathf.Sqrt(Mathf.Pow(_currentGameLevel.zombieSpawnRadius, 2) - Mathf.Pow(x, 2));
            //var target = ((ProtectPlantsGameplay)GameplayMgr.Instance).Target;
            //Vector3 originPos = target.position + new Vector3(x, 0, z * Mathf.Pow(-1, Random.Range(1, 10)));
            //Quaternion originRot = Quaternion.LookRotation(target.position - originPos);
            //var obj = Instantiate(mGameObject.zombiePrefab);
            //obj.OnSpawn(mGameObject, originPos, originRot, target, minute);
            //_zombieBehaviours.Add(obj);
        }

        IEnumerator CheckVisibilityCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(.05f);
                int left, right;
                left = right = 0;
                for (int i = 0; i < _zombieBehaviours.Count; i++) 
                {
                    if (_zombieBehaviours[i])
                    {
                        if (IsObjectVisible(_zombieBehaviours[i].transform))
                        {
                            _zombieBehaviours[i].SetVisibility((side, target) =>
                            {
                                var obj = Instantiate(moveIcon);
                                obj.SetActive(true);
                                obj.transform.position = side > 0 ? rightParent.position : leftParent.position;
                                obj.GetComponent<ZombieIcon>().Init(side, target, iconMoveDuration);
                            });
                            _zombieBehaviours[i].SetVisibilitySide(0);
                        }
                        else if (!_zombieBehaviours[i].IsVisibility)
                        {
                            Vector3 a = _targetCamera.transform.right;
                            Vector3 b = _zombieBehaviours[i].transform.position - _targetCamera.transform.position;
                            a.y = b.y = 0;
                            float dot = Vector3.Dot(a.normalized, b.normalized);
                            if (dot > 0) right++;
                            else left++;
                            _zombieBehaviours[i].SetVisibilitySide(dot > 0 ? 1 : -1);
                        }
                    }
                }
                for (int i = 0; i < leftParent.childCount; i++)
                    leftParent.GetChild(i).gameObject.SetActive(i < left);
                for (int i = leftParent.childCount; i < left; i++)
                    Instantiate(leftParent.GetChild(0).gameObject, leftParent).SetActive(true);
                for (int i = 0; i < rightParent.childCount; i++)
                    rightParent.GetChild(i).gameObject.SetActive(i < right);
                for (int i = rightParent.childCount; i < right; i++)
                    Instantiate(rightParent.GetChild(0).gameObject, rightParent).SetActive(true);
            }
        }
        
        bool IsObjectVisible(Transform target)
        {
            Renderer renderer = target.GetComponentInChildren<Renderer>();
            if (renderer == null)
                return false;

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_targetCamera);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }
        
        public string GetRelativeDirectionBehindCamera(Transform objectTransform)
        {

            Vector3 worldPosition = objectTransform.position;
            Vector3 viewportPosition = _targetCamera.WorldToViewportPoint(worldPosition);

            if (viewportPosition.x < 0.5f)
            {
                return "Left";
            }
            else
            {
                return "Right";
            }
        }
        
    }

    [System.Serializable]
    public class AutoLevel
    {
        public int aliveCount;
        public List<ZombieAsset> zombiePools;
        
    }
}