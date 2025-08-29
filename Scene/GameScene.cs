using System;
using System.Linq;
using BeatNote.Scripts.Manager;
using Gameplay.Script.Manager;
using Pico.Avatar;
using UnityEngine;

namespace Gameplay.Script.Scene
{
    public class GameScene  : BaseScene
    {
        public Transform enemyTargetPoint;
        [SerializeField] Transform enemySpawnPoint;

        public Transform[] EnemySpawnPoints =>
            enemySpawnPoint?.GetComponentsInChildren<Transform>().Skip(1).ToArray() ?? Array.Empty<Transform>();

        public override void OnLoadScene()
        {
            AudioMgr.Instance.PlayBGM("GamingBGM");
        }

        public override void LoadAvatar()
        {
            RoleManager.Instance.LoadAvatar(true, null, DeviceInputReaderBuilderInputType.PicoXR);
        }

        public override void OnUnloadScene()
        {
            
        }
    }
}