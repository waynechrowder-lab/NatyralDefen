using System;
using System.Collections;
using System.Linq;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using Gameplay.Script.MiniWorldGameplay;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Gameplay.Script.Gameplay
{
    public class PlantItem : MonoBehaviour
    {
        [SerializeField] protected Transform parent;
        [SerializeField] protected Renderer coolingRenderer;
        public Transform Parent => parent;
        [SerializeField] protected TMP_Text cost;

        protected bool _canCreate;
        private float _timer;
        public bool CanCreate => _canCreate;
        public bool CanCost => GameplayLogic.Instance.CanCost(_plantLevelData.cost);
        protected Action<int> _onCreate;
        protected GameObject _plant;
        [SerializeField] private PlantInherentData _plantInherentData;
        [SerializeField] protected PlantLevelData _plantLevelData;
        public UserPlantData UserPlant { get; protected set; }

        private void Awake()
        {
            coolingRenderer.material = new Material(coolingRenderer.material);
            EventDispatcher.Instance.Register((int)EventID.CREATEPLANT, OnCreatePlant);
        }

        private void OnEnable()
        {
            if (!_canCreate && _plant)
            {
                StartCoroutine(nameof(ContinueCountDown));
            }
        }

        private void OnDestroy()
        {
            EventDispatcher.Instance.UnRegister((int)EventID.CREATEPLANT, OnCreatePlant);
        }

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

        private void OnCreatePlant(GameEventArg arg)
        {
            var plant = arg.GetArg<UserPlantData>(0);
            if (plant == null || UserPlant == null)
            {
                return;
            }
            if (plant.userPlantId == UserPlant.userPlantId)
            {
                _onCreate?.Invoke(_plantLevelData.cost);
                StartCoroutine(nameof(CountDown));
            }
        }
        
        public void InitItem(int userPlantId, Action<int> onCreate)
        {
            if (_plant)
                DestroyImmediate(_plant);
#if UNITY_EDITOR
            PlantCreatorSystem creatorSystem = FindObjectOfType<PlantCreatorSystem>();
            creatorSystem.PlantSelectItems.Add(this);
#endif
            UserPlant = PlantUpgradeLogic.Instance.GetUserPlantData(userPlantId);
            if (UserPlant == null) return;
            _plantInherentData = PlantData.Instance.GetPlantInherentData(UserPlant.plantId);
            if (_plantInherentData == null) return;
            int plantLevel = UserPlant.plantLevel;
            Debug.Log($"userPlantId:{userPlantId},plantLevel:{plantLevel}");
            _plantLevelData = PlantData.Instance.GetPlantLevelData(UserPlant.plantId, plantLevel);
            if (SceneLoadManager.Instance.GetActiveSceneIndex()
                == (int)SceneLoadManager.SceneIndex.MRScene)
            {
                var asset = GameResourcesMgr.Instance.PlantAssets.FirstOrDefault(value
                    => value.plantName == UserPlant.plantId);
                _plantLevelData = new PlantLevelData()
                {
                    attackInterval = asset.attackInterval,
                    attackValue = asset.attackValue,
                    attackRange = asset.attackRange,
                    coolingTime = asset.coolingTime,
                    cost = asset.cost,
                    health = asset.health
                };
            }
            coolingRenderer.material.SetFloat("_Cooling", 1);
            _onCreate = onCreate;
            _canCreate = true;
            var res = Resources.Load<GameObject>($"Plant/Preview/{UserPlant.plantId}");
            if (!res)
            {
                Debug.LogError($"file doesnt exit : {UserPlant.plantId}");
                return;
            }
            _plant = Instantiate(res, parent).gameObject;
            _plant.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            _plant.GetComponent<PlantBehaviour>().SetAsPreview();
            var renderers = _plant.GetComponentsInChildren<Renderer>().ToList();
            renderers.ForEach(value => value.shadowCastingMode = ShadowCastingMode.Off);

            cost.text = _plantLevelData.cost.ToString();
        }
        IEnumerator CountDown()
        {
            _canCreate = false;
            coolingRenderer.material.SetFloat("_Cooling", 0);
            _timer = _plantLevelData.coolingTime;
            while (_timer > 0)
            {
                _timer -= .033f;
                coolingRenderer.material.SetFloat("_Cooling", 1 - _timer / _plantLevelData.coolingTime);
                yield return new WaitForSeconds(.033f);
            }
            coolingRenderer.material.SetFloat("_Cooling", 1);
            _canCreate = true;
        }
        
        IEnumerator ContinueCountDown()
        {
            while (_timer > 0)
            {
                _timer -= .033f;
                coolingRenderer.material.SetFloat("_Cooling", 1 - _timer / _plantLevelData.coolingTime);
                yield return new WaitForSeconds(.033f);
            }
            coolingRenderer.material.SetFloat("_Cooling", 1);
            _canCreate = true;
        }
    }
}