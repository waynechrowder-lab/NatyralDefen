using System.Linq;
using Gameplay.Script.Manager;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class PlantBullet : Bullet
    {
        [SerializeField] private AudioClip clip;
        [SerializeField] private AudioSource audioSource;
        private Color? _color;
        protected override void OnEnable()
        {
            if (!particleSystem)
                particleSystem = GetComponent<ParticleSystem>();
            Destroy(gameObject, 6);
            audioSource.clip = clip;
            audioSource.Play();

            // var bulletParent = GetComponentInParent<PlantBullet>();
            // if (bulletParent)
            // {
            //     Init(bulletParent.Damage, bulletParent.Buff, bulletParent.Color);
            // }
            // AudioMgr.Instance.PlaySoundOneShot(clip);
        }

        public override void Init(int damage, BuffAsset buff)
        {
            _damage = damage;
            _buff = buff;
        }
        
        public void Init(int damage, BuffAsset buff, Color? color)
        {
            _damage = damage;
            _buff = buff;
            _color = color;
            var list = GetComponentsInChildren<PlantBullet>().ToList();
            for (int i = 1; i < list.Count; i++) 
            {
                list[i].Init(_damage / (list.Count - 1), _buff, _color);
            }
        }

        protected override void OnParticleCollision(GameObject other)
        {
            if (other.TryGetComponent(out IInteroperableObject obj))
            {
                if (obj is ZombieBehaviour behaviour)
                {
                    behaviour.UnderAttack(_damage, _buff, _color);
                }
            }
            int maxCollisions = 30;
            ParticleCollisionEvent[] collisionEvents = new ParticleCollisionEvent[maxCollisions];
            // var particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
            // int numParticles = particleSystem.GetParticles(particles);
            // Debug.Log(numParticles);
            int numCollisions = particleSystem.GetCollisionEvents(other, collisionEvents);
            if (numCollisions > 0)
            {
                Destroy(Instantiate(hitEffectPrefab, collisionEvents[0].intersection, Quaternion.identity), 2);
            }
            Destroy(gameObject);
        }
    }
}