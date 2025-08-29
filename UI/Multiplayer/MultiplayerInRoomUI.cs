using System;
using System.Collections;
using System.Linq;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using Gameplay.Script.MultiplayerModule;
using Pico.Platform.Models;
using Script.Core.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Gameplay.Script.UI.Multiplayer
{
    public class MultiplayerInRoomUI : MonoBehaviour
    {
        [SerializeField] GameObject roomListPanel;
        
        [SerializeField] private TMP_Text roomName;
        [SerializeField] private TMP_Text roomId;
        [SerializeField] private RawImage roomHead;
        [SerializeField] private TMP_Text roomMap;
        [SerializeField] private TMP_Text roomUserCount;
        [SerializeField] private Transform inRoomPlayerParent;
        [SerializeField] private TMP_Text buttonPrepare;
        private GameObject _inRoomPlayerItem;

        private void OnEnable()
        {
            EventDispatcher.Instance.Register((int)EventID.RoomUpdateRoom, OnRoomUpdateRoom);
            if (MultiplayerLogic.Instance.IsMultiPlay())
            {
                OnRoomUpdateRoom(null);
            }
        }

        private void OnDisable()
        {
            EventDispatcher.Instance.UnRegister((int)EventID.RoomUpdateRoom, OnRoomUpdateRoom);
        }

        private void OnRoomUpdateRoom(GameEventArg arg)
        {
            buttonPrepare.text = MultiplayerLogic.Instance.IsHost() ? "开始" : "准备";
            Room room = MultiplayerLogic.Instance.CurrentRoom;
            if (room == null) throw new Exception("Room is null");
            roomName.text = room.Name;
            roomId.text = room.RoomId.ToString();
            string mapId = room.DataStore["map"];
            var map = MultiplayerLogic.Instance.GetMultiplayerMap(mapId);
            if (map != null) roomMap.text = map.name;
            roomUserCount.text = $"{room.UsersOptional?.Count ?? 0}/{room.MaxUsers}";
            SetRoomItem();
            
            var owner = room.OwnerOptional;
            StartCoroutine(LoadTexture(owner.SmallImageUrl, tex => roomHead.texture = tex));
            // Debug.Log("OwnerId:" + owner.ID);
            // UserDataManager.Instance.GetUserInfo(owner.ID, user =>
            // {
            //     if (user == null) return;
            //
            // });
        }
        
        void SetRoomItem()
        {
            Room room = MultiplayerLogic.Instance.CurrentRoom;
            if (room == null)
            {
                PrepareItem(0);
                return;
            }

            int count = room.UsersOptional?.Count ?? 0;
            Debug.Log($"UserCount: {count}");
            PrepareItem(count);
            for (int i = 0; i < count; i++)
            {
                var user = room.UsersOptional[i];
                RoomUserItem roomItem = inRoomPlayerParent.GetChild(i).GetComponent<RoomUserItem>();
                bool self = user.ID == UserData.Instance.PicoUser.ID;
                bool owner = user.ID == room.OwnerOptional.ID;
                roomItem.InitItem(owner, self, user, OpenUserPanel);
            }
        }

        void PrepareItem(int count)
        {
            if (!_inRoomPlayerItem)
                _inRoomPlayerItem = inRoomPlayerParent.GetChild(0).gameObject;
            for (int i = 0; i < inRoomPlayerParent.childCount; i++)
                inRoomPlayerParent.GetChild(i).gameObject.SetActive(i < count);
            for (int i = inRoomPlayerParent.childCount; i < count; i++)
                Instantiate(_inRoomPlayerItem, inRoomPlayerParent).SetActive(true);
        }

        private void OpenUserPanel(User user)
        {
            OnAddFriend(user);
        }

        void OnAddFriend(User user)
        {
            if (!FriendLogic.Instance.IsExitFriend(user.ID))
            {
                FriendLogic.Instance.AddPicoFriend(user.ID, error =>
                {
                    if (string.IsNullOrEmpty(error))
                    {
                        Debug.Log("add friend success");
                    }
                    else
                    {
                        Debug.LogError($"add friend error:{error}");
                    }
                }, () =>
                {
                    Debug.Log("add friend canceled");
                });
            }
        }
        
        public void OnClickQuitRoom()
        {
            MultiplayerLogic.Instance.OnLeaveRoom();
            roomListPanel.SetActive(true);
            gameObject.SetActive(false);
        }

        public void OnClickPrepare()
        {
            if (MultiplayerLogic.Instance.IsHost())
            {
                MultiplayerLogic.Instance.EnterMultiplayerGame();
            }
        }
        
        IEnumerator LoadTexture(string url, Action<Texture2D> callback)
        {
            string filename = null;
            string originUrl = url;
            Uri uri = new Uri(url);
            bool save = false;
            if (uri.IsFile)
            {
                filename = uri.Segments.Last().Trim('/');
                if (!string.IsNullOrEmpty(filename))
                    filename = $"{UserDataManager.Instance.UserHeadFolder}/{filename}";
            }
            save = !string.IsNullOrEmpty(filename) && IOHelper.IsExistFile(filename);
            if (save)
                url = $"file://{filename}";

            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();
                if (string.IsNullOrEmpty(www.error))
                {
                    Texture2D tex = new Texture2D(128, 128);
                    tex = DownloadHandlerTexture.GetContent(www);
                    callback?.Invoke(tex);
                    try
                    {
                        if (!save && !string.IsNullOrEmpty(filename))
                            IOHelper.CreateFile(filename, www.downloadHandler.data);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
                else
                {
                    using (UnityWebRequest www1 = UnityWebRequestTexture.GetTexture(originUrl))
                    {
                        yield return www1.SendWebRequest();
                        if (string.IsNullOrEmpty(www1.error))
                        {
                            Texture2D tex = new Texture2D(128, 128);
                            tex = DownloadHandlerTexture.GetContent(www1);
                            callback?.Invoke(tex);
                        }
                        else
                            Debug.LogError($"texture load failed {www1.error} + {originUrl}");
                    }
                    Debug.LogError($"texture load failed {www.error} + {url}");
                }
            }
        }
    }
}