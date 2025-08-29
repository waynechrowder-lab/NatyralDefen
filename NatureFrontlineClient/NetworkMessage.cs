using System.Collections.Generic;
using Newtonsoft.Json;

namespace Gameplay.Script.NatureFrontlineClient
{
    public enum MessageType
    {
        LoginRequest,
        LoginResponse,
        RegisterRequest,
        RegisterResponse,
        PositionUpdate,
        AllPlayersPositions
    }

    public class NetworkMessage
    {
        public MessageType Type { get; set; }
        public string Data { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static NetworkMessage FromJson(string json)
        {
            return JsonConvert.DeserializeObject<NetworkMessage>(json);
        }
    }

    // 登录请求数据
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    // 登录响应数据
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Player Player { get; set; }
    }

    // 注册请求数据
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    // 注册响应数据
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    // 位置更新数据
    public class PositionUpdate
    {
        public int PlayerId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    // 所有玩家位置数据
    public class AllPlayersPositions
    {
        public List<Player> Players { get; set; }
    }
}