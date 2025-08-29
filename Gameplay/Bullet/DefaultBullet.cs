using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class DefaultBullet : Bullet
    {
        protected override void OnEnable()
        {
            Destroy(gameObject, 6);
        }

        public override void Init(int damage, BuffAsset asset)
        {
            
        }

        protected override void OnParticleCollision(GameObject other)
        {
            
        }
    }
}