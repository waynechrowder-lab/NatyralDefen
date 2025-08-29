using System;
using Currency.Core.Run;
using UnityEngine;

namespace Gameplay.Script.Manager
{
    public class GameplayMgr : MonoSingleton<GameplayMgr>, IGameplayManager
    {
        private GameplayState _gameplayState = GameplayState.None;

        public GameplayState GameplayState
        {
            get => _gameplayState;
            protected set => _gameplayState = value;
        }

        private Action<GameplayState, GameplayState> _gameplayStateChangeCallback;

        public void SetGameplayState(GameplayState state)
        {
            _gameplayStateChangeCallback?.Invoke(_gameplayState, state);
            _gameplayState = state;
        }

        public void RegisterGameplayStateChange(Action<GameplayState, GameplayState> callback)
        {
            _gameplayStateChangeCallback += callback;
        }
        public void UnRegisterGameplayStateChange(Action<GameplayState, GameplayState> callback)
        {
            _gameplayStateChangeCallback -= callback;
        }
    }
    
    public enum GameplayState
    {
        None,
        Tutorial,
        Gaming,
        Pause,
        Finish
    }
}