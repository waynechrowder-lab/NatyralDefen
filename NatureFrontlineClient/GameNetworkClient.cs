using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Gameplay.Script.NatureFrontlineClient
{
    public class GameNetworkClient : IDisposable
    {
        private TcpClient _client;
        private NetworkStream _stream;
        public bool ClientConnected => _client?.Connected ?? false;
        public bool ConnectedServer { get; private set; }

        public async UniTask ConnectAsync(string ip, int port, int timeoutMs = 5000)
        {
            var cts = new CancellationTokenSource(timeoutMs);

            try
            {
                Debug.Log($"Connecting to server at {ip}:{port}");

                _client = await UniTask.Run(() => new TcpClient(ip, port), cancellationToken: cts.Token);
                _stream = _client.GetStream();

                Debug.Log("Connection established successfully.");

                // 启动接收消息任务
                _ = ReceiveMessages(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.LogError("Connection timed out.");
                Disconnect();
                throw;
            }
            catch (SocketException ex)
            {
                Debug.LogError($"SocketException occurred: {ex.Message}");
                Disconnect();
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected exception occurred: {ex.Message}");
                Disconnect();
                throw;
            }
        }

        public void Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username and password cannot be null or empty.");
            }

            var request = new NetworkMessage
            {
                Type = MessageType.RegisterRequest,
                Data = JsonConvert.SerializeObject(new RegisterRequest
                {
                    Username = username,
                    Password = password
                })
            };

            SendMessageAsync(request.ToJson());
        }

        public async UniTask LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username and password cannot be null or empty.");
            }

            var request = new NetworkMessage
            {
                Type = MessageType.LoginRequest,
                Data = JsonConvert.SerializeObject(new LoginRequest
                {
                    Username = username,
                    Password = password
                })
            };

            try
            {
                await SendMessageAsync(request.ToJson());
            }
            catch (Exception ex)
            {
                Debug.LogError($"Login failed: {ex.Message}");
            }
        }

        public void UpdatePosition(float x, float y, float z)
        {
            // 发送给服务器
            var request = new NetworkMessage
            {
                Type = MessageType.PositionUpdate,
                Data = JsonConvert.SerializeObject(new PositionUpdate
                {
                    X = x,
                    Y = y,
                    Z = z
                })
            };

            SendMessageAsync(request.ToJson());
        }

        private async UniTask SendMessageAsync(string message)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                _stream?.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to send message: {ex.Message}");
            }
        }

        private async UniTask ReceiveMessages(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[4096];
            int bytesRead;

            try
            {
                while (!cancellationToken.IsCancellationRequested && (bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) != 0)
                {
                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var message = NetworkMessage.FromJson(json);

                    switch (message.Type)
                    {
                        case MessageType.LoginResponse:
                            var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(message.Data);
                            Debug.Log($"登录响应: {loginResponse.Message}");
                            if (loginResponse.Success) ConnectedServer = true;
                            break;

                        case MessageType.RegisterResponse:
                            var registerResponse = JsonConvert.DeserializeObject<RegisterResponse>(message.Data);
                            Debug.Log($"注册响应: {registerResponse.Message}");
                            if (registerResponse.Success) ConnectedServer = true;
                            break;

                        case MessageType.AllPlayersPositions:
                            var positions = JsonConvert.DeserializeObject<AllPlayersPositions>(message.Data);
                            Debug.Log("所有玩家位置:");
                            foreach (var player in positions.Players)
                            {
                                Debug.Log($"{player.Username}: ({player.X}, {player.Y}, {player.Z})");
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error receiving messages: {ex.Message}");
                Disconnect();
            }
        }

        public void Disconnect()
        {
            ConnectedServer = false;

            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
                _stream = null;
            }

            if (_client != null)
            {
                _client.Close();
                _client.Dispose();
                _client = null;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
