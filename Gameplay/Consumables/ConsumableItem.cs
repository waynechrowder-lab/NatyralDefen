using System;
using System.Linq;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gameplay.Script.Gameplay
{
    public class ConsumableItem : PlantItem
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlantCreatorSystem creatorSystem))
            {
                creatorSystem.PlantSelectItems.Add(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlantCreatorSystem creatorSystem))
            {
                creatorSystem.PlantSelectItems.Remove(this);
            }
        }
        
        public void InitItem(string weaponId, Action<int> onCreate)
        {
            if (_plant)
                DestroyImmediate(_plant);
#if UNITY_EDITOR
            PlantCreatorSystem creatorSystem = FindObjectOfType<PlantCreatorSystem>();
            creatorSystem.PlantSelectItems.Add(this);
#endif
            coolingRenderer.material.SetFloat("_Cooling", 1);
            _onCreate = onCreate;
            _canCreate = true;
            UserPlant = new UserPlantData()
            {
                userPlantId = -1,
                plantId = weaponId,
            };
            _plantLevelData = new PlantLevelData()
            {
                cost = 500
            };
            var res = Resources.Load<GameObject>($"Consumable/Preview/{weaponId}");
            if (!res)
            {
                Debug.LogError($"file doesnt exit : {weaponId}");
                return;
            }
            _plant = Instantiate(res, parent).gameObject;
            _plant.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            
            var renderers = _plant.GetComponentsInChildren<Renderer>().ToList();
            renderers.ForEach(value => value.shadowCastingMode = ShadowCastingMode.Off);

            cost.text = _plantLevelData.cost.ToString();
        }
    }
}