using System;
using Gameplay.Script.Logic;
using MicroWar.Platform;
using Pico.Platform.Models;
using UnityEngine;

namespace Gameplay.Script.UI.Multiplayer
{
    public class MultiplayerRoomListUI : MonoBehaviour
    {
        [SerializeField] private GameObject joinRoomPanel;
        [SerializeField] private Transform roomParent;
        private GameObject _roomItem;

        private void OnEnable()
        {
            EventDispatcher.Instance.Register((int)EventID.RoomUpdateRoomList, OnRoomUpdateRoomList);
            InvokeRepeating(nameof(RefreshRoomList), 1, 3);
            SetRoomItem();
        }

        void OnDisable()
        {
            CancelInvoke();
            EventDispatcher.Instance.UnRegister((int)EventID.RoomUpdateRoomList, OnRoomUpdateRoomList);
        }
        
        private void OnRoomUpdateRoomList(GameEventArg arg)
        {
            SetRoomItem();
        }

        void SetRoomItem()
        {
            var roomList = MultiplayerLogic.Instance.RoomList;
            if (roomList == null)
            {
                PrepareItem(0);
                return;
            }
            PrepareItem(roomList.Count);
            for (int i = 0; i < roomList.Count; i++)
            {
                RoomItem roomItem = roomParent.GetChild(i).GetComponent<RoomItem>();
                Room room = roomList[i];
                roomItem.InitItem(room, OpenJoinPanel);
            }
        }

        void PrepareItem(int count)
        {
            if (!_roomItem)
                _roomItem = roomParent.GetChild(0).gameObject;
            for (int i = 0; i < roomParent.childCount; i++)
                roomParent.GetChild(i).gameObject.SetActive(i < count);
            for (int i = roomParent.childCount; i < count; i++)
                Instantiate(_roomItem, roomParent).SetActive(true);
        }
        
        private void OpenJoinPanel(Room room)
        {
            MultiplayerLogic.Instance.SetCurrentRoom(room);
            joinRoomPanel.SetActive(true);
        }
        
        public void RefreshRoomList()
        {
            MultiplayerLogic.Instance.GetRoomList();
        }
        
        
    }
}