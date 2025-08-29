using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Gameplay.Script.Agent;
using Gameplay.Script.Data;
using Gameplay.Script.Element;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using Gameplay.Script.MultiplayerModule;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Gameplay.Script.Gameplay
{
    public class ZombieBehaviour : MonoBehaviour, IInteroperableObject, ICharacter
    {
        [SerializeField] private AudioClip attackAudio;
        [SerializeField] private AudioClip underAttackAudio;
        [SerializeField] private AudioClip[] deathAudios;
        [SerializeField] private AudioClip walkAudio;
        private AudioSource _audioSource;
        private AudioSource _audioOneShotSource;
        [SerializeField] private AudioClip[] groanAudio;
        private AudioSource _groanAudioSource;
        private ZombieAnimator _zombieAnimator;
        private ZombieAnimator zombieAnimator
        {
            get
            {
                if (!_zombieAnimator)
                {
                    _zombieAnimator = GetComponent<ZombieAnimator>();
                }

                if (!_zombieAnimator)
                {
                    _zombieAnimator = gameObject.AddComponent<ZombieAnimator>();
                }
                return _zombieAnimator;
            }
        }
        private AdvancedNavMeshAgent _agent;
        private AdvancedNavMeshAgent agent
        {
            get
            {
                if (!_agent)
                {
                    _agent = GetComponent<AdvancedNavMeshAgent>();
                }

                if (!_agent)
                {
                    _agent = gameObject.AddComponent<AdvancedNavMeshAgent>();
                }
                return _agent;
            }
        }
        
        [SerializeField] private float colorDuration = 0.1f;
        [SerializeField] private float colorCanChangeInterval = 1.5f;
        [SerializeField] private string colorShaderId = "_EmissionColor";
        [SerializeField] private float destroyTime = 4;
        [SerializeField] private string dissolveShaderId = "_EdgeClip";
        [SerializeField] private float jungleHeartDis = 0.6f;
        [SerializeField] private GameObject portal;
        [SerializeField] private float portalDuration = 1.5f;
        [SerializeField] private float portalHideTime = 3;
        // [SerializeField] private ElementClass[] elementObj;
        public float k = 5 / 3f;
        public float maxSize = 8;
        public Transform iconMove2;
        public bool IsVisibility { get; private set; } = false;
        public int VisibilitySide { get; private set; } = 0;
        
        private GameObject _portal;
        private ZombieState _zombieState;
        public ZombieState ZombieState =>  _zombieState;
        public bool IsAlive => _zombieState < ZombieState.Dead;
        public int Health => _ctx?.Health ?? 0;
        private Transform _target;
        private PlantBehaviour _targetPlant;
        private List<Renderer> _renderers;
        private Color _currentColor;
        private float _currentTime;
        private float _timer;
        private Tweener _iconMove;
        private int _minute;
        private string _enemyId;
        [SerializeField] private EnemyInherentData _enemyInherentData;
        [SerializeField] private EnemyLevelData _enemyLevelData;

        private ZombieAsset _zombieAsset;
        private ZombieContext _ctx;
        private readonly List<IZombieSkill> _skills = new();
        public ZombieType ZombieType
        {
            get;
            private set;
        }

        private ElementItemObject[] _elementItemObjects;

        private void Awake()
        {
            _elementItemObjects = GetComponentsInChildren<ElementItemObject>(true);
            
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 1;
            _audioSource.minDistance = 1;
            _audioSource.maxDistance = 10;
            
            _audioOneShotSource = gameObject.AddComponent<AudioSource>();
            _audioOneShotSource.playOnAwake = false;
            _audioOneShotSource.spatialBlend = 1;
            _audioOneShotSource.minDistance = 1;
            _audioOneShotSource.maxDistance = 10;
            
            _groanAudioSource = gameObject.AddComponent<AudioSource>();
            _groanAudioSource.playOnAwake = false;
            _groanAudioSource.spatialBlend = 1;
            _groanAudioSource.minDistance = 1;
            _groanAudioSource.maxDistance = 10;

            _ = agent;
            _ = zombieAnimator;
        }

        private void Start()
        {
            portal.SetActive(false);
            _renderers = GetComponentsInChildren<Renderer>().ToList();
            _currentColor = _renderers[0].material.GetColor(colorShaderId);
            EventDispatcher.Instance.Register((int)EventID.GAMEOVER, OnGameOver);
            EventDispatcher.Instance.Register((int)EventID.GAMERESTART, OnRestart);
        }

        private void OnDestroy()
        {
            if (_portal)
                Destroy(_portal);
            EventDispatcher.Instance.UnRegister((int)EventID.GAMEOVER, OnGameOver);
            EventDispatcher.Instance.UnRegister((int)EventID.GAMERESTART, OnRestart);
            DOTween.Kill(transform);
        }
        
        private void Update()
        {
            if (GameplayMgr.Instance.GameplayState != GameplayState.Gaming) return;
            _currentTime -= Time.deltaTime;
            if (SceneLoadManager.Instance.GetActiveSceneIndex()
                == (int)SceneLoadManager.SceneIndex.MRScene)
                return;
            UpdatePoisonElement();
            UpdateIceElement();
            float dt = Time.deltaTime;
            for (int i = 0; i < _skills.Count; i++)
                _skills[i].Tick(dt);
        }
        
        public void OnGamePause()
        {
            if (zombieAnimator) zombieAnimator.SetSpeed(0);
            if (agent) agent.Pause();
            if (_groanAudioSource) _groanAudioSource.Pause();
            if (_audioSource) _audioSource.Pause();
            for (int i = 0; i < _skills.Count; i++)
                _skills[i].OnPaused(true);
        }

        public void OnGameUnPause()
        {
            if (zombieAnimator) zombieAnimator.SetSpeed(1);
            if (agent) agent.UnPause();
            if (_groanAudioSource) _groanAudioSource.UnPause();
            if (_audioSource) _audioSource.UnPause();
            for (int i = 0; i < _skills.Count; i++)
                _skills[i].OnPaused(false);
        }

        private void OnGameOver(GameEventArg arg)
        {
            bool success = arg.GetArg<bool>(0);
            if (_portal)
                Destroy(_portal);
            if (success)
                Kill();
            else
                Celebrate();
        }
        
        private void OnRestart(GameEventArg arg)
        {
            Destroy(gameObject);
        }

        public void OnSpawn(string enemyId, int level, Vector3 originPos, Quaternion originRot, Transform target)
        {
            _enemyId = enemyId;
            IsVisibility = false;
            VisibilitySide = 0;
            _target = target;
            _zombieState = ZombieState.Idle;

            _enemyInherentData = EnemyData.Instance.GetEnemy(enemyId);
            _enemyLevelData = EnemyData.Instance.GetEnemyLevel(enemyId, level);

            _ctx = new ZombieContext
            {
                Host = gameObject,
                Transform = transform,
                Animator = zombieAnimator,
                Health = _enemyLevelData.health
            };
            
            // NavMeshHit hit;
            // if (NavMesh.SamplePosition(originPos, out hit, _enemyLevelData.attackRange, NavMesh.AllAreas))
            //     originPos = hit.position;
            transform.SetPositionAndRotation(originPos, originRot);
            zombieAnimator.Init();
            Invoke(nameof(SetPortal), .5f);
            StartCoroutine(nameof(ZombieBehaviourCoroutine));
        }
        
        public void OnSpawn(ZombieAsset asset, Vector3 originPos, Quaternion originRot, Transform target, int minute)
        {
            _zombieAsset = asset;
            IsVisibility = false;
            VisibilitySide = 0;
            _target = target;
            ZombieType = asset.zombieType;
            _zombieState = ZombieState.Idle;
            _minute = minute;
            float health = asset.health;
            float attackV = asset.attackValue;
            int k = 0;
            while (minute > 0)
            {
                minute --;
                health *= 1 + 0.1f * k;
                attackV *= 1 + 0.05f * k;
                k++;
            }

            _ctx = new ZombieContext
            {
                Host = gameObject,
                Transform = transform,
                Animator = zombieAnimator,
                Asset = asset,
                Health = (int)health
            };

            _enemyLevelData = new EnemyLevelData()
            {
                health = _ctx.Health,
                attackInterval = asset.attackInterval,
                attackRange = asset.attackRange,
                attackValue = (int)attackV,
                coin = asset.attackValue,
                speed = asset.moveSpeed,
            };
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(originPos, out hit, asset.attackRange, NavMesh.AllAreas))
                originPos = hit.position;
            transform.SetPositionAndRotation(originPos, originRot);
            zombieAnimator.Init();
            if (asset.skills != null)
            {
                foreach (var sk in asset.skills)
                {
                    if (!sk) continue;
                    var skill = sk.AddTo(gameObject);
                    skill.Init(_ctx, sk);
                    _skills.Add(skill);
                }
            }

            _portal = Instantiate(portal);
            _portal.SetActive(true);
            _portal.transform.SetPositionAndRotation(portal.transform.position, portal.transform.rotation);
            var localScale = _portal.transform.GetChild(0).localScale;
            float originScaleX = localScale.x;
            localScale.x = 0;
            _portal.transform.GetChild(0).localScale = localScale;
            _portal.transform.GetChild(0).DOScaleX(originScaleX, portalDuration);
            StartCoroutine(nameof(ZombieBehaviourCoroutine));
        }

        void SetPortal()
        {
            _portal = Instantiate(portal);
            _portal.SetActive(true);
            _portal.transform.SetPositionAndRotation(portal.transform.position, portal.transform.rotation);
            var localScale = _portal.transform.GetChild(0).localScale;
            float originScaleX = localScale.x;
            localScale.x = 0;
            _portal.transform.GetChild(0).localScale = localScale;
            _portal.transform.GetChild(0).DOScaleX(originScaleX, portalDuration);
        }
        
        public void SetVisibility(Action<int, ZombieBehaviour> callback)
        {
            IsVisibility = true;
            if (VisibilitySide != 0)
                callback?.Invoke(VisibilitySide, this);
        }
        
        public void SetVisibilitySide(int side)
        {
            VisibilitySide = side;
        }
        /// <summary>
        /// 敌人行为发生改变
        /// </summary>
        /// <param name="last"></param>
        /// <param name="current"></param>
        /// <param name="callback"></param>
        void ChangeBehaviour(ZombieState last, ZombieState current, Action callback)
        {
            _zombieState = current;
            zombieAnimator.StartAnimation(_zombieState);
            callback?.Invoke();
            //todo:联机下僵尸状态改变
            _networkStateChanged?.Invoke((int)current);
        }
        /// <summary>
        /// 敌人行为控制
        /// </summary>
        /// <returns></returns>
        IEnumerator ZombieBehaviourCoroutine()
        {
            yield return new WaitUntil(() => GameplayMgr.Instance.GameplayState == GameplayState.Gaming);
            agent.Init(_target, _enemyLevelData.speed, jungleHeartDis);
            ChangeBehaviour(_zombieState, ZombieState.Moving, () =>
            {
                _audioSource.clip = walkAudio;
                _audioSource.Play();
                _groanAudioSource.clip = groanAudio[Random.Range(0, 10) % groanAudio.Length];
                _groanAudioSource.Play();
            });

            yield return new WaitForSeconds(portalHideTime);

            _portal.transform.GetChild(0).DOScaleX(0, portalDuration).OnComplete(() =>
            {
                Destroy(_portal);
            });

            while (true)
            {
                if (_targetPlant)
                {
                    _timer += 0.1f;
                    AttackPlant();
                }
                else
                {
                    if (FindAlivePlant(out _targetPlant))
                    {
                        agent.ChangeTarget(_targetPlant.transform, _enemyLevelData.attackRange);
                        Rotate();
                        _timer = 0;
                    }
                    else
                    {
                        AttackCore();
                    }
                }
                yield return new WaitForSeconds(0.1f);
                yield return new WaitUntil(() => GameplayMgr.Instance.GameplayState == GameplayState.Gaming);
            }
        }

        /// <summary>
        /// 攻击水晶核心
        /// </summary>
        void AttackCore()
        {
            agent.ChangeTarget(_target);
            Vector3 v1 = transform.position;
            Vector3 v2 = _target.position;
            v1.y = v2.y = 0;
            if (Vector3.Distance(v1, v2) <= jungleHeartDis)
            {
                _timer += 0.1f;
                if (_timer > _enemyLevelData.attackInterval)
                {
                    _timer = 0;
                    ChangeBehaviour(_zombieState, ZombieState.Attacking, () =>
                    {
                        if (_audioSource.clip != attackAudio) _audioSource.clip = attackAudio;
                        if (!_audioSource.isPlaying) _audioSource.Play();
                        _groanAudioSource.Stop();
                        if (SceneLoadManager.Instance.GetActiveSceneIndex()
                            == (int)SceneLoadManager.SceneIndex.MRScene)
                        {
                            ((ProtectPlantsGameplay)GameplayMgr.Instance).JungleHeart.UnderAttack(
                                _zombieAsset.attackValue, null, null);
                            return;
                        }
                        GameEventArg arg =
                            EventDispatcher.Instance.GetEventArg((int)EventID.KernelTakeDamage);
                        arg.SetArg(0, _enemyLevelData.attackValue);
                        EventDispatcher.Instance.Dispatch((int)EventID.KernelTakeDamage);
                    });
                }
            }
            else
            {
                if (_zombieState != ZombieState.Moving)
                {
                    ChangeBehaviour(_zombieState, ZombieState.Moving, () =>
                    {
                        if (_audioSource.clip != walkAudio) _audioSource.clip = walkAudio;
                        if (!_audioSource.isPlaying) _audioSource.Play();
                        _groanAudioSource.clip = groanAudio[Random.Range(0, 10) % groanAudio.Length];
                        _groanAudioSource.Play();
                    });
                }
            }
        }
        
        /// <summary>
        /// 攻击植物
        /// </summary>
        void AttackPlant()
        {
            Rotate(.1f);
            if (_timer > _enemyLevelData.attackInterval)
            {
                _timer = 0;
                ChangeBehaviour(_zombieState, ZombieState.Attacking, () =>
                {
                    if (_audioSource.clip != attackAudio)
                        _audioSource.clip = attackAudio;
                    if (!_audioSource.isPlaying)
                        _audioSource.Play();
                    _groanAudioSource.Stop();
                    _targetPlant.UnderAttack(_enemyLevelData.attackValue);
                });
            }
        }
        
        /// <summary>
        /// 转向目标物
        /// </summary>
        /// <param name="duration"></param>
        void Rotate(float duration = .5f)
        {
            Vector3 pos1 = transform.position;
            Vector3 pos2 = _targetPlant.transform.position;
            pos1.y = pos2.y = 0;
            Quaternion rot = Quaternion.LookRotation(pos2 - pos1);
            transform.DORotateQuaternion(rot, duration);
        }
        /// <summary>
        /// 寻找周围攻击范围内植物
        /// </summary>
        /// <param name="zombie"></param>
        /// <returns></returns>
        bool FindAlivePlant(out PlantBehaviour zombie)
        {
            zombie = null;
            float distance = _enemyLevelData.attackRange;
            var list = PlantCreatorSystem.PlantBehaviours;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    Vector3 pos1 = list[i].transform.position;
                    Vector3 pos2 = transform.position;
                    pos1.y = pos2.y = 0;
                    if (Vector3.Distance(pos1, pos2) <= distance)
                    {
                        distance = Vector3.Distance(pos1, pos2);
                        zombie = list[i];
                    }
                }
            }
            return zombie != null;
        }

        public void UnderAttack(string id, int userObjId, int itemCount)
        {

        }

        public void UnderAttack(int damage)
        {
            UnderAttack(damage, null, null);
        }

        public void UnderAttack(int damage, BuffAsset buff, Color? color)
        {
            if (buff)
            {
                if (buff.plantType == BuffType.僵尸击退)
                    OnRepel(buff);
                else if (buff.plantType == BuffType.僵尸聚集)
                    OnGather(buff);
                else if (buff.plantType == BuffType.僵尸中毒)
                    OnPoison(buff, damage);
                else if (buff.plantType == BuffType.僵尸禁锢)
                    OnImprison(buff);
                else
                {
                    CancelInvoke(nameof(Recover));
                    agent.ChangeSpeed(_zombieAsset.moveSpeed * buff.value);
                    zombieAnimator.SetSpeed(.5f);
                    Invoke(nameof(Recover), buff.duration);
                }
            }
            if (_zombieState == ZombieState.Dead) return;
            if (GameplayMgr.Instance.GameplayState != GameplayState.Gaming) return;
            _ctx.DoDamage(damage);

            if (_ctx.Health <= 0)
            {
                Death(buff && buff.plantType == BuffType.自爆 ? "爆炸" : null);
            }
            else
            {
                zombieAnimator.StartAnimation(ZombieState.UnderAttacking);
            }
            if (color.HasValue && (_currentTime < 0 || damage >= 300))
            {
                _currentTime = colorCanChangeInterval;
                if (buff || (damage >= 300 && _ctx.Health <= 0))
                {
                    _renderers.ForEach(value =>
                    {
                        value.material.DOColor(color.Value, colorShaderId, colorDuration / 2f)
                            .SetEase(Ease.OutCubic);
                    });
                    return;
                }
                _renderers.ForEach(value =>
                {
                    value.material.DOColor(color.Value, colorShaderId ,colorDuration / 2f)
                        .SetEase(Ease.OutCubic).OnComplete(() =>
                        {
                            value.material.DOColor(_currentColor, colorShaderId, colorDuration / 2f)
                                .SetEase(Ease.OutCubic);
                        });
                });

            }
        }

        void OnRepel(BuffAsset buff)
        {
            if (buff.target)
            {
                Vector3 dir = (buff.target.transform.position - transform.position).normalized;
                var target = transform.position - dir * buff.value;
                transform.DOMove(target, buff.duration).SetEase(Ease.OutCubic);
                // Vector3 dir = (buff.target.transform.position - transform.position).normalized;
                // transform.position -= dir * buff.value;
            }
        }

        void OnGather(BuffAsset buff)
        {
            if (buff.target)
            {
                
            }
        }

        void OnPoison(BuffAsset buff, int damage)
        {
            StartCoroutine(DoPoison((int)(damage * buff.value), buff.duration, 3));
        }

        IEnumerator DoPoison(int damage, float duration, int count)
        {
            float t = 0;
            int k = 0;
            while (k < count)
            {
                t += Time.deltaTime;
                yield return null;
                if (_zombieState >= ZombieState.Dead)
                    yield break;
                if (t < duration)
                    continue;
                k++;
                _ctx.DoDamage(damage);
                if (_ctx.Health <= 0)
                {
                    Death(null);
                    yield break;
                }
                zombieAnimator.StartAnimation(ZombieState.UnderAttacking);
            }
        }

        private float _imprisonTime;
        private Coroutine _imprisonIE;
        void OnImprison(BuffAsset buff)
        {
            var duration = buff.duration;
            _imprisonTime += duration;
            if (_imprisonIE == null)
                _imprisonIE = StartCoroutine(nameof(Imprison));
        }

        IEnumerator Imprison()
        {
            while (true)
            {
                if (_imprisonTime > 0)
                {
                    _imprisonTime -= Time.deltaTime;
                    if (_imprisonTime <= 0) _imprisonTime = -0.01f;
                    _agent.EnableAgent(false);
                }
                else if (_imprisonTime < 0)
                {
                    _imprisonTime = 0;
                    _agent.EnableAgent(true);
                }
                yield return null;
            }
        }
        
        /// <summary>
        /// 受击
        /// </summary>
        /// <param name="id"></param>
        /// <param name="originZombie"></param>
        /// <param name="userPlant"></param>
        /// <param name="itemCount"></param>
        public void UnderAttack(bool originZombie, UserPlantData userPlant, int itemCount)
        {
            // if (MultiplayerLogic.Instance.IsMultiPlay())
            // {
            //     _zombieNetworkObject.UnderAttack_ServerRpc(id, itemCount);
            //     return;
            // }
            // if (SceneLoadManager.Instance.GetActiveSceneIndex()
            //     == (int)SceneLoadManager.SceneIndex.MRScene) return;
            if (_zombieState >= ZombieState.Dead) return;
            if (GameplayMgr.Instance.GameplayState != GameplayState.Gaming) return;
            
            if (userPlant.plantId.ToLower().Contains("weapon"))
            {
                UnderAttackByWeapon(userPlant, true);
                return;
            }
            UnderAttackByPlant(userPlant, originZombie, itemCount, true);
        }

        /// <summary>
        /// 受到武器攻击
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userPlant"></param>
        /// <param name="applyValue"></param>
        void UnderAttackByWeapon(UserPlantData userPlant, bool applyValue)
        {
            if (applyValue)
            {
                int defenseValue = _enemyLevelData?.defenseValue ?? 0;
                int weaponDamage = 20;
                if (_zombieState >= ZombieState.Dead) return;
                if (GameplayMgr.Instance.GameplayState != GameplayState.Gaming) return;
                weaponDamage -= defenseValue;
                weaponDamage = Math.Max(weaponDamage, 1);
                _ctx.DoDamage(weaponDamage);
                if (_ctx.Health <= 0)
                    Death(null);
                else
                    zombieAnimator.StartAnimation(ZombieState.UnderAttacking);
            }
        }

        /// <summary>
        /// 受到植物攻击
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userPlant"></param>
        /// <param name="originZombie"></param>
        /// <param name="itemCount"></param>
        /// <param name="applyValue"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        void UnderAttackByPlant(UserPlantData userPlant, bool originZombie, int itemCount, bool applyValue)
        {
            if (userPlant == null) throw new Exception($"userPlant is null");
            var inherentData = PlantData.Instance.GetPlantInherentData(userPlant.plantId);
            var levelData = PlantData.Instance.GetPlantLevelData(userPlant.plantId, userPlant.plantLevel);
            if (levelData == null) throw new Exception($"levelData is null. id : {userPlant.plantId}");;
            if (applyValue)
            {
                int defenseValue = _enemyLevelData?.defenseValue ?? 0;
                float damage = levelData.attackValue;
                if (userPlant.elementType != null)
                {
                    var id = userPlant.elementType.elementId;
                    if (string.IsNullOrEmpty(id)) return;
                    var level = userPlant.elementType.elementLevel;
                    var elementData = ElementData.Instance.GetElementLevelData(id, level);
                    if (!originZombie)
                        damage *= elementData.damage;
                    if (userPlant.elementType is { elementId: "Element_03" })
                        SuperpositionPoisonElement(userPlant, elementData, (int)damage);
                    if (userPlant.elementType is { elementId: "Element_04" })
                        SuperpositionIceElement(elementData);
                }
                if (itemCount > 0) damage /= itemCount;
            
                damage -= defenseValue;
                damage = Math.Max(damage, 1);
                _ctx.DoDamage((int)damage);
                if (_ctx.Health <= 0)
                    Death(inherentData.attribute);
                else
                    zombieAnimator.StartAnimation(ZombieState.UnderAttacking);
            }
            
            ApplyElementEffect(userPlant);
            ApplyZombieUnderAttackColor(inherentData);
        }

        void ApplyElementEffect(UserPlantData userPlant)
        {
            var element = userPlant.elementType;
            if (element != null)
            {
                var id = userPlant.elementType.elementId;
                var level = userPlant.elementType.elementLevel;
                var elementData = ElementData.Instance.GetElementLevelData(id, level);
                var elementId = element.elementId;
                var elementObject = _elementItemObjects?.ToList().
                    FirstOrDefault(value => value.ElementId == elementId);
                if (elementObject != null)
                {
                    elementObject.gameObject.SetActive(true);
                    elementObject.Init(elementData);
                }
                else
                    Debug.LogWarning($"No ElementItemObject found for ElementType: {elementId}");
            }
        }

        private Dictionary<string, PoisonLayer> _poisonLayers = new();
        private float _poisonTimer = 1;
        void SuperpositionPoisonElement(UserPlantData userPlant, ElementLevelData elementLevelData, int damage)
        {
            string key = $"{userPlant.userPlantId}_{elementLevelData.level}";
            if (!_poisonLayers.ContainsKey(key))
            {
                _poisonLayers.Add(key, new PoisonLayer()
                {
                    damage = damage,
                    duration = elementLevelData.duration,
                    layer = 1,
                });
            }
            else
            {
                _poisonLayers[key].layer++;
                if (_poisonLayers[key].duration <= 0)
                    _poisonLayers[key].layer = 1;
                else
                    _poisonLayers[key].layer = 
                        Mathf.Min(_poisonLayers[key].layer, (int)elementLevelData.count);
                _poisonLayers[key].duration = elementLevelData.duration;
            }
        }

        void UpdatePoisonElement()
        {
            if (_poisonLayers.Count <= 0) return;
            _poisonTimer -= Time.deltaTime;
            foreach (var value in _poisonLayers)
                value.Value.duration -= Time.deltaTime;

            if (_poisonTimer < 0)
            {
                var value = (int)_poisonLayers.Values.ToList().Sum(value => value.GetDamage());
                if (value > 0)
                    DoPoisonDamage(value);
                _poisonTimer = 1;
            }
        }

        void DoPoisonDamage(int value)
        {
            int damage = value;
            int defenseValue = _enemyLevelData?.defenseValue ?? 0;
            damage -= defenseValue;
            damage = Math.Max(damage, 1);
            _ctx.DoDamage((int)damage);
            if (_ctx.Health <= 0)
                Death("plant");
            else
                zombieAnimator.StartAnimation(ZombieState.UnderAttacking);
        }

        public void PlayAnimation(int state)
        {
            if (zombieAnimator)
                zombieAnimator.StartAnimation((ZombieState)state);
        }

        private float _maxIceValue = 1;
        private float _maxIceDuration = 1;
        void SuperpositionIceElement(ElementLevelData userPlant)
        {
            _maxIceDuration = 1;
            float value = userPlant.damage;
            float duration = userPlant.duration;
            _maxIceDuration = Mathf.Max(_maxIceDuration, duration);
            _maxIceValue = Mathf.Min(_maxIceValue, value);
        }

        void UpdateIceElement()
        {
            _maxIceDuration -= Time.deltaTime;
            if (_maxIceDuration < 0)
                _maxIceValue = 1;
            agent.ChangeSpeed(_enemyLevelData.speed * _maxIceValue);
        }
        
        void ApplyZombieUnderAttackColor(PlantInherentData inherentData)
        {
            Color? color = null;
            if (!string.IsNullOrEmpty(inherentData.attackColor))
            {
                var colorRGB = inherentData.attackColor.Split(',');
                if (colorRGB.Length > 2)
                    color = new Color
                    {
                        r = float.Parse(colorRGB[0]) / 255,
                        g = float.Parse(colorRGB[1]) / 255,
                        b = float.Parse(colorRGB[2]) / 255,
                        a = 1
                    };
            }
            bool bombDie = inherentData.attribute != null && inherentData.attribute.Contains("爆炸") && _ctx.Health <= 0;
            if (MultiplayerLogic.Instance.IsHost() || !MultiplayerLogic.Instance.IsMultiPlay())
            {
                ApplyUnderAttackColor(color, bombDie);
                if (MultiplayerLogic.Instance.IsMultiPlay())
                {
                    Color c = color ?? Color.white;
                    _zombieNetworkObject.ApplyAttackColor_ClientRpc(color.HasValue, c.r, c.g, c.b, bombDie);
                }
            }
        }
        
        /// <summary>
        /// 受击颜色变化
        /// </summary>
        /// <param name="color"></param>
        /// <param name="bombDie"></param>
        public void ApplyUnderAttackColor(Color? color, bool bombDie)
        {
            if (color.HasValue && (_currentTime < 0 || bombDie))
            {
                _currentTime = colorCanChangeInterval;
                if (bombDie)
                {
                    _renderers.ForEach(value =>
                    {
                        value.material.DOColor(color.Value, colorShaderId, colorDuration / 2f)
                            .SetEase(Ease.OutCubic);
                    });
                    return;
                }
                _renderers.ForEach(value =>
                {
                    value.material.DOColor(color.Value, colorShaderId ,colorDuration / 2f)
                        .SetEase(Ease.OutCubic).OnComplete(() =>
                        {
                            value.material.DOColor(_currentColor, colorShaderId, colorDuration / 2f)
                                .SetEase(Ease.OutCubic);
                        });
                });
            }
        }
        
        void Recover()
        {
            agent.ChangeSpeed(_enemyLevelData.speed);
            _renderers.ForEach(value =>
            {
                value.material.DOColor(_currentColor, colorShaderId, colorDuration / 2f)
                    .SetEase(Ease.OutCubic);
            });
            zombieAnimator.SetSpeed(1);
        }
        /// <summary>
        /// 死亡
        /// </summary>
        /// <param name="attribute"></param>
        void Death(string attribute)
        {
            bool bomb = attribute != null && attribute.Contains("爆炸");
            if (_zombieState >= ZombieState.Dead) return;
            agent.Stop();
            var colliders = GetComponentsInChildren<Collider>().ToList();
            colliders.ForEach(value => value.enabled = false);
            StopAllCoroutines();
            DOTween.Kill(transform);
            ChangeBehaviour(_zombieState, bomb ? ZombieState.DeadBomb : ZombieState.Dead, () =>
            {
                _audioSource.clip = deathAudios[Random.Range(0, 10) % deathAudios.Length];
                _audioSource.Play();
            });

            Destroy(gameObject, destroyTime);
            if (SceneLoadManager.Instance.GetActiveSceneIndex()
                == (int)SceneLoadManager.SceneIndex.MRScene)
            {
                var minute = _minute;
                float coin = _zombieAsset.coin;
                float exp = _zombieAsset.exp;
                int k = 0;
                while (minute > 0)
                {
                    minute --;
                    coin *= 1 + 0.1f * k;
                    exp *= 1 + 0.1f * k;
                    k++;
                }
                GameEventArg arg = EventDispatcher.Instance.GetEventArg((int)EventID.MONSTERDEAD);
                arg.SetArg(0, _zombieAsset.score);
                arg.SetArg(1, (int)coin);
                arg.SetArg(2, (int)exp);
                // arg.SetArg(1, transform.position);
                EventDispatcher.Instance.Dispatch((int)EventID.MONSTERDEAD);
            }
            GameEventArg arg0 = EventDispatcher.Instance.GetEventArg((int)EventID.EnemyDead);
            arg0.SetArg(0, _enemyId);
            arg0.SetArg(1, _enemyLevelData.level);
            arg0.SetArg(2, _enemyLevelData.expValue);
            EventDispatcher.Instance.Dispatch((int)EventID.EnemyDead);
            for (int i = 0; i < _skills.Count; i++)
                _skills[i].OnZombieDeath();
            InvokeRepeating(nameof(ZombieDissolve), 3, 0.03f);
        }
        /// <summary>
        /// 消融
        /// </summary>
        void ZombieDissolve()
        {
            float value = _renderers[0].material.GetFloat(dissolveShaderId);
            value -= 0.03f * 0.6f;
            _renderers.ForEach(renderer => renderer.material.SetFloat(dissolveShaderId, value));
            if (value <= 0)
            {
                CancelInvoke(nameof(ZombieDissolve));
            }
        }
        
        public void Kill()
        {
            if (_zombieState >= ZombieState.Dead) return;
            agent.Stop();
            var colliders = GetComponentsInChildren<Collider>().ToList();
            colliders.ForEach(value => value.enabled = false);
            StopAllCoroutines();
            CancelInvoke();
            DOTween.Kill(transform);
            ChangeBehaviour(_zombieState, ZombieState.Dead, () =>
            {
                _audioSource.Stop();
            });
            Destroy(gameObject, destroyTime);
            for (int i = 0; i < _skills.Count; i++)
                _skills[i].OnZombieDeath();
            InvokeRepeating(nameof(ZombieDissolve), 3, 0.03f);
        }
        
        void Celebrate()
        {
            if (_zombieState >= ZombieState.Dead) return;
            agent.Stop();
            var colliders = GetComponentsInChildren<Collider>().ToList();
            colliders.ForEach(value => value.enabled = false);
            StopAllCoroutines();
            CancelInvoke();
            DOTween.Kill(transform);
            ChangeBehaviour(_zombieState, ZombieState.Celebrate, () =>
            {
                _audioSource.Stop();
            });
        }

        private NetworkEnemy _zombieNetworkObject;
        private Action<int> _networkStateChanged;
        public void OnNetworkSpawn(NetworkEnemy networkObject, bool isRemote, Action<int> callback)
        {
            _zombieNetworkObject = networkObject;
            _networkStateChanged = callback;
            if (!isRemote) return;
            IsVisibility = false;
            VisibilitySide = 0;
            _zombieState = ZombieState.Idle;
            zombieAnimator.Init();
            Invoke(nameof(DelaySpawn), 0.05f);
        }

        void DelaySpawn()
        {
            _portal = Instantiate(portal);
            _portal.SetActive(true);
            _portal.transform.SetPositionAndRotation(portal.transform.position, portal.transform.rotation);
            var localScale = _portal.transform.GetChild(0).localScale;
            float originScaleX = localScale.x;
            localScale.x = 0;
            _portal.transform.GetChild(0).localScale = localScale;
            _portal.transform.GetChild(0).DOScaleX(originScaleX, portalDuration);
            Invoke(nameof(DestroyPortal), portalHideTime);
        }
        
        void DestroyPortal()
        {
            _portal.transform.GetChild(0).DOScaleX(0, portalDuration).OnComplete(() =>
            {
                Destroy(_portal);
            });
        }

        public void NetworkMove()
        {
            if (_audioSource.clip != walkAudio)
                _audioSource.clip = walkAudio;
            if (!_audioSource.isPlaying)
                _audioSource.Play();
            _groanAudioSource.clip = groanAudio[Random.Range(0, 10) % groanAudio.Length];
            _groanAudioSource.Play();
            _zombieState = ZombieState.Moving;
            zombieAnimator.StartAnimation(_zombieState);
        }

        public void NetworkAttack()
        {
            if (_audioSource.clip != attackAudio)
                _audioSource.clip = attackAudio;
            if (!_audioSource.isPlaying)
                _audioSource.Play();
            _groanAudioSource.Stop();
            _zombieState = ZombieState.Attacking;
            zombieAnimator.StartAnimation(_zombieState);
        }

        public void NetworkDead(ZombieState state)
        {
            _audioSource.Stop();
            _zombieState = state;
            zombieAnimator.StartAnimation(_zombieState);
            Destroy(gameObject, destroyTime);
            InvokeRepeating(nameof(ZombieDissolve), 3, 0.03f);
        }

        public void NetworkCelebrate()
        {
            _zombieState = ZombieState.Celebrate;
            zombieAnimator.StartAnimation(_zombieState);
            _audioSource.Stop();
        }
        
        public void NetworkUnderAttack(bool isServer, string id, int itemCount)
        {
            if (_zombieState >= ZombieState.Dead) return;
            if (GameplayMgr.Instance.GameplayState != GameplayState.Gaming) return;
            
            // if (id.ToLower().Contains("weapon"))
            // {
            //     UnderAttackByWeapon(id, isServer);
            //     return;
            // }
            // UnderAttackByPlant(id, itemCount, isServer);
        }
    }

    public class PoisonLayer
    {
        public int damage;
        public int layer;
        public float duration;

        public float GetDamage()
        {
            return duration <= 0 ? 0 : damage * layer;
        }
    }
    
    public enum ZombieState
    {
        Spawn,
        Idle,
        Moving,
        Attacking,
        UnderAttacking,
        Dead,
        DeadBomb,
        Celebrate
    }
}