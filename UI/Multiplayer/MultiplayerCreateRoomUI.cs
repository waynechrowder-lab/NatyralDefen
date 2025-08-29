using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using Michsky.UI.Shift;
using Script.Core.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Gameplay.Script.UI.Multiplayer
{
    public class MultiplayerCreateRoomUI : MonoBehaviour
    {
        [SerializeField] private GameObject inRoomPanel;
        
        [SerializeField] private TMP_Text creatorName;
        [SerializeField] private RawImage creatorHead;
        [SerializeField] private HorizontalSelector createRoomMap;
        [SerializeField] private HorizontalSelector createRoomSize;
        [SerializeField] private SwitchManager createRoomSwitch;
        [SerializeField] private TMP_InputField createRoomPasswordInput;

        private void OnEnable()
        {
            var user = UserData.Instance.CurrentUser;
            creatorName.text = $"{user.displayName}的房间";
            var maps = MultiplayerLogic.Instance.GetMultiplayerMaps();
            List<HorizontalSelector.Item> itemList = new List<HorizontalSelector.Item>();
            for (int i = 0; i < maps.Count; i++)
            {
                var mapData = MultiplayerLogic.Instance.GetMultiplayerMap(maps[i]);
                HorizontalSelector.Item item = new HorizontalSelector.Item();
                item.itemTitle = mapData.name;
                item.onValueChanged.AddListener(() => OnValueChanged(mapData.id));
                itemList.Add(item);
            }
            createRoomMap.itemList = itemList;
            createRoomMap.invokeAtStart = true;
            itemList = new List<HorizontalSelector.Item>();
            for (int i = 0; i < 6; i++)
            {
                int count = i + 2;
                HorizontalSelector.Item item = new HorizontalSelector.Item();
                item.itemTitle = count.ToString();
                item.onValueChanged.AddListener(() => OnValueChanged(count));
                itemList.Add(item);
            }
            createRoomSize.itemList = itemList;
            createRoomSize.invokeAtStart = true;
            createRoomSwitch.saveValue = false;
            createRoomSwitch.isOn = false;
            createRoomSwitch.OnEvents.AddListener(() => OnValueChanged(true));
            createRoomSwitch.OffEvents.AddListener(() => OnValueChanged(false));
            createRoomPasswordInput.text = "";
            createRoomPasswordInput.interactable = false;

            StartCoroutine(LoadTexture(user.smallImageUrl, tex => creatorHead.texture = tex));
        }

        private void OnValueChanged(string id)
        {
            MultiplayerLogic.Instance.SetMapId(id);
        }
        
        private void OnValueChanged(int count)
        {
            MultiplayerLogic.Instance.SetPlayerCount(count);
        }
        
        private void OnValueChanged(bool on)
        {
            createRoomPasswordInput.interactable = on;
        }

        public void OnClickCreateRoom()
        {
            string password = createRoomPasswordInput.text;
            if (createRoomPasswordInput.interactable)
            {
                if (password.Length != 5)
                    return;
                MultiplayerLogic.Instance.SetPassword(password);
            }
            else
                MultiplayerLogic.Instance.SetPassword(null);
            MultiplayerLogic.Instance.CreateRoom();
            inRoomPanel.SetActive(true);
            gameObject.SetActive(false);
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