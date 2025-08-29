using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Gameplay.Script.Data;
using Gameplay.Script.Element;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using Gameplay.Script.MiniWorldGameplay;
using Gameplay.Script.MultiplayerModule;
// using Gameplay.Script.MultiplayerModule;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.Script.Gameplay
{
    public class PlantBehaviour : MonoBehaviour
    {
        [SerializeField] private PlantUpgradeUIMono upgradeUIMono;
        [Header("投掷力度offset")]
        [SerializeField] private float force = 1;
        
        [Header("落地后设置")]
        public float checkDistance = 0.1f;
        public LayerMask groundLayer;
        
        private PlantState _plantState = PlantState.Spawn;
        public bool IsAlive => _plantState < PlantState.Dead;
        private Rigidbody rb;
        private Vector3 lastPosition;
        private Quaternion lastRotation;

        private float interval = 0.02f;
        private Vector3 velocity;
        private Quaternion deltaRotation;
        private Vector3 velocityRot;

        private bool hasLanded = false;
        private float landingTime;
        private Quaternion targetRotation;

        private PhysicsController _physicsController;
        private Transform _old;
        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("Unplace");
            _physicsController = gameObject.AddComponent<PhysicsController>();
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            EventDispatcher.Instance.Register((int)EventID.GAMEOVER, OnGameOver);
            EventDispatcher.Instance.Register((int)EventID.GAMERESTART, OnRestart);
            ApplyUpgradeVisual(0);
        }

        public void Initialize(IGameplayManager gameplayManager)
        {
            _gameplayManager = gameplayManager;
        }

        [SerializeField] private ModularPlantAsset definition;

        private readonly List<IPlantSkill> _skills = new();
        private PlantContext _ctx;
        private PlantAnimator _anim;
        private IGameplayManager _gameplayManager;
        
        void SetPlantContext()
        {
            if (!gameObject.TryGetComponent(out _anim))
                _anim = gameObject.AddComponent<PlantAnimator>();
            _ctx = new PlantContext
            {
                Host = gameObject,
                Transform = transform,
                Animator = _anim,
                Health = definition ? definition.baseHealth : 100,
                Level = 0
            };
            if (definition)
            {
                foreach (var asset in definition.skills)
                {
                    if (!asset) continue;
                    var skill = asset.AddTo(gameObject);
                    skill.Init(_ctx, asset);
                    _skills.Add(skill);
                }
            }
        }
        
        private void OnDestroy()
        {
            EventDispatcher.Instance.UnRegister((int)EventID.GAMEOVER, OnGameOver);
            EventDispatcher.Instance.UnRegister((int)EventID.GAMERESTART, OnRestart);
            DOTween.Kill(transform);
        }
        
        void Update()
        {
            interval -= Time.deltaTime;
            
            if (interval < 0)
            {
                interval = 0.03f;
                velocity = (transform.position - lastPosition) / interval;
                deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);
                deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
                velocityRot = axis * (angle * Mathf.Deg2Rad / interval);
                lastPosition = transform.position;
                lastRotation = transform.rotation;
            }

            if (hasLanded && _plantState < PlantState.Dead)
            {
                float dt = Time.deltaTime;
                for (int i = 0; i < _skills.Count; i++)
                    _skills[i].Tick(dt);
            }
        }
        
        void FixedUpdate()
        {
            if (gameObject.layer == LayerMask.NameToLayer("Unplace")) return;
            if (!hasLanded && IsGrounded())
            {
                rb.velocity *= 0.5f;
                rb.angularVelocity *= 0.5f;
                hasLanded = true;
                rb.drag = 2f;
                rb.angularDrag = 5f;
                rb.maxAngularVelocity = 5f;
                if (_anim) _anim.SetAnimatorActive(true);
            }

            if (hasLanded)
            {
                _physicsController?.ApplyStabilization(rb);
                _physicsController?.ApplyAlignmentTorque(rb);
            }
        }
        
        public void OnGamePause()
        {

        }
        
        public void OnGameUnPause()
        {

        }
        
        bool IsGrounded()
        {
            return Physics.Raycast(transform.position, Vector3.down, checkDistance, groundLayer);
        }

        private void OnGameOver(GameEventArg arg)
        {
            CancelInvoke();
            if (gameObject.layer != LayerMask.NameToLayer("Plant"))
                return;
            bool success = arg.GetArg<bool>(0);
            if (!success)
                Kill();
            else
                Celebrate();
        }
        
        private void OnRestart(GameEventArg arg)
        {
            Destroy(gameObject);
        }

        public void SetAsPreview()
        {
            gameObject.layer = LayerMask.NameToLayer("Unplace");
        }
        
        public void OnSpawn(string plantId)
        {
            SetPlantContext();
            _ctx.SetPlantData(plantId);
            _anim.SetAnimatorActive(false);
            gameObject.layer = LayerMask.NameToLayer("Plant");
            _plantState = PlantState.Idle;
            Drop();
        }

        public void Drop()
        {
            rb.isKinematic = false;
            rb.velocity = velocity * force;
            rb.angularVelocity = velocityRot;
        }
        
        public void UnderAttack(int damage)
        {
            if (_plantState >= PlantState.Dead) return;
            if (_gameplayManager != null && _gameplayManager.GameplayState != GameplayState.Gaming) return;
            _ctx.DoDamage(damage);
            var health = _ctx.Health;

            if (health <= 0)
            {
                Death();
            }
            else
            {
                _plantState = PlantState.UnderAttacking;
                _anim.StartAnimation(_plantState);
            }
        }

        void Death(float delay = 2)
        {
            if (_plantState >= PlantState.Dead) return;
            StopAllCoroutines();
            DOTween.Kill(transform);
            _plantState = PlantState.Dead;
            if (_anim) 
            {
                _anim.SetAnimatorActive(true);
                _anim.SetSpeed(1);
                _anim.StartAnimation(_plantState);
            }
            Destroy(gameObject, delay);
        }

        public void Kill()
        {
            if (_plantState >= PlantState.Dead) return;
            StopAllCoroutines();
            CancelInvoke();
            DOTween.Kill(transform);
            _plantState = PlantState.Dead;
            if (_anim) 
            {
                _anim.SetAnimatorActive(true);
                _anim.SetSpeed(1);
                _anim.StartAnimation(_plantState);
            }
            Destroy(gameObject, 2);
        }

        void Celebrate()
        {
            if (_plantState >= PlantState.Dead) return;
            StopAllCoroutines();
            CancelInvoke();
            DOTween.Kill(transform);
            _plantState = PlantState.Celebrate;
            if (_anim) 
            {
                _anim.SetAnimatorActive(true);
                _anim.SetSpeed(1);
                _anim.StartAnimation(_plantState);
            }
        }

        public void ShowUpgradeUI()
        {
            if (upgradeUIMono)
                upgradeUIMono.DoUpgrade();
        }
        
        public void Upgrade(IntensifyData data)
        {
            if (!_ctx.OnUpgrade(data))
                return;
            var currentLevel = _ctx.Level;
            ApplyUpgradeVisual(currentLevel);
            for (int i = 0; i < _skills.Count; i++)
                _skills[i].OnUpgrade(data);

            if (upgradeUIMono)
                upgradeUIMono.DoUpgrade(currentLevel);
        }

        void ApplyUpgradeVisual(int level)
        {
            if (definition != null && definition.upgradeStages != null)
            {
                for (int i = 0; i < definition.upgradeStages.Count; i++)
                {
                    var stage = definition.upgradeStages[i];

                    if (stage != null)
                    {
                        var trans = transform.Find(stage.replacePath);
                        if (trans)
                        {
                            if (level == stage.requiredLevel)
                            {
                                trans.gameObject.SetActive(true);
                                if (!gameObject.TryGetComponent(out _anim))
                                    _anim = gameObject.AddComponent<PlantAnimator>();
                                _anim.InitAnimator(trans);
                                _anim.SetAnimatorActive(true);
                                _anim.StartAnimation(_plantState);
                                if (_old) _old.gameObject.SetActive(false);
                                _old = trans;
                            }
                            else if (level == 0)
                                trans.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
        
        #region Network
        
        public void OnNetworkInit(string id)
        {
            // _plantId = id;
            gameObject.layer = LayerMask.NameToLayer("Plant");
            _plantState = PlantState.Idle;
            // _plantAnimator.Init();
            // _plantAnimator.SetAnimatorActive(true);
        }
        
        public void OnNetworkBomb()
        {
            StartCoroutine(NetworkBomb());
        }

        IEnumerator NetworkBomb()
        {
            // _plantAnimator.StartAnimation(PlantState.Attacking);
            yield return new WaitForSeconds(1.5f);
            // _audioSource.clip = attackAudio;
            // _audioSource.Play();
            // Destroy(Instantiate(boomPrefab, transform.position, transform.rotation), 2);
            Destroy(gameObject);
        }
        
        public void OnNetworkAttack(int state, Vector3 targetPos)
        {
            _plantState = PlantState.Attacking;
            // _plantAnimator.StartAnimation(_plantState);
            var plantType = (PlantType)state;
            // if (plantType is PlantType.远程攻击 or PlantType.近战)
            // {
            //     var obj = Instantiate(this.userBullet);
            //     var pos1 = targetPos;
            //     var pos2 = bulletOrigin.position;
            //     pos1.y = pos2.y = 0;
            //     var dir = pos1 - pos2;
            //     obj.transform.SetPositionAndRotation(pos2, Quaternion.LookRotation(dir));
            //     if (plantType == PlantType.远程攻击)
            //     {
            //         //todo:network bullet
            //         // (obj as PlantBullet)?.Init(_plantAsset.attackValue, _plantAsset.buffAsset,
            //         //     _plantAsset.attackColor);
            //     }
            //     else
            //     {
            //         _audioSource.clip = attackAudio;
            //         _audioSource.Play();
            //     }
            // }
            // else
            // {
            //     _audioSource.clip = attackAudio;
            //     _audioSource.Play();
            // }
        }

        public void OnNetworkIdle()
        {
            _plantState = PlantState.Idle;
            // _plantAnimator.StartAnimation(_plantState);
        }
        
        public void OnNetworkUnderAttack()
        {
            _plantState = PlantState.UnderAttacking;
            // _plantAnimator.StartAnimation(_plantState);
        }
        
        public void OnNetworkDeath()
        {
            _plantState = PlantState.Dead;
            // _plantAnimator.StartAnimation(_plantState);
        }
        
        public void OnNetworkCelebrate()
        {
            _plantState = PlantState.Dead;
            // _plantAnimator.StartAnimation(PlantState.Celebrate);
        }
        #endregion 
    }
    
    public enum PlantState
    {
        Spawn,
        Idle,
        Attacking,
        Bomb,
        UnderAttacking,
        Dead,
        Celebrate
    }
}