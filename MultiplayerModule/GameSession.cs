using Unity.Netcode;
using System.Collections;
using UnityEngine;

namespace Gameplay.Script.MultiplayerModule
{
    public class GameSession : NetworkBehaviour
    {
        private NetworkVariable<int> _ready2Game;
        private MultiplayerGameplayMgr _mgr;

        void Awake()
        {
            _ready2Game = new NetworkVariable<int>(0);
            _ready2Game.OnValueChanged = OnReady2GameValueChanged;
            _mgr = GetComponent<MultiplayerGameplayMgr>();
        }

        private void OnReady2GameValueChanged(int previousValue, int newValue)
        {
            Debug.Log($"ready count down {newValue}");
        }

        #region NetworkBehaviour

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                _ready2Game = new NetworkVariable<int>(3);
                StartCoroutine(nameof(Ready2Start));
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }

        #endregion


        IEnumerator Ready2Start()
        {
            while (_ready2Game.Value > 0)
            {
                yield return new WaitForSeconds(1);
                _ready2Game.Value -= 1;
            }
            StartGame_ClientRpc();
        }

        [ClientRpc]
        void StartGame_ClientRpc()
        {
            _mgr.StartGamePlay();
        }

        [ServerRpc]
        public void BackToLobby_ServerRpc()
        {
            SceneLoadManager.Instance.OnLoadScene(SceneLoadManager.SceneIndex.Lobby);
            BackToLobby_ClientRpc();
        }

        [ClientRpc]
        void BackToLobby_ClientRpc()
        {
            if (!IsHost)
                SceneLoadManager.Instance.OnLoadScene(SceneLoadManager.SceneIndex.Lobby);
        }
    }
}