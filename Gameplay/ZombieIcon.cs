using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.Gameplay
{
    public class ZombieIcon : MonoBehaviour
    {
        [SerializeField] private Image icon;
        private Tweener _iconMove;
        private CanvasGroup _canvasGroup;
        private Camera _mainCamera;
        // private float _originDis;
        private void Start()
        {
            _canvasGroup = GetComponentInChildren<CanvasGroup>();
            _canvasGroup.alpha = 1;
        }
        public void Init(int side, ZombieBehaviour target, float iconMoveDuration)
        {
            _mainCamera = Camera.main;
            Vector3 a = _mainCamera.transform.position;
            Vector3 b = transform.position;
            a.y = b.y = 0;
            // _originDis = Vector3.Distance(a, b);
            icon.transform.localScale = new Vector3(side, 1, 1);
            float t = 0;
            _iconMove = transform.DOMove(target.iconMove2.position, iconMoveDuration)
                .SetEase(Ease.OutCubic).OnUpdate(() =>
                {
                    t += Time.deltaTime;
                    if (target)
                        _iconMove.ChangeEndValue(target.iconMove2.position, iconMoveDuration - t, true); 
                    a = _mainCamera.transform.position;
                    b = transform.position;
                    a.y = b.y = 0;
                    var dis = Vector3.Distance(a, b);
                    // float k = dis / _originDis;
                    var value = dis * target.k;
                    value = Mathf.Clamp(value, 1, target.maxSize);
                    transform.localScale = Vector3.one * value;
                    if (iconMoveDuration - t <= 0)
                    {
                        _iconMove.Kill();
                        if (target)
                            transform.SetParent(target.transform);
                        InvokeRepeating(nameof(Disappear), 1, 0.03f);
                    }
                });
        }

        void Disappear()
        {
            if (_canvasGroup)
            {
                float value = _canvasGroup.alpha;
                value -= 0.03f;
                _canvasGroup.alpha = value;
                if (value <= 0)
                {
                    CancelInvoke(nameof(Disappear));
                    Destroy(gameObject);
                }
            }
        }
    }
}