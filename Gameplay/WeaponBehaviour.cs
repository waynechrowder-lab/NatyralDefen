using Gameplay.Script.Data;
using Gameplay.Scripts.Manager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Gameplay.Script.Gameplay
{
    public class WeaponBehaviour : MonoBehaviour
    {
        [FormerlySerializedAs("bullet")] [SerializeField] private UserBullet userBullet;
        [SerializeField] private Transform bulletOrigin;
        [SerializeField] private float interval = 0.2f;
        
        bool _triggerPerformed = false;
        private float _timer = 0;
        void Start()
        {
            GameInputMgr.Instance.RegisterRightTrigger(OnTriggerPerformed, OnTriggerCanceled);
        }

        private void OnDestroy()
        {
            if (GameInputMgr.Instance)
            {
                GameInputMgr.Instance.UnRegisterRightTrigger(OnTriggerPerformed, OnTriggerCanceled);
            }

        }

        private void OnTriggerCanceled(InputAction.CallbackContext obj)
        {
            _triggerPerformed = false;
        }

        private void OnTriggerPerformed(InputAction.CallbackContext obj)
        {
            _triggerPerformed = true;
        }

        private void Update()
        {
            if (_triggerPerformed)
            {
                _timer += Time.deltaTime;
                if (_timer > interval)
                {
                    _timer = 0;
                    var obj = Instantiate(this.userBullet);
                    obj.transform.SetPositionAndRotation(bulletOrigin.position, Quaternion.LookRotation(transform.forward));
                    UserPlantData data = new UserPlantData("Weapon01", 0);
                    (obj as WeaponUserBullet)?.Init(data);
                }
            }
        }
    }
}