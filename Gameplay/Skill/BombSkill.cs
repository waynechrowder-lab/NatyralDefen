using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class BombSkill : MonoBehaviour, IPlantSkill
    {
        public BombSkillAsset Data;
        private PlantContext _ctx;
        private bool _triggered;
        private Coroutine _co;

        public void Init(PlantContext context, PlantSkillAsset asset)
        {
            _ctx = context;
            Data = (BombSkillAsset)Instantiate(asset);
            _triggered = false;
        }


        public void Tick(float dt)
        {
            if (_triggered) return;
            _triggered = true;
            _co = StartCoroutine(DoBomb());     
        }


        IEnumerator DoBomb()
        {
            List<ZombieBehaviour> list;
            while (!_ctx.TryFindNearbyEnemies(Data.radius, out list))
                yield return new WaitForSeconds(0.1f);
            _ctx.Animator.StartAnimation(PlantState.Attacking);
            yield return new WaitForSeconds(Data.triggerDelay);
            _ctx.TryFindNearbyEnemies(Data.radius, out list);
            // list = ZombieSystem.Instance.ZombieBehaviours;
            for (int i = 0; i < list.Count; i++)
            {
                var z = list[i];
                if (z == null || z.ZombieState >= ZombieState.Dead) continue;
                z.UnderAttack(Data.damage, Data.buffAsset, Data.attackColor);
            }
            if (Data.boomPrefab) Destroy(Instantiate(Data.boomPrefab, _ctx.Transform.position, _ctx.Transform.rotation), 2f);
            Destroy(gameObject);
        }


        public void OnUpgrade(IntensifyData data)
        {
            var currentLevel = _ctx.Level;
            if (currentLevel <= 5)
            {
                Data.radius *= data.attackRangeK;
                Data.damage = (int)(data.attackValueK * Data.damage);
                Data.cooldown *= data.attackInterval;
            }
        }
        public void OnPaused(bool paused) { }
        public void OnPlantDeath() { if (_co != null) StopCoroutine(_co); }
    }
}