using System;
using DG.Tweening;
using Gameplay.Script.MiniWorldGameplay;
using UnityEngine;
namespace Gameplay.Script.Gameplay
{
    public class IntensifyConsumable : ConsumableMono
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Transform bulletOrigin;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private IntensifyData intensifyData;
        private bool _isShoot;
        private void Update()
        {
            if (!lineRenderer)
                return;
            Vector3 origin = bulletOrigin.position;
            Vector3 target = origin + bulletOrigin.forward * 2;
            Ray ray = new Ray(bulletOrigin.position, bulletOrigin.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, 1 << LayerMask.NameToLayer("Plant")))
            {
                target = hit.point;
                var go = hit.collider.gameObject;
                if (go && go.TryGetComponent(out PlantBehaviour plant))
                {
                    plant.ShowUpgradeUI();
                }
            }

            lineRenderer.SetPositions(new[] { origin, target });
            lineRenderer.startWidth = lineRenderer.endWidth = 0.0065f;
        }

        protected override void OnPerformed()
        {
            base.OnPerformed();
            if (_isShoot) return;
            Ray ray = new Ray(bulletOrigin.position, bulletOrigin.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, 1 << LayerMask.NameToLayer("Plant")))
            {
                var go = hit.collider.gameObject;
                if (go && go.TryGetComponent(out PlantBehaviour plant) && plant.IsAlive)
                {
                    var bullet = Instantiate(bulletPrefab);
                    bullet.transform.SetPositionAndRotation(bulletOrigin.position, bulletOrigin.rotation);
                    bullet.SetActive(true);

                    bullet.transform.DOMove(hit.point, 2f).SetSpeedBased().SetEase(Ease.InOutQuad).OnComplete(() =>
                    {
                        if (go && plant.IsAlive)
                        {
                            plant.Upgrade(intensifyData);
                        }
                        Destroy(bullet);
                    });
                }

                _isShoot = true;
                Destroy(gameObject, .5f);
            }
        }

        protected override void OnBPerformed()
        {
            base.OnBPerformed();
            if (_isShoot) return;
            GameplayLogic.Instance.CoinChanged(500);
            Destroy(gameObject);
        }
    }

    [System.Serializable]
    public class IntensifyData
    {
        public float healthK = 1.2f;
        public float attackRangeK = 1f;
        public float attackValueK = 1.5f;
        public float attackInterval = 0.9f;
    }
}