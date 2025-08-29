using System;
using System.Collections;
using System.Linq;
using Gameplay.Script.Manager;
using Newtonsoft.Json;
using Pico.Platform;
using Pico.Platform.Models;
using Script.Core.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class DLCItem : MonoBehaviour
    {
        [SerializeField] private RawImage dlcIcon;
        [SerializeField] private TMP_Text dlcName;
        [SerializeField] private TMP_Text dlcPrice;
        [SerializeField] private GameObject locked;
        public void InitItem(AssetDetails asset)
        {
            dlcName.text = asset.IapName;
            if (asset.IapStatus == "entitled")
            {
                locked.SetActive(false);
                dlcPrice.text = "已解锁";
            }
            else
            {
                locked.SetActive(true);
                dlcPrice.text = $"{asset.IapPrice}";
                locked.GetComponent<Button>().onClick.RemoveAllListeners();
                locked.GetComponent<Button>().onClick.AddListener(() =>
                {
                    IAPService.LaunchCheckoutFlow(asset.IapSku, asset.IapPrice, asset.IapCurrency).OnComplete(msg =>
                    {
                        if (msg.IsError)
                        {
                            Debug.LogError($"LaunchCheckoutFlow failed:{JsonConvert.SerializeObject(msg.Error)}");
                            return;
                        }
                        asset.IapStatus = "entitled";
                        locked.SetActive(false);
                        UserDataManager.Instance.PurchaseAsset(asset.Filename);
                    });
                });
            }
            StartCoroutine(LoadTexture(asset.IapIcon));
        }
        
        IEnumerator LoadTexture(string url)
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
                    dlcIcon.texture = tex;
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
                            dlcIcon.texture = tex;
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