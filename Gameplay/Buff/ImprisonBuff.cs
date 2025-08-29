using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class ImprisonBuff : MonoBehaviour
    {
        private BuffAsset _buff;
        int _damage;
        Color? _color;
        public void Init(BuffAsset buff, int damage, Color? color)
        {
            _buff = buff;
            _damage = damage;
            _color = color;
        }
    }
}