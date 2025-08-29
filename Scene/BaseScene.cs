using System;
using System.Linq;
using Unity.XR.CoreUtils;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace Gameplay.Script.Scene
{
    public abstract class BaseScene : MonoBehaviour
    {
        [SerializeField] protected XRDeviceSimulator _deviceSimulator;
        public Transform spawnPoint;

        protected virtual void Awake()
        {
            OnLoadScene();
        }

        protected virtual void Start()
        {
#if UNITY_EDITOR
            if (_deviceSimulator)
            {
                _deviceSimulator.gameObject.SetActive(true);
            }
#endif
            LoadAvatar();
        }

        protected virtual void OnDestroy()
        {
            OnUnloadScene();
        }
        
        public abstract void OnLoadScene();

        public abstract void LoadAvatar();

        public abstract void OnUnloadScene();
        
    }
}