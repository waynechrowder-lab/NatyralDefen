using Unity.Netcode;

namespace Gameplay.Script.MultiplayerModule
{
    public class LobbySession : NetworkBehaviour
    {
        [ServerRpc]
        public void EnterMultiplayerScene_ServerRpc(int sceneName)
        {
            SceneLoadManager.Instance.OnLoadScene((SceneLoadManager.SceneIndex)sceneName);
            EnterMultiplayerScene_ClientRpc(sceneName);
        }
        
        [ClientRpc]
        void EnterMultiplayerScene_ClientRpc(int sceneName)
        {
            if (!IsHost)
                SceneLoadManager.Instance.OnLoadScene((SceneLoadManager.SceneIndex)sceneName);
        }
    }
}