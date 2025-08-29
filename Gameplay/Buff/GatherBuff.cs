using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Script.Gameplay
{
    public class GatherBuff : MonoBehaviour
    {
        [SerializeField] private List<ZombieBehaviour> _zombies = new();
        private BuffAsset _buff;
        int _damage;
        Color? _color;
        private void Start()
        {

        }

        private void Update()
        {

        }

        public void Init(BuffAsset buff, int damage, Color? color)
        {
            _buff = buff;
            _damage = damage;
            _color = color;
            StartCoroutine(nameof(RecoverAgent));
            // Destroy(gameObject, buff.duration);
        }

        IEnumerator RecoverAgent()
        {
            float t = 0;
            while (t < _buff.duration)
            {
                t +=  Time.deltaTime;
                yield return null;
                if (TryFindNearbyEnemies(_buff.value, ref _zombies))
                {
                    for (int i = 0; i < _zombies.Count; i++)
                    {
                        var zombie = _zombies[i];
                        if (zombie != null && zombie.ZombieState <= ZombieState.Dead)
                        {
                            var current = zombie.transform.position;
                            var target = gameObject.transform.position;
                            target.y = current.y;
                            if (zombie.gameObject.TryGetComponent(out NavMeshAgent agent))
                            {
                                agent.enabled = false;
                                agent.radius = 0.1f;
                                var dis = Vector3.Distance(target, current);
                                if (dis <= 0.3f && _zombies.Count > 0)
                                    continue;
                            }
                            zombie.transform.position = Vector3.Lerp(current, target, Time.deltaTime * 10);
                        }
                    }
                }
            }
            t = 0;
            while (t < 0.4f)
            {
                t +=  Time.deltaTime;
                yield return null;
                for (int i = 0; i < _zombies.Count; i++)
                {
                    var zombie = _zombies[i];
                    if (zombie != null && zombie.ZombieState <= ZombieState.Dead)
                    {
                        if (zombie.gameObject.TryGetComponent(out NavMeshAgent agent))
                        {
                            agent.enabled = true;
                            if (agent.radius < 0.5f)
                                agent.radius += t;
                        }
                    }
                }
            }
            
            Destroy(gameObject);
        }
        
        public bool TryFindNearbyEnemies(float range, ref List<ZombieBehaviour> zombies)
        {
            float distance = range;
            var list = ZombieSystem.Instance.ZombieBehaviours;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    Vector3 pos1 = list[i].transform.position;
                    Vector3 pos2 = transform.position;
                    pos1.y = pos2.y = 0;
                    if (Vector3.Distance(pos1, pos2) <= distance && list[i].ZombieState < ZombieState.Dead)
                    {
                        distance = Vector3.Distance(pos1, pos2);
                        if (!zombies.Contains(list[i]))
                        {
                            list[i].UnderAttack(_damage, _buff, _color);
                            zombies.Add(list[i]);
                        }
                    }
                }
            }

            return zombies.Count > 0;
        }
    }
}