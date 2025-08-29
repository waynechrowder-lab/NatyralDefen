

using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    /// <summary>
    /// Coordinates zombie sub components like health, audio and AI.
    /// </summary>
    [RequireComponent(typeof(ZombieHealth))]
    [RequireComponent(typeof(ZombieAudio))]
    [RequireComponent(typeof(ZombieAI))]
    public class ZombieBehaviour : MonoBehaviour
    {
        private ZombieHealth _health;
        private ZombieAudio _audio;
        private ZombieAI _ai;

        private void Awake()
        {
            _health = GetComponent<ZombieHealth>();
            _audio = GetComponent<ZombieAudio>();
            _ai = GetComponent<ZombieAI>();
            _health.OnDeath += HandleDeath;
        }

        private void HandleDeath()
        {
            _ai.ChangeState(ZombieState.Dead);
            _audio.PlayDeath();
        }

        /// <summary>Initialise zombie components.</summary>
        public void Init(int health)
        {
            _health.Init(health);
            _ai.ChangeState(ZombieState.Idle);
        }

        /// <summary>Move behaviour.</summary>
        public void Move()
        {
            _ai.ChangeState(ZombieState.Moving);
            _audio.PlayWalk();
        }

        /// <summary>Attack behaviour.</summary>
        public void Attack()
        {
            _ai.ChangeState(ZombieState.Attacking);
            _audio.PlayAttack();
        }

        /// <summary>Apply damage to the zombie.</summary>
        public void UnderAttack(int damage)
        {
            _health.TakeDamage(damage);
            if (_health.CurrentHealth > 0)
            {
                _ai.ChangeState(ZombieState.UnderAttacking);
                _audio.PlayUnderAttack();
            }
        }
    }
}


