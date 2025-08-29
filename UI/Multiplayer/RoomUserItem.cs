using System;
using System.Collections;
using System.Linq;
using Gameplay.Script.Manager;
using Pico.Platform.Models;
using Script.Core.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Gameplay.Script.UI.Multiplayer
{
    public class RoomUserItem : MonoBehaviour
    {
        [SerializeField] private Image ownerImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private RawImage headImage;
        private User _user;
        private Action<User> _callback;
        
        public void InitItem(bool owner, bool self, User user, Action<User> callback)
        {
            _user = user;
            _callback = callback;
            ownerImage.enabled = owner;
            nameText.text = user.DisplayName;
            StartCoroutine(LoadTexture(user.SmallImageUrl, tex => headImage.texture = tex));
        }

        public void OnClickGetUser()
        {
            _callback?.Invoke(_user);
        }
        
        IEnumerator LoadTexture(string url, Action<Texture> callback)
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