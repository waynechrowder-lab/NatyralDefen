using System;
using DG.Tweening;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public abstract class UserBullet : MonoBehaviour
    {
        [SerializeField] protected GameObject hitEffectPrefab;
        [SerializeField] protected ParticleSystem particleSystem;

        protected abstract void OnEnable();
        
        protected abstract void OnDisable();
        
        protected abstract void OnParticleCollision(GameObject other);

        public abstract void Pause();

        public abstract void UnPause();

        public void OnDestroy()
        {
            DOTween.Kill(transform);
        }
    }
}