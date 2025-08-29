using System;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    /// <summary>
    /// Controls zombie hit points and death behaviour.
    /// </summary>
    public class ZombieHealth : MonoBehaviour
    {
        [SerializeField] private int _health = 100;

        /// <summary>Current health of the zombie.</summary>
        public int CurrentHealth => _health;

        /// <summary>Raised when health drops to zero.</summary>
        public event Action OnDeath;

        /// <summary>Initialise health value.</summary>
        public void Init(int health)
        {
            _health = health;
        }

        /// <summary>Apply damage to the zombie.</summary>
        public void TakeDamage(int damage)
        {
            if (_health <= 0) return;
            _health -= damage;
            if (_health <= 0)
            {
                _health = 0;
                OnDeath?.Invoke();
            }
        }
    }
}

