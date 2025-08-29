using Gameplay.Script.Data;
using Gameplay.Script.Element;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class DefaultUserBullet : UserBullet
    {
        [SerializeField] private float delayHit;
        [SerializeField] protected AudioClip clip;
        [SerializeField] protected AudioSource audioSource;
        private ElementItemObject[] _elementItemObjects;
        private UserPlantData _userPlant;
        private ZombieBehaviour _zombieBehaviour;
        private Transform _origin;
        private void Awake()
        {
            _elementItemObjects = GetComponentsInChildren<ElementItemObject>(true);
        }

        
        protected override void OnEnable()
        {
            Destroy(gameObject, 6);
        }

        protected override void OnDisable()
        {
            
        }

        protected override void OnParticleCollision(GameObject other)
        {
            
        }

        public override void Pause()
        {
            
        }

        public override void UnPause()
        {
            
        }

        public void Init(UserPlantData userPlant, Transform bulletOrigin, ZombieBehaviour targetZombie)
        {
            _origin = bulletOrigin;
            _userPlant = userPlant;
            _zombieBehaviour = targetZombie;
            Invoke(nameof(OnHitZombie), delayHit);
        }

        void OnHitZombie()
        {
            if (audioSource && clip)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }

            if (hitEffectPrefab != null)
            {
                var bulletHit = Instantiate(hitEffectPrefab, _origin.position, Quaternion.identity);
                Destroy(bulletHit, 4.5f);
                if (_zombieBehaviour)
                {
                    // _zombieBehaviour.UnderAttack(_userPlant, 0);
                    bulletHit.GetComponent<UserBulletHit>().
                        InitBulletHit(_zombieBehaviour, _userPlant, 0);
                }
            }
        }
    }
}