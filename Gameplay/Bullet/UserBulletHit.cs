using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Gameplay.Script.Data;
using Gameplay.Script.Element;
using Gameplay.Script.MiniWorldGameplay;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class UserBulletHit : MonoBehaviour
    {
        [SerializeField] private GameObject electricityObj;
        private ElementItemObject[] _elementItemObjects;
        private UserPlantData _plantData;
        private int _bulletCount;
        private ZombieBehaviour _originZombie;
        private void Awake()
        {
            _elementItemObjects = GetComponentsInChildren<ElementItemObject>(true);
        }

        public void InitBulletHit(ZombieBehaviour originZombie, UserPlantData plantData, int bulletCount)
        {
            _originZombie = originZombie;
            _plantData = plantData;
            _bulletCount = bulletCount;
            ApplyElementEffect();
            SetElementEffect();
        }

        void ApplyElementEffect()
        {
            var element = _plantData.elementType;
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
        
        void SetElementEffect()
        {
            var element = _plantData.elementType;
            if (element != null)
            {
                switch (element.elementId)
                {
                    case "Element_01":
                        _ = FindFireRangeZombies();
                        break;
                    case "Element_02":
                        _ = FindElectricityRangeZombies();
                        break;
                    default:
                        _originZombie.UnderAttack(true, _plantData, _bulletCount);
                        break;
                }
            }
        }
        
        List<ZombieBehaviour> FindFireRangeZombies()
        {
            var element = _plantData.elementType;
            var inherentData = ElementData.Instance.GetElementInherentData(element.elementId);
            var levelData = ElementData.Instance.GetElementLevelData(inherentData.id, element.elementLevel);
            float distance = levelData.range;
            var newlist = new List<ZombieBehaviour>();
            var list = EnemyManager.ZombieBehaviours;
            for (int i = 0; i < list.Count; i++)
            {
                var zombie = list[i];
                if (zombie != null)
                {
                    Vector3 pos1 = zombie.transform.position;
                    Vector3 pos2 = transform.position;
                    pos1.y = pos2.y = 0;
                    if (Vector3.Distance(pos1, pos2) <= distance 
                        && zombie.ZombieState < ZombieState.Dead)
                    {
                        newlist.Add(zombie);
                        zombie.UnderAttack(_originZombie == zombie, _plantData, _bulletCount);
                    }
                }
            }
            return newlist;
        }
        
        List<ZombieBehaviour> FindElectricityRangeZombies()
        {
            var element = _plantData.elementType;
            var inherentData = ElementData.Instance.GetElementInherentData(element.elementId);
            var levelData = ElementData.Instance.GetElementLevelData(inherentData.id, element.elementLevel);
            float distance = levelData.range;
            int count = (int)levelData.count;
            var newlist = new List<ZombieBehaviour>();
            var list = EnemyManager.ZombieBehaviours;
            for (int i = 0; i < list.Count; i++)
            {
                var zombie = list[i];
                if (zombie != null)
                {
                    Vector3 pos1 = zombie.transform.position;
                    Vector3 pos2 = transform.position;
                    pos1.y = pos2.y = 0;
                    
                    //todo:过滤角度过大的僵尸
                    if (Vector3.Distance(pos1, pos2) <= distance 
                        && zombie.ZombieState < ZombieState.Dead)
                    {
                        newlist.Add(zombie);
                        zombie.UnderAttack(_originZombie == zombie, _plantData, _bulletCount);
                        if (newlist.Count >= count) break;
                    }
                }
            }

            newlist = newlist.OrderBy(zombie =>
            {
                Vector3 pos1 = zombie.transform.position;
                Vector3 pos2 = transform.position;
                pos1.y = pos2.y = 0;
                return Vector3.Distance(pos1, pos2);
            }).ToList();

            if (newlist.Count > 1)
            {
                var moveObj = Instantiate(electricityObj);
                var startPos = _originZombie.transform.position;
                startPos.y = transform.position.y;
                moveObj.transform.position = startPos;
                moveObj.SetActive(true);
                Destroy(moveObj, 3);
                Sequence sequence = DOTween.Sequence();
                
                for (int i = 1; i < newlist.Count; i++)
                {
                    Vector3 targetPos = newlist[i].transform.position;
                    targetPos.y = transform.position.y;
                    sequence.Append(moveObj.transform.DOMove(targetPos, 0.3f));
                }
                
                sequence.OnComplete(() =>
                {
                    moveObj.SetActive(false);
                });
                
                sequence.Play();
            }
            
            return newlist;
        }
    }
}