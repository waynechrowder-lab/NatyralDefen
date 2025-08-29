using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    [System.Serializable]
    public class CharacterContext
    {
        public GameObject Host;
        public Transform Transform;
        public int Health;

        public void DoDamage(int damage)
        {
            Health -= damage;
        }
    }
}
