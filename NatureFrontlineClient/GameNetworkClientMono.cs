using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Gameplay.Script.Data;
using UnityEngine;

namespace Gameplay.Script.NatureFrontlineClient
{
    public class GameNetworkClientMono : MonoBehaviour
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private bool _isConnected = false;
        private string _username;

        // 玩家位置同步相关
        public Vector3 currentPosition;

        public ConcurrentDictionary<string, PlayerPosition> otherPlayers =
            new ConcurrentDictionary<string, PlayerPosition>();

        // 服务器配置
        private const string SERVER_IP = "139.196.99.168";
        private const int SERVER_PORT = 11350;

        // 位置更新频率
        private float _positionUpdateInterval = 0.1f; // 每秒10次
        private float _positionUpdateTimer = 0f;

        // 位置获取频率
        private float _positionsRequestInterval = 0.5f; // 每秒2次
        private float _positionsRequestTimer = 0f;

        private void Start()
        {
            _username = UserData.Instance.CurrentUser.relevanceId;
            // 初始化玩家位置
            currentPosition = new Vector3(0, 0, 0);
        }

        private void Update()
        {
            if (!_isConnected) return;

            if (_isConnected && (_client == null || !_client.Connected))
            {
                Debug.Log("Detected disconnected state");
                Disconnect();
            }
            
            // 定时发送位置更新
            _positionUpdateTimer += Time.deltaTime;
            if (_positionUpdateTimer >= _positionUpdateInterval)
            {
                _positionUpdateTimer = 0f;
                SendPositionUpdate();
            }

            // 定时获取其他玩家位置
            _positionsRequestTimer += Time.deltaTime;
            if (_positionsRequestTimer >= _positionsRequestInterval)
            {
                _positionsRequestTimer = 0f;
                RequestAllPlayerPositions();
            }
        }

        private void OnDestroy()
        {
            if (_isConnected && _client is not { Connected: true })
                SendLogout();
            Disconnect();
        }

        // 连接到服务器
        public async Task<bool> ConnectToServer()
        {
            try
            {
                _client = new TcpClient();
                _client.NoDelay = true; // 禁用Nagle算法
                await _client.ConnectAsync(SERVER_IP, SERVER_PORT);
                _stream = _client.GetStream();
                _stream.ReadTimeout = 5000; // 5秒读取超时
                _stream.WriteTimeout = 5000; // 5秒写入超时

                // 启动接收消息任务
                _ = Task.Run(ReceiveMessages);
                _isConnected = true;
                Debug.Log("Connected to server");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Connection error: {e.Message}");
                return false;
            }
        }

        // 断开连接
        public void Disconnect()
        {
            if (!_isConnected) return;
    
            _isConnected = false;
    
            try
            {
                _stream?.Close();
                _client?.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error during disconnect: {ex.Message}");
            }
            finally
            {
                _stream = null;
                _client = null;
            }
    
            Debug.Log("Disconnected from server");
        }

        // 注册账号
        public async Task<string> Register(string username, string password)
        {
            if (!_isConnected) return "error:not_connected";

            string message = $"register:{username},{password}";
            string response = await SendAndReceive(message);

            if (response.StartsWith("success"))
            {
                Debug.Log("Registration successful");
            }
            else
            {
                Debug.LogError($"Registration failed: {response}");
            }

            return response;
        }

        // 登录账号
        public async Task<string> Login(string username, string password)
        {
            if (!_isConnected) return "error:not_connected";

            string message = $"login:{username},{password}";
            string response = await SendAndReceive(message);

            if (response.StartsWith("success"))
            {
                _username = username;
                Debug.Log("Login successful");

                // 解析初始位置
                // string[] parts = response.Split('|');
                // foreach (var part in parts)
                // {
                //     if (part.StartsWith("position:"))
                //     {
                //         string positionStr = part.Substring(9);
                //         string[] coords = positionStr.Split(',');
                //         currentPosition = new Vector3(
                //             float.Parse(coords[0]),
                //             float.Parse(coords[1]),
                //             float.Parse(coords[2]));
                //     }
                // }
            }
            else
            {
                Debug.LogError($"Login failed: {response}");
            }

            return response;
        }

        // 发送位置更新
        private async void SendPositionUpdate()
        {
            if (!_isConnected) return;

            string message = $"position:{_username},{currentPosition.x},{currentPosition.y},{currentPosition.z}";
            await SendMessage(message);
        }

        // 请求所有玩家位置
        private async void RequestAllPlayerPositions()
        {
            if (!_isConnected || string.IsNullOrEmpty(_username)) return;

            string message = $"getpositions:{_username}";
            string response = await SendAndReceive(message);

            if (response.StartsWith("success:positions|"))
            {
                ParsePlayerPositions(response);
            }
        }

        // 解析玩家位置数据
        private void ParsePlayerPositions(string response)
        {
            string positionsData = response.Substring(17); // 去掉"success:positions|"
            string[] playerEntries = positionsData.Split(';');
            Debug.Log($"player count: {playerEntries}");
            foreach (string entry in playerEntries)
            {
                string[] parts = entry.Split(':');
                if (parts.Length == 2 && parts[0] != _username) // 忽略自己的位置
                {
                    string[] coords = parts[1].Split(',');
                    var position = new PlayerPosition
                    {
                        X = float.Parse(coords[0]),
                        Y = float.Parse(coords[1]),
                        Z = float.Parse(coords[2]),
                        Timestamp = DateTime.Parse(coords[3])
                    };

                    otherPlayers.AddOrUpdate(parts[0], position, (key, oldValue) => position);
                }
            }
        }

        // 发送登出消息
        private void SendLogout()
        {
            if (!_isConnected || string.IsNullOrEmpty(_username)) return;

            string message = $"logout:{_username}";
            _ = SendMessage(message);
        }

        // 发送消息并等待响应
        private async Task<string> SendAndReceive(string message)
        {
            await SendMessage(message);
            return await ReceiveMessage();
        }

        // 发送消息
        private async Task SendMessage(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();
                Debug.Log($"Sent: {message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Send error: {e.Message}");
                Disconnect();
            }
        }

        // 接收消息
        private async Task<string> ReceiveMessage()
        {
            try
            {
                if (_stream == null || !_client.Connected)
                    return "error:disconnected";

                byte[] buffer = new byte[4096]; // 使用固定大小缓冲区
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
        
                if (bytesRead == 0)
                {
                    Disconnect();
                    return "error:disconnected";
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (!message.Contains("success:position"))
                    Debug.Log($"Received message: {message}");
                return message;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Receive error: {ex.GetType().Name}: {ex.Message}");
                Disconnect();
                return "error:network_error";
            }
        }

        // 持续接收消息
        private async void ReceiveMessages()
        {
            while (_isConnected)
            {
                try
                {
                    string message = await ReceiveMessage();
                    if (message == "error:disconnected")
                    {
                        break;
                    }

                    // 处理服务器推送的消息
                    if (message.StartsWith("broadcast:"))
                    {
                        Debug.Log($"Broadcast message: {message.Substring(10)}");
                        // 在这里处理广播消息
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Receive error: {e.Message}");
                    Disconnect();
                    break;
                }
            }
        }
    }

// 玩家位置数据结构
    public class PlayerPosition
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public DateTime Timestamp { get; set; }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }
    }
}