using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Gameplay.Script.Agent
{
    public class AdvancedNavMeshAgent : MonoBehaviour
    {
        private Transform _target;
        private float _patrolRadius;
        private NavMeshAgent _agent;
        private Vector3? _patrolPoint = null;
        bool _initialized = false;
        private float _stopRadius;

        private void Start()
        {
            if (!_agent)
                _agent = GetComponent<NavMeshAgent>();
            if (_agent)
                _agent.enabled = false;
        }

        public void Init(Transform target, float speed, float stopRadius, float patrolRadius = 10)
        {
            _initialized = true;
            if (!_agent)
                _agent = GetComponent<NavMeshAgent>();
            _target = target;
            _agent.speed = speed;
            _patrolRadius = patrolRadius;
            _stopRadius = stopRadius;
            _agent.stoppingDistance = _stopRadius;
            Invoke(nameof(EnableAgent), .1f);
        }

        public void ChangeTarget(Transform target, float? range = null)
        {
            range ??= _stopRadius;
            _target = target;
            _stopRadius = (float)range;
        }

        public void ChangeSpeed(float speed)
        {
            _agent.speed = speed;
            Debug.Log($"ChangeSpeed: {speed}");
        }

        public void Pause()
        {
            if (!_initialized) return;
            if (!_agent.isOnNavMesh) return;
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            _agent.updatePosition = false;
            _agent.updateRotation = false;
            _initialized = false;
        }

        public void UnPause()
        {
            if (!_agent.isOnNavMesh) return;
            _agent.isStopped = false;
            _agent.updatePosition = true;
            _agent.updateRotation = true;
            _initialized = true;
        }

        public void Stop()
        {
            _initialized = false;
            if (!_agent.enabled)
                _agent.enabled = true;
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
        }

        public void EnableAgent(bool enabled)
        {
            _agent.enabled = enabled;
        }
        
        void EnableAgent()
        {
            _agent.enabled = true;
            _agent.isStopped = false;
        }
        
        void Update()
        {
            if (!_initialized) return;
            if (!_agent.isOnNavMesh) return;
            if (_target != null)
            {
                _agent.SetDestination(_target.position);
                Vector3 v1 = transform.position;
                Vector3 v2 = _target.position;
                v1.y = v2.y = 0;
                if (Vector3.Distance(v1, v2) <= _stopRadius)
                {
                    _agent.isStopped = true;
                    _agent.velocity = Vector3.zero;
                }
                else
                    _agent.isStopped = false;
                Quaternion rot = Quaternion.LookRotation(v2 - v1);
                transform.rotation = rot;
            }
            else if (_patrolPoint == null || _agent.remainingDistance < 0.5f)
            {
                GenerateRandomPatrolPoint();
                _agent.SetDestination(_patrolPoint ?? Vector3.zero);
            }
            
        }

        void GenerateRandomPatrolPoint()
        {
            Vector3 randomDirection = Random.insideUnitSphere * _patrolRadius;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, _patrolRadius, NavMesh.AllAreas);
            _patrolPoint = hit.position;
        }
    }
}