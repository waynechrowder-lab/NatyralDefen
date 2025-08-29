using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using Gameplay.Script.MiniWorldGameplay;
using Gameplay.Script.MultiplayerModule;
using Gameplay.Script.UI;
using Gameplay.Scripts.Manager;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Gameplay.Script.Gameplay
{
    public class PlantCreatorSystem : MonoBehaviour
    {
        
        [Header("提示音效")]
        [SerializeField] private AudioClip notEnoughEnergyClip;      // 能量不足音效
        [SerializeField] private AudioClip plantGeneratingClip;      // CD 冷却中音效

        // [Header("冷却提示音效")]
        // [SerializeField] private AudioClip coolingClip;

        // 记录每个 AudioClip 下一次允许播放的时间点
        private Dictionary<AudioClip, float> _nextAllowedPlayTime = new Dictionary<AudioClip, float>();
        
        [SerializeField] private float maxDistance = 3;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] InGameUI inGameUI;
        [SerializeField] private PlantCreatorUI plantCreatorUI;
        [SerializeField] private ConsumableUI consumableUI;
        [SerializeField] private Transform plantSelectorParent;

        [SerializeField] LineRenderer[] lineRenderers;
        [SerializeField] XRRayInteractor[] xRRayInteractors;

        private AudioSource _audioSource;  // 播放音效的 AudioSource
        public List<PlantItem> PlantSelectItems { get; set; } = new();
        private static List<PlantBehaviour> _plantBehaviours = new();
        public static List<PlantBehaviour> PlantBehaviours => _plantBehaviours;
        private PlantBehaviour _currentPlantBehaviour;
        private ConsumableMono _currentConsumable;
        
        private UserPlantData _currentPlant;
        
        public static Transform PlantCreatorSystemTransform { get; private set; } 

        public static void OnNetworkSpawn(PlantBehaviour plant)
        {
            _plantBehaviours.Add(plant);
        }
        
        private void Start()
        {
            PlantCreatorSystemTransform = plantSelectorParent;
            plantCreatorUI.gameObject.SetActive(false);
            consumableUI.gameObject.SetActive(false);
            inGameUI.gameObject.SetActive(false);
            inGameUI.RegisterContinueAction(() =>
            {
                OnBPerformed(new InputAction.CallbackContext());
            });
            xRRayInteractors.ToList().ForEach(value => value.enabled = false);
            lineRenderers.ToList().ForEach(value => value.enabled = false);
            GameInputMgr.Instance.RegisterRightTrigger(OnTriggerPerformed, OnTriggerCanceled);
            GameInputMgr.Instance.RegisterRightBTrigger(OnBPerformed, OnBCanceled);
            // GameInputMgr.Instance.RegisterRightATrigger(OnAPerformed, OnACanceled);
            EventDispatcher.Instance.Register((int)EventID.StartSpawnPlantOnHand, OnStartSpawnPlantOnHand);
            EventDispatcher.Instance.Register((int)EventID.GAMEOVER, OnGameOver);
            
            GameplayMgr.Instance.RegisterGameplayStateChange(OnGameplayStateChanged);
            _audioSource = gameObject.GetComponent<AudioSource>();
            if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.loop = false;
            _audioSource.playOnAwake = false;
        }

        void OnDestroy()
        {
            if (GameInputMgr.Instance)
            {
                GameInputMgr.Instance.UnRegisterRightTrigger(OnTriggerPerformed, OnTriggerCanceled);
                GameInputMgr.Instance.UnRegisterRightBTrigger(OnBPerformed, OnBCanceled);
                // GameInputMgr.Instance.UnRegisterRightATrigger(OnAPerformed, OnACanceled);
            }
            EventDispatcher.Instance.UnRegister((int)EventID.StartSpawnPlantOnHand, OnStartSpawnPlantOnHand);
            EventDispatcher.Instance.UnRegister((int)EventID.GAMEOVER, OnGameOver);
            if (GameplayMgr.Instance)
                GameplayMgr.Instance.UnRegisterGameplayStateChange(OnGameplayStateChanged);
        }

        private void OnBCanceled(InputAction.CallbackContext context)
        {

        }

        private void OnBPerformed(InputAction.CallbackContext context)
        {
            if (GameplayMgr.Instance.GameplayState != GameplayState.Gaming)
                return;
            if (!_currentConsumable)
            {
                if (_currentPlantBehaviour)
                {
                    OnTriggerCanceled(context);
                }
                bool open = inGameUI.gameObject.activeInHierarchy;
                inGameUI.gameObject.SetActive(!open);
                plantCreatorUI.gameObject.SetActive(open);
                consumableUI.gameObject.SetActive(open);
                xRRayInteractors.ToList().ForEach(value => value.enabled = !open);
                lineRenderers.ToList().ForEach(value => value.enabled = !open);
                EventDispatcher.Instance.Dispatch((int)EventID.ACTIVEINGAMEUI);
            }
        }

        private void Update()
        {
            #if UNITY_EDITOR
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                CreatePlantFast(0);
            }
            if (Keyboard.current.digit1Key.wasReleasedThisFrame)
            {
                ConfirmCreatePlantFast(0);
            }
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                CreatePlantFast(1);
            }
            if (Keyboard.current.digit2Key.wasReleasedThisFrame)
            {
                ConfirmCreatePlantFast(1);
            }
            if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                CreatePlantFast(2);
            }
            if (Keyboard.current.digit3Key.wasReleasedThisFrame)
            {
                ConfirmCreatePlantFast(2);
            }
            if (Keyboard.current.digit4Key.wasPressedThisFrame)
            {
                CreatePlantFast(3);
            }
            if (Keyboard.current.digit4Key.wasReleasedThisFrame)
            {
                ConfirmCreatePlantFast(3);
            }
            if (Keyboard.current.digit5Key.wasPressedThisFrame)
            {
                CreatePlantFast(4);
            }
            if (Keyboard.current.digit5Key.wasReleasedThisFrame)
            {
                ConfirmCreatePlantFast(4);
            }
            if (Keyboard.current.digit6Key.wasPressedThisFrame)
            {
                CreatePlantFast(5);
            }
            if (Keyboard.current.digit6Key.wasReleasedThisFrame)
            {
                ConfirmCreatePlantFast(5);
            }
            #endif
        }

        void CreatePlantFast(int index)
        {
            if (GameplayMgr.Instance.GameplayState != GameplayState.Gaming)
                return;
            if (inGameUI.gameObject.activeInHierarchy)
                return;
            if (_currentConsumable)
                return;
            if (index < PlantSelectItems.Count)
            {
                var item = PlantSelectItems[index];
                var plant = PlantSelectItems[index].UserPlant;
                // if (!PlantSelectItems[index].CanCreate) plant = null;
                if (!item.CanCost)
                {
                    PlayClipOnce(notEnoughEnergyClip);
                    return;
                }
                if (!item.CanCreate)
                {
                    PlayClipOnce(plantGeneratingClip);
                    return;
                }
                SetPlant(plant);
            }
        }

        void ConfirmCreatePlantFast(int index)
        {
            UserPlantData plant = null;
            if (index < PlantSelectItems.Count)
                plant = PlantSelectItems[index].UserPlant;
            if (plant == null)
                return;
            if (_currentPlantBehaviour && plant.userPlantId == _currentPlant.userPlantId)
            {
                _currentPlant = plant;
                _currentPlantBehaviour.transform.SetParent(null);
                _currentPlantBehaviour.OnSpawn(_currentPlant.plantId);
                // _currentPlantBehaviour.OnSpawn(_currentPlant);
                if (!MultiplayerLogic.Instance.IsMultiPlay())
                    _plantBehaviours.Add(_currentPlantBehaviour);
                _currentPlantBehaviour = null;
                
                GameEventArg arg = EventDispatcher.Instance.GetEventArg((int)EventID.CREATEPLANT);
                arg.SetArg(0, plant);
                EventDispatcher.Instance.Dispatch((int)EventID.CREATEPLANT);
            }
        }
        
        private void OnTriggerCanceled(InputAction.CallbackContext obj)
        {
            if (_currentPlantBehaviour)// && _hitGround)
            {
                _currentPlantBehaviour.transform.SetParent(null);
                _currentPlantBehaviour.OnSpawn(_currentPlant.plantId);
                // _currentPlantBehaviour.OnSpawn(_currentPlant);
                if (!MultiplayerLogic.Instance.IsMultiPlay())
                    _plantBehaviours.Add(_currentPlantBehaviour);
                _currentPlantBehaviour = null;
                
                GameEventArg arg = EventDispatcher.Instance.GetEventArg((int)EventID.CREATEPLANT);
                arg.SetArg(0, _currentPlant);
                EventDispatcher.Instance.Dispatch((int)EventID.CREATEPLANT);
            }
        }

        private void OnTriggerPerformed(InputAction.CallbackContext obj)
        {
            if (GameplayMgr.Instance.GameplayState != GameplayState.Gaming)
                return;
            if (inGameUI.gameObject.activeInHierarchy)
                return;
            if (_currentConsumable)
                return;
            
            if (PlantSelectItems.Count > 0)
            {
                float dis = float.MaxValue;
                PlantItem item = null;
                UserPlantData plant = null;
                for (int i = 0; i < PlantSelectItems.Count; i++)
                {
                    // if (!PlantSelectItems[i].CanCreate) continue;
                    float value = Vector3.Distance(transform.position, PlantSelectItems[i].Parent.position);
                    if (value < dis)
                    {
                        dis = value;
                        item = PlantSelectItems[i];
                        plant = PlantSelectItems[i].UserPlant;
                    }
                }

                if (plant == null) return;

                if (!item.CanCost)
                {
                    PlayClipOnce(notEnoughEnergyClip);
                    return;
                }
                if (!item.CanCreate)
                {
                    PlayClipOnce(plantGeneratingClip);
                    return;
                }
                SetPlant(plant);
            }
        }

        private void PlayClipOnce(AudioClip clip)
        {
            float now = Time.time;
            if (_nextAllowedPlayTime.TryGetValue(clip, out var nextTime) && now < nextTime)
                return;

            _audioSource.PlayOneShot(clip);
            // 标记该 clip 播放结束后的时刻，以后才可再次触发
            _nextAllowedPlayTime[clip] = now + clip.length;
        }
        
        private void OnAPerformed(InputAction.CallbackContext obj)
        {
            if (GameplayMgr.Instance.GameplayState != GameplayState.Gaming) return;
            if (_currentPlantBehaviour)
            {
                return;
            }
            if (_currentConsumable)
            {
                DestroyImmediate(_currentConsumable.gameObject);
            }
            else
            {
                var res = Resources.Load<GameObject>($"Weapon/Weapon01");
                if (!res)
                {
                    Debug.LogError($"file doesnt exit : Weapon01");
                    return;
                }
                _currentConsumable = Instantiate(res, plantSelectorParent).GetComponent<ConsumableMono>();
                _currentConsumable.transform.SetLocalPositionAndRotation(Vector3.zero, 
                    Quaternion.Euler(new Vector3(0, 180, 0)));
            }
        }
        
        private void OnACanceled(InputAction.CallbackContext obj)
        {

        }
        
        private void OnStartSpawnPlantOnHand(GameEventArg arg)
        {
            _plantBehaviours.Clear();
            PlantSelectItems.Clear();
            plantCreatorUI.gameObject.SetActive(true);
            consumableUI.gameObject.SetActive(true);
            if (_currentPlantBehaviour) 
                Destroy(_currentPlantBehaviour.gameObject);
            // Debug.Log(string.Join(",", UserDataManager.Instance.GetBagItems()));
            plantCreatorUI.Init(UserDataManager.Instance.GetBagItems(GameLevelLogic.Instance.QuickGameMode));
            var list = new List<string>();
            list.Add("C001");
            consumableUI.Init(list);
            InvokeRepeating(nameof(SetCanvasGroupInteractable), .5f, .5f);
        }

        void SetCanvasGroupInteractable()
        {
            if (GameplayMgr.Instance.GameplayState == GameplayState.Gaming)
            {
                CancelInvoke(nameof(SetCanvasGroupInteractable));
            }
        }

        void OnGameOver(GameEventArg arg)
        {
            inGameUI.gameObject.SetActive(false);
            plantCreatorUI.gameObject.SetActive(false);
            consumableUI.gameObject.SetActive(false);
            if (_currentConsumable)
                Destroy(_currentConsumable.gameObject);
            if (_currentPlantBehaviour) 
                Destroy(_currentPlantBehaviour.gameObject);
        }
        
        private void OnGameplayStateChanged(GameplayState last, GameplayState current)
        {
            if (current == GameplayState.Pause)
            {
                if (_currentConsumable)
                    _currentConsumable.gameObject.SetActive(false);
                if (_currentPlantBehaviour)
                    _currentPlantBehaviour.gameObject.SetActive(false);
                plantCreatorUI?.gameObject.SetActive(false);
                consumableUI?.gameObject.SetActive(false);
                _plantBehaviours.ForEach(value =>
                {
                    if (value)
                    {
                        value.OnGamePause();
                    }
                });
            }
            else if (current == GameplayState.Gaming)
            {
                if (_currentConsumable)
                    _currentConsumable.gameObject.SetActive(true);
                if (_currentPlantBehaviour)
                    _currentPlantBehaviour.gameObject.SetActive(true);
                plantCreatorUI?.gameObject.SetActive(true);
                consumableUI?.gameObject.SetActive(true);
                _plantBehaviours.ForEach(value =>
                {
                    if (value)
                    {
                        value.OnGameUnPause();
                    }
                });
            }
        }

        void SetPlant(UserPlantData plant)
        {
            _currentPlant = plant;
            Debug.Log($"set plant : {_currentPlant.userPlantId}");
            if (!MultiplayerLogic.Instance.IsMultiPlay())
            {
                if (_currentPlant.userPlantId < 0)
                {
                    var res = Resources.Load<GameObject>($"Consumable/{plant.plantId}");
                    if (!res)
                    {
                        Debug.LogError($"file doesnt exit : {plant.plantId}");
                        return;
                    }
                    if (_currentPlantBehaviour) DestroyImmediate(_currentPlantBehaviour.gameObject);
                    _currentConsumable = Instantiate(res, plantSelectorParent).GetComponent<ConsumableMono>();
                    _currentConsumable.transform.SetLocalPositionAndRotation(Vector3.zero, 
                        Quaternion.Euler(new Vector3(0, 180, 0)));
                    GameplayLogic.Instance.CoinChanged(-500);
                    return;
                }

                {
                    var res = Resources.Load<GameObject>($"Plant/{plant.plantId}");
                    if (!res)
                    {
                        Debug.LogError($"file doesnt exit : {plant.plantId}");
                        return;
                    }
                    if (_currentPlantBehaviour) DestroyImmediate(_currentPlantBehaviour.gameObject);
                    _currentPlantBehaviour = Instantiate(res, plantSelectorParent).GetComponent<PlantBehaviour>();
                    _currentPlantBehaviour.SetAsPreview();
                    return;
                }

            }
            var networkObj = Spawner.Instance.SpawnPlant(plant.plantId);
            if (!networkObj)
            {
                Debug.LogError($"file doesnt exit : {plant.plantId}");
                return;
            }
            if (_currentPlantBehaviour) DestroyImmediate(_currentPlantBehaviour.gameObject);
            // networkObj.TrySetParent(plantSelectorParent);
            // networkObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _currentPlantBehaviour = networkObj.GetComponent<PlantBehaviour>();
            _currentPlantBehaviour.SetAsPreview();
        }
    }
}