using System;
using System.Collections.Generic;
using Currency.Core.Run;
using Gameplay.Script.Gameplay;
using Gameplay.Script.Manager;

namespace Gameplay.Script.MiniWorldGameplay
{
    public class BulletSystem : MonoSingleton<BulletSystem>
    {
        private List<UserBullet> _bullets = new List<UserBullet>();

        private void Start()
        {
            GameplayMgr.Instance.RegisterGameplayStateChange(OnGameplayStateChanged);
        }

        protected void OnDestroy()
        {
            if (GameplayMgr.Instance)
                GameplayMgr.Instance.UnRegisterGameplayStateChange(OnGameplayStateChanged);
        }

        private void OnGameplayStateChanged(GameplayState last, GameplayState current)
        {
            if (current == GameplayState.Pause)
            {
                _bullets.ForEach(value =>
                {
                    if (value)
                    {
                        value.Pause();
                    }
                });
            }
            else if (current == GameplayState.Gaming)
            {
                _bullets.ForEach(value =>
                {
                    if (value)
                    {
                        value.UnPause();
                    }
                });
            }
        }
        
        public void Add(UserBullet userBullet)
        {
            _bullets.Add(userBullet);
        }
        
        public void Remove(UserBullet userBullet)
        {
            _bullets.Remove(userBullet);
        }
    }
}