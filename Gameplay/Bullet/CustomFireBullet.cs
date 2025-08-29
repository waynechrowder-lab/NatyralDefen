using System;
using System.Linq;
using Gameplay.Script.Data;
using Gameplay.Script.Element;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class CustomFireBullet : UserBullet
    {
        private ElementItemObject[] _elementItemObjects;
        private UserPlantData _userPlant;
        private ZombieBehaviour _zombieBehaviour;
        private Transform _origin;
        private Hovl_Laser _laser;

        private void Awake()
        {
            _elementItemObjects = GetComponentsInChildren<ElementItemObject>(true);
            _laser = GetComponentInChildren<Hovl_Laser>();
        }

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }

        protected override void OnParticleCollision(GameObject other)
        {

        }

        public override void Pause()
        {

        }

        public override void UnPause()
        {

        }

        protected int _damage;
        protected BuffAsset _buff;
        protected Color? _color;
        private float _deltaDamage;
        
        public void Init(int damage, BuffAsset buff, Color? color, Transform origin, float destroyTime = 5)
        {
            _damage = damage;
            _buff = buff;
            _color = color;
            var list = GetComponentsInChildren<PlantUserBullet>().ToList();
            for (int i = 1; i < list.Count; i++) 
            {
                list[i].Init(_damage / (list.Count - 1), _buff, _color);
            }
            _origin = origin;
            Destroy(gameObject, destroyTime);
        }

        private void Update()
        {
            if (_origin)
            {
                transform.position = _origin.position;
                transform.rotation = _origin.rotation;
            }

            if (_laser && _laser.HitObject && _laser.HitObject.TryGetComponent(out ZombieBehaviour zombie))
            {
                _deltaDamage += Time.deltaTime * _damage;
                var damageInt = (int)_deltaDamage;
                if (damageInt > 0)
                {
                    _deltaDamage = 0;
                    zombie.UnderAttack(damageInt, _buff, _color);
                }
            }

            if (_laser && _buff && _buff.target && _buff.target.TryGetComponent(out PlantBehaviour plant) && !plant.IsAlive)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out IInteroperableObject obj) &&
                obj is ZombieBehaviour behaviour)
            {
                behaviour.UnderAttack(_damage, _buff, _color);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.TryGetComponent(out IInteroperableObject obj) &&
                obj is ZombieBehaviour behaviour)
            {
                behaviour.UnderAttack(_damage, _buff, _color);
            }
        }
    }
}