using System;
using System.Collections.Generic;
using Currency.Core.Run;
using Gameplay.Script.Data;
using Gameplay.Script.MiniWorldGameplay;
using Gameplay.Script.MultiplayerModule;
using MicroWar.Platform;
using Pico.Platform;
using Pico.Platform.Models;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Script.Logic
{
    public class MultiplayerLogic : Single<MultiplayerLogic>
    {
        private PlatformController_Rooms _roomController;
        private PlatformController_Network _networkController;
        private string _mapId;
        private int _playerCount;
        private string _password;

        RoomList _roomList;
        
        public RoomList RoomList => _roomList;
        private Room _currentRoom;
        public Room CurrentRoom => _currentRoom;
        public uint ClientId { get; private set; }
        
        public void RegisterMultiNotification()
        {
            _roomController = PlatformServiceManager.Instance.GetController<PlatformController_Rooms>();
            _networkController = PlatformServiceManager.Instance.GetController<PlatformController_Network>();
            PlatformServiceManager.Instance.RegisterNotification<RoomListEvent>(HandleRoomListEvent);
            PlatformServiceManager.Instance.RegisterNotification<RoomUpdateEvent>(HandleRoomUpdateEvent);
            PlatformServiceManager.Instance.RegisterNotification<RoomPlayerEvent>(HandleRoomPlayerEvent);
        }

        public void UnregisterMultiNotification()
        {
            PlatformServiceManager.Instance.UnregisterNotification<RoomListEvent>(HandleRoomListEvent);
            PlatformServiceManager.Instance.UnregisterNotification<RoomUpdateEvent>(HandleRoomUpdateEvent);
            PlatformServiceManager.Instance.UnregisterNotification<RoomPlayerEvent>(HandleRoomPlayerEvent);
        }

        public bool IsMultiPlay() => _networkController.IsMultiPlay;
        public bool IsHost() => _networkController.IsHost;
        public void SetMapId(string mapId) => _mapId = mapId;
        public void SetPlayerCount(int playerCount) => _playerCount = playerCount;
        public void SetPassword(string password) => _password = password;

        public bool CurrentRoomHasPassword()
        {
            return _currentRoom != null && _currentRoom.DataStore.ContainsKey("password") && 
                   !string.IsNullOrEmpty(_currentRoom.DataStore["password"]);
        }
        public List<string> GetMultiplayerMaps()
        {
            return GameLevelData.Instance.GetMultiLevelIds();
        }

        public GameLevelInherentData GetMultiplayerMap(string id)
        {
            return GameLevelData.Instance.GetGameLevel(id);
        }
        
        private void HandleRoomListEvent(EventWrapper<RoomListEvent> eventdata)
        {
            // Debug.Log(eventdata.Data.RoomListRetrieveStatus);
            if (eventdata.Data.RoomListRetrieveStatus != RoomListRetrieveStatus.Idle)
                return;
            
            _roomList = eventdata.Data.RoomList;
            EventDispatcher.Instance.Dispatch((int)EventID.RoomUpdateRoomList);
        }
        
        private void HandleRoomPlayerEvent(EventWrapper<RoomPlayerEvent> eventdata)
        {
            var roomPlayerEvent = eventdata.Data;
            switch (roomPlayerEvent.RoomUserActionType)
            {
                case RoomUserActionType.Join:
                
                    break;
                case RoomUserActionType.Leave:
                    
                    break;
                case RoomUserActionType.Kicked:
                    break;
                case RoomUserActionType.HostChange:
                    break;
                default:
                    break;
            }
        }
    
        private void HandleRoomUpdateEvent(EventWrapper<RoomUpdateEvent> eventdata)
        {
            if (eventdata.Data.RoomServiceStatus != RoomServiceStatus.Idle) // Don't allow creating rooms unless the state is idle.
            {
                if (eventdata.Data.RoomServiceStatus == RoomServiceStatus.InRoom) // If in a room, update the room info UI.
                {
                    UpdateRoomInfo(eventdata.Data.CurrentRoom);
                }
                return;
            }

            if (eventdata.NotificationType == NotificationType.RoomProperties)
                UpdateRoomInfo(eventdata.Data.CurrentRoom);
            if (eventdata.NotificationType == NotificationType.RoomServiceStatus)
            {

            }
        }
        
        void UpdateRoomInfo(Room room)
        {
            _currentRoom = null;
            if (room is { UsersOptional: not null } and { OwnerOptional: not null } )
                _currentRoom = room;
            EventDispatcher.Instance.Dispatch((int)EventID.RoomUpdateRoom);
        }
        #region 房间

        public void GetRoomList()
        {
            _roomController.RetrieveRoomList();
        }

        public void CreateRoom()
        {
            RoomJoinPolicy joinPolicy = RoomJoinPolicy.Everyone;
        
            string roomName = $"{UserData.Instance.CurrentUser.displayName}的房间";
            RoomOptions options = new RoomOptions();
            options.SetRoomName(roomName);
            options.SetDataStore("map", _mapId);
            options.SetDataStore("password", _password);
            _roomController.CreateRoom(RoomJoinPolicy.Everyone, true, (uint)_playerCount, options);
            SceneLoadManager.SceneIndex sceneIndex = SceneLoadManager.SceneIndex.MultiplayerLobby;
            // SceneLoadManager.Instance.OnLoadScene(sceneIndex);
        }

        public void SetCurrentRoom(Room room)
        {
            _currentRoom = room;
        }
        
        public void JoinRoom(string password, Action<string> callback)
        {
            var dataStore = _currentRoom.DataStore;
            dataStore.TryGetValue("password", out var roomPassword);
            _mapId = dataStore["map"];
            if (_currentRoom.MaxUsers == _currentRoom.UsersOptional?.Count)
            {
                callback?.Invoke("房间已满");
                return;
            }
            if (!string.IsNullOrEmpty(roomPassword))
            {
                if (!roomPassword.Equals(password))
                {
                    callback?.Invoke("密码错误");
                    return;
                }
            }
            _roomController.JoinToRoom(_currentRoom.RoomId);
            callback?.Invoke(null);
        }
        
        public void OnLeaveRoom()
        {
            _roomController.LeaveRoom();
            _currentRoom = null;
            SceneLoadManager.Instance.OnReloadScene();
        }
        
        #endregion

        public void EnterMultiplayerGame()
        {
            var selectedInherentData = GameLevelData.Instance.GetGameLevel(_mapId);
            var selectedDatas = GameLevelData.Instance.GetGameLevelItem(_mapId);
            GameplayLogic.Instance.SetGameLevelData(selectedInherentData, selectedDatas);
            var lobbySession = Spawner.Instance.LobbySession;
            if (lobbySession)
            {
                string sceneId = selectedInherentData.id;
                if (!Enum.TryParse(sceneId, out SceneLoadManager.SceneIndex scene))
                    scene = SceneLoadManager.SceneIndex.LEVEL_M001;
                lobbySession.GetComponent<LobbySession>().EnterMultiplayerScene_ServerRpc((int)scene);
            }
        }

        public void QuitMultiplayerGame()
        {
            var gameSession = Spawner.Instance.CurrentGameSession;
            if (gameSession)
            {
                gameSession.GetComponent<GameSession>().BackToLobby_ServerRpc();
            }
        }
    }
}