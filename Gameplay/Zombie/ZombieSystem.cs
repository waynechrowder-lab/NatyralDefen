using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class ZombieSystem : MonoSingle<ZombieSystem>
    {
        [SerializeField] private ZombieSpawner spawner;
        [SerializeField] private ZombieHUD hud;

        public List<ZombieBehaviour> ZombieBehaviours => spawner ? spawner.ZombieBehaviours : null;
        public int ZombieRemaining => spawner ? spawner.ZombieRemaining : 0;
        public int ZombieWaitTime => spawner ? spawner.ZombieWaitTime : 0;

        private void Start()
        {
            if (spawner == null)
                spawner = GetComponent<ZombieSpawner>();
            if (hud == null)
                hud = GetComponent<ZombieHUD>();
            if (hud != null && spawner != null)
            {
                hud.SetZombieList(spawner.ZombieBehaviours);
                spawner.OnZombieSpawned += hud.OnZombieSpawned;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (spawner != null && hud != null)
                spawner.OnZombieSpawned -= hud.OnZombieSpawned;
        }
    }
}
