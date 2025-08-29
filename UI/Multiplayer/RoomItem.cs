using System;
using System.Collections;
using System.Linq;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using Pico.Platform.Models;
using Script.Core.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Gameplay.Script.UI.Multiplayer
{
    public class RoomItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text roomName;
        [SerializeField] private TMP_Text roomId;
        [SerializeField] private TMP_Text roomMap;
        [SerializeField] private TMP_Text roomPlayer;
        [SerializeField] private RawImage roomHead;
        private Room _room;
        Action<Room> _callback;
        public void InitItem(Room room, Action<Room> callback)
        {
            _room = room;
            _callback = callback;
            var owner = room.OwnerOptional;
            roomName.text = room.Name;
            roomId.text = room.RoomId.ToString();
            var mapId = room.DataStore["map"];
            var map = MultiplayerLogic.Instance.GetMultiplayerMap(mapId);
            if (map != null) roomMap.text = map.name;
            roomPlayer.text = $"{room.UsersOptional?.Count ?? 0}/{room.MaxUsers}";
            StartCoroutine(LoadTexture(owner.SmallImageUrl, tex => roomHead.texture = tex));
        }

        public void OnClickRoom()
        {
            _callback?.Invoke(_room);
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