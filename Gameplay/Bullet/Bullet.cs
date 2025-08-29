using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public abstract class Bullet : MonoBehaviour
    {
        [SerializeField] protected GameObject hitEffectPrefab;
        [SerializeField] protected ParticleSystem particleSystem;
        protected int _damage;
        protected BuffAsset _buff;

        protected abstract void OnEnable();

        public abstract void Init(int damage, BuffAsset asset);

        protected abstract void OnParticleCollision(GameObject other);
    }
}