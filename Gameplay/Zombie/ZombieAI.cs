using System;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    /// <summary>
    /// Lightweight zombie AI state machine.
    /// </summary>
    public class ZombieAI : MonoBehaviour
    {
        /// <summary>Current AI state.</summary>
        public ZombieState State { get; private set; } = ZombieState.Idle;

        /// <summary>Raised when state changes.</summary>
        public event Action<ZombieState> OnStateChanged;

        /// <summary>Change to a new state.</summary>
        public void ChangeState(ZombieState newState)
        {
            if (State == newState) return;
            State = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}

