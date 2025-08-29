using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Gameplay.Script.Manager;
using Gameplay.Script.MiniWorldGameplay;
using UnityEngine;
using UnityEngine.Animations;
using Random = UnityEngine.Random;

namespace Gameplay.Script.Gameplay
{
    public class CoinBullet : Bullet
    {
        [SerializeField] private AudioClip collect;
        [SerializeField] private float absorptionRange = 1;
        [SerializeField] private int coinValue;
        [SerializeField] private float disappearDelayTime = 8;
        [SerializeField] private float disappearDuration = 1;
        [Header("能量设置")]
        [SerializeField] float minSpeed = 5f;
        [SerializeField] float maxSpeed = 10f;
        [SerializeField] float minAngle = 50f;
        [SerializeField] float maxAngle = 70f;
        
        [Header("落地后设置")]
        public float checkDistance = 0.1f;
        public float alignmentForce = 5f;
        public LayerMask groundLayer;

        private List<Transform> _transforms;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Transform _leftHand;
        [SerializeField] private Transform _rightHand;
        private bool hasLanded = false;
        private float _delayTime = 1;
        private bool pickUp = false;
        private Tweener pickUpMove;
        protected override void OnEnable()
        {
            _transforms = gameObject.GetComponentsInChildren<Transform>().ToList();
            _mainCamera = Camera.main;
            _leftHand = _mainCamera.transform.parent.Find("Left Controller");
            _rightHand = _mainCamera.transform.parent.Find("Right Controller");
            var lookAt = GetComponentInChildren<LookAtConstraint>();
            lookAt.AddSource(new ConstraintSource()
            {
                sourceTransform = _mainCamera.transform,
                weight = 1
            });
            Invoke(nameof(Disappear), disappearDelayTime);
            Destroy(gameObject, disappearDelayTime + disappearDuration);
        }
        
        void FixedUpdate()
        {
            _delayTime -= Time.fixedDeltaTime;
            if (pickUp) return;
            if (!hasLanded && IsGrounded() && _delayTime < 0)
            {
                hasLanded = true;
                // if (gameObject.TryGetComponent(out Rigidbody rig))
                //     rig.isKinematic = true;
                // Debug.Log("ground");
            }
        }
        
        bool IsGrounded()
        {
            return Physics.Raycast(transform.position, Vector3.down, checkDistance, groundLayer);
        }

        void Disappear()
        {
            if (pickUp) return;
            _transforms.ForEach(value => value.DOScale(0, disappearDuration));
        }

        public void DoMoveAsGravity()
        {
            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = false;
            rigidbody.drag = .5f;
        }
        
        public void DoMoveAsParabola()
        {
            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = false;
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            Vector3 horizontalDirection = new Vector3(randomCircle.x, 0, randomCircle.y);
            
            float speed = Random.Range(minSpeed, maxSpeed);
            float angle = Random.Range(minAngle, maxAngle);
            
            float angleInRad = angle * Mathf.Deg2Rad;
            Vector3 initialVelocity = horizontalDirection * speed * Mathf.Cos(angleInRad);
            initialVelocity.y = speed * Mathf.Sin(angleInRad);
            rigidbody.drag = 4f;
            rigidbody.velocity = initialVelocity;
        }

        public override void Init(int damage, BuffAsset asset)
        {
            
        }

        protected override void OnParticleCollision(GameObject other)
        {
            
        }

        private void Update()
        {
            if (!hasLanded) return;
            if (pickUp) return;
            var pos1 = _mainCamera.transform.position;
            var pos2 = transform.position;
            pos2.y = pos1.y = 0;
            if (Vector3.Distance(pos1, pos2) < absorptionRange)
            {
                StartCoroutine(nameof(PickUp));
                return;
            }

            if (_leftHand)
            {
                pos1 = _leftHand.position;
                pos2.y = pos1.y = 0;
                if (Vector3.Distance(pos1, pos2) < absorptionRange)
                {
                    StartCoroutine(nameof(PickUp));
                    return;
                }
            }

            if (_rightHand)
            {
                pos1 = _rightHand.position;
                pos2.y = pos1.y = 0;
                if (Vector3.Distance(pos1, pos2) < absorptionRange)
                {
                    StartCoroutine(nameof(PickUp));
                    return;
                }
            }
        }

        IEnumerator PickUp()
        {
            AudioMgr.Instance.PlaySoundOneShot(collect, 1, Random.Range(0.85f, 1.15f));
            pickUp = true;
            _transforms.ForEach(value => value.DOScale(0, 0.3f).SetDelay(0.5f));
            var targetTrans = ((ProtectPlantsGameplay)ProtectPlantsGameplay.Instance).CoinMove2;
            float t = 0;
            pickUpMove = transform.DOMove(targetTrans.position, .8f)
                .SetEase(Ease.OutCubic).OnUpdate(() =>
                {
                    t += Time.deltaTime;
                    pickUpMove.ChangeEndValue(targetTrans.position, .8f - t, true);
                });
            if (ProtectPlantsGameplay.Instance.GameplayState != GameplayState.Gaming) yield break;
            yield return new WaitForSeconds(.8f);
            GameEventArg arg = EventDispatcher.Instance.GetEventArg((int)EventID.CREATECOIN);
            arg.SetArg(0, coinValue);
            EventDispatcher.Instance.Dispatch((int)EventID.CREATECOIN);
            GameplayLogic.Instance.CoinChanged(coinValue);
            Destroy(gameObject);
        }
    }
}