using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.Gameplay
{
    public class PlantUI : MonoBehaviour
    {
        [SerializeField] Slider healthSlider;
        [SerializeField] private Transform damageOrigin;
        [SerializeField] private TextMeshPro damageText;
        [SerializeField] private bool showDamageText;
        int _health, _currentHealth;
        
        public void Init(int health)
        {
            healthSlider = GetComponentInChildren<Slider>();
            healthSlider.value = 1;
            _health = _currentHealth = health;
        }

        public void UpdateUI(int damage, int health)
        {
            if (_currentHealth == 0) return;
            _currentHealth = health;
            healthSlider.value = (float)_currentHealth / _health;
            if (!showDamageText) return;
            var obj = Instantiate(damageText);
            obj.text = damage.ToString();
            var pos = damageOrigin.position;
            var rot = damageOrigin.rotation;
            obj.transform.SetPositionAndRotation(pos, rot);
            obj.gameObject.SetActive(true);
            float y = obj.transform.position.y;
            obj.transform.DOMoveY(y + 3, 2).OnComplete(() =>
            {
                Destroy(obj.gameObject);
            });
        }
    }
}