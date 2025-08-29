  using System;
using DG.Tweening;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Gameplay.Script.Gameplay
{
    public class JungleHeart : MonoBehaviour, IInteroperableObject
    {
        [SerializeField] Material shieldMat;
        //[SerializeField] private Renderer renderer;
        [SerializeField] private GameObject shield;
        [SerializeField] string shieldShaderId;
        [SerializeField] private string underAttackShaderId = "_LieHenClip";
        private Transform _targetPoint;
        private int _health;
        private int _fullHealth;
        private GameplayMgr _gameplay;
        private bool _isMoving = false;
        public bool TriggerArea { get; private set; }

        private void Start()
        {
            shield.SetActive(false);
            _gameplay = ProtectPlantsGameplay.Instance;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals("TriggerArea") && !TriggerArea)
                TriggerArea = true;
        }

        public void Init(int jungleHeartHealth, Transform targetPoint)
        {
            shield.SetActive(false);
            //animator.enabled = false;
            shieldMat.SetFloat(underAttackShaderId, -0.1f);
            shield.GetComponent<Renderer>().material.SetFloat(shieldShaderId, -1f);
            //renderer.material.SetFloat("_Clip", 1.1f);
            TriggerArea = false;
            _targetPoint = targetPoint;
            if (Application.isEditor)
                jungleHeartHealth = 100000;
            _health = _fullHealth = jungleHeartHealth;
        }

        public void DoAutoMove(Action callback)
        {
            if (_isMoving) return;
            transform.SetParent(null);
            _isMoving = true;
            transform.DORotate(Vector3.zero, 1.5f).SetEase(Ease.OutCubic);
            transform.DOMove(_targetPoint.position, 2).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                shield.SetActive(true);
                callback?.Invoke();
            });
        }

        public void UnderAttack(int damage, BuffAsset buff, Color? color)
        {
            if (_gameplay.GameplayState != GameplayState.Gaming) return;
            _health -= damage;
            var arg = EventDispatcher.Instance.GetEventArg((int)EventID.UPDATEJUNGLEHEALTH);
            arg.SetArg(0, (float)_health / _fullHealth);
            EventDispatcher.Instance.Dispatch((int)EventID.UPDATEJUNGLEHEALTH);
            float value = Mathf.Clamp(1 - (float)_health / _fullHealth, -0.1f, 1.1f);
            shieldMat.SetFloat(underAttackShaderId, value);
            //renderer.material.SetFloat(underAttackShaderId, value);
            if (_health <= 0)// && GameLevelLogic.Instance.QuickGameMode == QuickGameMode.Normal)
            {
                ((ProtectPlantsGameplay)GameplayMgr.Instance).FinishGame(false);
            }
        }

        public void UnderAttack(string id, int userObjId, int itemCount)
        {
            throw new NotImplementedException();
        }
    }
}