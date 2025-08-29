using DG.Tweening;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class AttackSkill : MonoBehaviour, IPlantSkill
    {
        public AttackSkillAsset Data;
        [SerializeField] private PlantContext _ctx;
        [SerializeField] private float _timer;
        private ZombieBehaviour _target;
        private Transform _origin;
        [SerializeField] private bool _attacking;
        private bool _createBullet;
        public void Init(PlantContext context, PlantSkillAsset asset)
        {
            _ctx = context;
            Data = (AttackSkillAsset)Instantiate(asset);
            _timer = Data.cooldown;
            if (!string.IsNullOrEmpty(Data.originName))
                _origin = transform.Find(Data.originName);
            else
                _origin = transform;
            _ctx.Animator.RegisterAnimationEvent(AnimatorEventCallback);
        }
        
        public void Tick(float dt)
        {
            var level = Mathf.Min(_ctx.Level, IPlantSkill.MaxLevel);
            if (Data.applyLevels is { Count: > 0 } && !Data.applyLevels.Contains(level))
                return;
            _timer += dt;
            float range = Data.attackRange;

            // if (_target == null || Vector3.Distance(_target.transform.position, _ctx.Transform.position) > range)
            // {
            //     _ctx.TryFindNearestEnemy(range, out _target);
            // }
            _ctx.TryFindNearestEnemy(range, out _target);
            if (_target == null) return;
            if (_target.ZombieState >= ZombieState.Dead)
            {
                _target = null;
                return;
            }
            var from = _ctx.Transform.position; from.y = 0;
            var to = _target.transform.position; to.y = 0;
            var rot = Quaternion.LookRotation(to - from);
            _ctx.Transform.DORotateQuaternion(rot, 0.1f);
            
            var damageTime = Data.cooldown + Data.damageDelayVisual + Data.duration;
            if (_timer >= damageTime && _attacking) {
                if (Data.mode == AttackSkillAsset.AttackMode.Melee)
                    DoMelee();
                _timer = 0f;
                _attacking = false;
                _ctx.Scheduler.OnSkillFinished(this);
                _ctx.Animator.StartAnimation(PlantState.Idle);
            }
            
            if (_timer >= Data.cooldown)
            {
                if (!_attacking)
                {
                    if (!_ctx.Scheduler.TryRequest(this))
                        return;
                    _attacking = true;
                    _timer = Data.cooldown;
                    _createBullet = false;
                    _ctx.Animator.StartAnimation(PlantState.Attacking, Data.animatorName);

                    if (string.IsNullOrEmpty(Data.animatorName)) {
                        if (Data.mode == AttackSkillAsset.AttackMode.Ranged) FireProjectile();
                        else if (Data.mode == AttackSkillAsset.AttackMode.Scope) DoRanged();
                    }
                }
                if (Data.buffAsset)
                    Data.buffAsset.target = _ctx.Host;
            }
        }

        void AnimatorEventCallback(string eventName)
        {
            if ((Data.applyLevels is { Count: > 0 } && !Data.applyLevels.Contains(_ctx.Level)) || Data.animatorName != eventName)
                return;

            if (!_createBullet)
                FireProjectile();
            switch (eventName)
            {
                case "Attack":
                    
                    break;
                case "Skill1":
                    break;
                case "Skill2":
                    break;
            }
        }

        void FireProjectile()
        {
            if (!_target) return;
            if (!Data.projectilePrefab || !_origin) return;
            _createBullet = true;
            var pos1 = _target ? _target.transform.position : _ctx.Transform.position + _ctx.Transform.forward * 2f;
            var pos2 = _origin.position; pos1.y = pos2.y = 0;
            var dir = (pos1 - pos2).normalized;
            pos2.y = _origin.position.y;
            var go = Instantiate(Data.projectilePrefab, pos2, Quaternion.LookRotation(dir));
            
            var plantBullet = go.GetComponentInChildren<UserBullet>();
            if (plantBullet)
            {
                if (plantBullet is PlantUserBullet bullet)
                {
                    bullet.Init(Data.attackValue, Data.buffAsset, Data.attackColor);
                }
                else if (plantBullet is CustomFireBullet fire)
                {
                    fire?.Init(Data.attackValue, Data.buffAsset, Data.attackColor, _origin, Data.duration);
                    Destroy(fire.gameObject, Data.duration - 0.65f);
                }
            }
        }


        void DoMelee()
        {
            if (!_target) return;

            _target.UnderAttack(Data.attackValue, Data.buffAsset, Data.attackColor);
            if (Data.projectilePrefab)
            {
                var go = Instantiate(Data.projectilePrefab);
                var plantBullet = go.GetComponent<UserBullet>();
            }

        }

        void DoRanged()
        {
            if (_ctx.TryFindNearbyEnemies(Data.attackRange, out var zombies))
            {
                for (int i = 0; i < zombies.Count; i++)
                {
                    var zombie = zombies[i];
                    if (zombie != null && zombie.ZombieState <= ZombieState.Dead)
                    {
                        zombie.UnderAttack(Data.attackValue, Data.buffAsset, Data.attackColor);
                    }
                }
            }
        }
        
        public void OnUpgrade(IntensifyData data)
        {
            var currentLevel = _ctx.Level;
            if (currentLevel <= 5)
            {
                Data.attackRange *= data.attackRangeK;
                Data.attackValue = (int)(data.attackValueK * Data.attackValue);
                Data.cooldown *= data.attackInterval;
                _timer = Data.cooldown;
            }
        }


        public void OnPaused(bool paused) {  }
        public void OnPlantDeath() {  }
    }
}