using System;
using System.Linq;
using Gameplay.Script.Data;
using Gameplay.Script.Element;
using Gameplay.Script.MiniWorldGameplay;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class PlantUserBullet : UserBullet
    {
        [SerializeField] protected PlantUserBullet[] items;
        [SerializeField] protected AudioClip clip;
        [SerializeField] protected AudioSource audioSource;
        private ElementItemObject[] _elementItemObjects;
        // protected string _id;
        protected int _itemCount;
        // private int _userObjId = -1;
        private UserPlantData _userPlant;
        private void Awake()
        {
            _elementItemObjects = GetComponentsInChildren<ElementItemObject>(true);
        }

        protected override void OnEnable()
        {
            if (!particleSystem)
                particleSystem = GetComponent<ParticleSystem>();
            Destroy(gameObject, 6);
            if (!audioSource)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f;
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.minDistance = 1f;
                audioSource.maxDistance = 6f;
            }
            if (clip)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }

            if (SceneLoadManager.Instance.GetActiveSceneIndex() 
                != (int)SceneLoadManager.SceneIndex.MRScene)
                BulletSystem.Instance.Add(this);
        }

        protected override void OnDisable()
        {
            if (SceneLoadManager.Instance.GetActiveSceneIndex()
                != (int)SceneLoadManager.SceneIndex.MRScene)
                BulletSystem.Instance.Remove(this);
        }

        // public void Init(string id, int userObjId)
        // {
        //     _id = id;
        //     _itemCount = 0;
        //     _userObjId = userObjId;
        //     if (items is { Length: > 0 })
        //     {
        //         items.ToList().ForEach(value => value.Init(_id, userObjId, items.Length));
        //     }
        // }

        public void Init(UserPlantData userPlant)
        {
            // _id = id;
            _itemCount = 0;
            _userPlant = userPlant;
            if (items is { Length: > 0 })
            {
                items.ToList().ForEach(value => value.Init(userPlant, items.Length));
            }
            else
            {
                Init(_userPlant, _itemCount);
            }
        }
        
        void Init(UserPlantData userPlant, int itemCount)
        {
            _userPlant = userPlant;
            _itemCount = itemCount;
            ApplyElementEffect(userPlant.elementType);
        }

        protected int _damage;
        protected BuffAsset _buff;
        protected Color? _color;
        
        public void Init(int damage, BuffAsset buff, Color? color)
        {
            _damage = damage;
            _buff = buff;
            _color = color;
            var list = GetComponentsInChildren<PlantUserBullet>().ToList();
            for (int i = 1; i < list.Count; i++) 
            {
                list[i].Init(_damage / (list.Count - 1), _buff, _color);
            }
        }
        
        void ApplyElementEffect(UserElement element)
        {
            if (element != null)
            {
                var elementId = element.elementId;
                var elementObject = _elementItemObjects?.ToList().
                    FirstOrDefault(value => value.ElementId == elementId);
                if (elementObject != null)
                    elementObject.gameObject.SetActive(true);
                else
                    Debug.LogWarning($"No ElementItemObject found for ElementType: {elementId}");
            }
        }

        protected override void OnParticleCollision(GameObject other)
        {
            if (other.TryGetComponent(out IInteroperableObject obj))
            {
                // if (obj is ZombieBehaviour behaviour)
                // {
                //     behaviour.UnderAttack(_userPlant, _itemCount);
                // }
            }
            int maxCollisions = 30;
            ParticleCollisionEvent[] collisionEvents = new ParticleCollisionEvent[maxCollisions];
            int numCollisions = particleSystem.GetCollisionEvents(other, collisionEvents);
            if (numCollisions > 0)
            {
                GameObject bulletHit = null;
                if (hitEffectPrefab)
                {
                    bulletHit = Instantiate(hitEffectPrefab, collisionEvents[0].intersection, Quaternion.identity);
                    Destroy(bulletHit, 3);
                }
                if (obj is ZombieBehaviour behaviour)
                {
                    if (_buff && _buff.plantType == BuffType.僵尸聚集)
                    {
                        var gather = new GameObject("Gather").AddComponent<GatherBuff>();
                        gather.Init(_buff, _damage, _color);
                        gather.transform.position = other.transform.position;
                        var pos = bulletHit.transform.position;
                        pos.y = collisionEvents[0].intersection.y;
                        bulletHit.transform.position = pos + new Vector3(0, 0, 0.335f);
                        return;
                    }
                    if (_buff && _buff.plantType == BuffType.僵尸禁锢)
                    {
                        var pos = bulletHit.transform.position;
                        pos.y = collisionEvents[0].intersection.y;
                        bulletHit.transform.position = pos;
                        behaviour.UnderAttack(_damage, _buff, _color);
                        return;
                    }
                    behaviour.UnderAttack(_damage, _buff, _color);
                    Destroy(gameObject, 2);
                }
                //Destroy(Instantiate(hitEffectPrefab, collisionEvents[0].intersection, Quaternion.identity), 2);
            }
            Destroy(gameObject, 2);
        }

        public override void Pause()
        {
            if (particleSystem) particleSystem.Pause(true);
        }

        public override void UnPause()
        {
            if (particleSystem) particleSystem.Play(true);
        }
    }
}