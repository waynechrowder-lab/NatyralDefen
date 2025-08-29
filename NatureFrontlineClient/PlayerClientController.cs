using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Script.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.Script.NatureFrontlineClient
{
    public class PlayerClientController : MonoBehaviour
    {
        [FormerlySerializedAs("networkClient")] public GameNetworkClientMono networkClientMono;
        public string username = "testuser";
        public string password = "testpass";
        private const string SERVER_IP = "139.196.99.168";
        private const int SERVER_PORT = 11350;
        private GameNetworkClient client;
        private Camera _camera;
        private CancellationTokenSource _cts;
        private async void Start()
        {
            await StartAsync();
            
            //todo:同步位置信息
            return;
            username = UserData.Instance.CurrentUser.relevanceId;
            password = username;
            Debug.Log("连接服务器");
            bool connected = await networkClientMono.ConnectToServer();
            if (!connected) return;
            
            Debug.Log("登录或注册");
            string loginResult = await networkClientMono.Login(username, password);
            Debug.Log($"服务器响应{loginResult}");
            if (!loginResult.StartsWith("success"))
            {
                Debug.Log("登录失败尝试注册");
                string registerResult = await networkClientMono.Register(username, password);
                Debug.Log($"服务器响应{registerResult}");
                if (registerResult.StartsWith("success"))
                    Debug.Log("注册成功");
                else
                    Debug.Log("注册失败");
            }
        }
        
        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            client?.Dispose();
        }
        
        private async UniTask StartAsync()
        {
            _camera = Camera.main;
            client = new GameNetworkClient();
            _cts = new CancellationTokenSource();
            try
            {
                await client.ConnectAsync(SERVER_IP, SERVER_PORT);
                await UniTask.WaitUntil(() => client.ClientConnected, cancellationToken: _cts.Token);
                await client.LoginAsync(username, password);
            }
            catch (Exception ex)
            {
                Debug.LogError($"连接或登录失败: {ex.Message}");
                _cts.Cancel();
            }
            
            await UniTask.WaitUntil(() => client.ConnectedServer, cancellationToken: _cts.Token);
            
            await UniTask.Create(() => UpdatePositionTask(_cts.Token));
        }

        private async UniTask UpdatePositionTask(CancellationToken cancellationToken = default)
        {
            while (true)
            {
                await UniTask.SwitchToMainThread();

                float x = _camera.transform.position.x;
                float y = _camera.transform.position.y;
                float z = _camera.transform.position.z;
                client.UpdatePosition(x, y, z);
                
                await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: cancellationToken);
            }
        }

        private void Update()
        {
            networkClientMono.currentPosition = transform.position;
            foreach (var player in networkClientMono.otherPlayers)
            {

            }
        }
    
        private void OnApplicationQuit()
        {
            networkClientMono.Disconnect();
        }
        
    }
}