using System;
using System.Collections;
using System.Linq;
using Gameplay.Script.Bmob;
using Gameplay.Script.ConstValue;
using Gameplay.Script.Data;
using Gameplay.Script.Gameplay;
using Gameplay.Script.Manager;
using Script.Core.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class UserRankingItem : MonoBehaviour
    {
        [SerializeField] private Texture2D[] defaultHead;
        [SerializeField] private TMP_Text userName;
        [SerializeField] private RawImage userHead;
        [SerializeField] private TMP_Text userScore;
        [SerializeField] private TMP_Text userRank;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Transform bagPlants;
        
        private string _url;
        private WaitForSeconds _waitSecond;
        private static readonly WaitForEndOfFrame WaitEndFrame = new WaitForEndOfFrame();
        public void InitItem(MiniWorldUserRankingData rankingData, Action onClick, int index)
        {
            StopAllCoroutines();
            //StartCoroutine(HideItem(index));
            SetUser(index, rankingData);
        }
        
        public void InitItem(QuickGameMode gameMode, MiniWorldUser rankingData, Action onClick, int index)
        {
            StopAllCoroutines();
            //StartCoroutine(HideItem(index));
            SetUser(gameMode, index, rankingData);
        }

        IEnumerator HideItem(int index)
        {
            yield return new WaitForSeconds(index * 0.1f);
            float value = canvasGroup.alpha;
            float speed = 5f;
            while (value > 0)
            {
                value -= Time.deltaTime * speed;
                yield return WaitEndFrame;
                canvasGroup.alpha = value;
            }
            canvasGroup.alpha = 0;
        }
        
        void SetUser(int index, MiniWorldUserRankingData rankingData)
        {
            userRank.text = (index + 1).ToString();
            UserDataManager.Instance.GetUserInfo(rankingData.relevanceId, SetRankUser);
            userScore.text = rankingData.fightingCapacity.Get().ToString();
            void SetRankUser(MiniWorldUser miniWorldUser)
            {
                if (miniWorldUser?.name == null)
                    return;
                StartCoroutine(ShowItem(index, () =>
                {
                    userName.text = miniWorldUser.name;
                }));
                StartCoroutine(LoadTexture(index, miniWorldUser.headUrl));
            }
        }
        
        void SetUser(QuickGameMode gameMode, int index, MiniWorldUser rankingData)
        {
            var second = rankingData.second?.Get() ?? 0;
            if (gameMode == QuickGameMode.Mode1)
            {
                second = rankingData.secondmode1?.Get() ?? 0;
            }
            else if (gameMode == QuickGameMode.Mode2)
            {
                second = rankingData.secondmode2?.Get() ?? 0;
            }
            else if (gameMode == QuickGameMode.Mode3)
            {
                second = rankingData.secondmode3?.Get() ?? 0;
            }
            userScore.text = $"{second / 60:00}分{second % 60:00}秒";
            if (index < 0)
            {
                userRank.text = "--";
                if (second > 0)
                    userRank.text = "50+";
                canvasGroup.alpha = 1;
            }
            else
                userRank.text = (index + 1).ToString();
            if (rankingData.relevanceId.Equals(UserData.Instance.CurrentUser.relevanceId))
            {
                index = 0;
            }
            SetRankUser(rankingData);
            var plants = rankingData.bagPlants;
            if (gameMode == QuickGameMode.Mode1)
            {

            }
            else if (gameMode == QuickGameMode.Mode2)
            {
                plants = rankingData.bagPlantsMode2;
            }
            else if (gameMode == QuickGameMode.Mode3)
            {

            }
            if (string.IsNullOrEmpty(plants))
            {
                bagPlants.gameObject.SetActive(false);
            }
            else
            {
                var list = plants.Split(',').ToList();
                bagPlants.gameObject.SetActive(true);
                for (int i = 0; i < list.Count; i++)
                {
                    var asset = UIAssetsBindData.Instance.GetPlantIconAsset(list[i]);
                    if (i < bagPlants.childCount)
                    {
                        bagPlants.GetChild(i).gameObject.SetActive(true);
                        bagPlants.GetChild(i).GetComponent<Image>().sprite = asset.plantIcon;
                    }
                }
                for (int i = list.Count; i < bagPlants.childCount; i ++)
                {
                    bagPlants.GetChild(i).gameObject.SetActive(false);
                }
            }
            void SetRankUser(MiniWorldUser miniWorldUser)
            {
                if (miniWorldUser?.name == null)
                    return;
                StartCoroutine(ShowItem(index, () =>
                {
                    userName.text = miniWorldUser.name;
                }));
                StartCoroutine(LoadTexture(index, miniWorldUser.headUrl));
            }
        }
        
        IEnumerator ShowItem(int index, Action callback)
        {
            yield return new WaitForSeconds(index * 0.14f);
            callback?.Invoke();
            int random = UnityEngine.Random.Range(0, defaultHead.Length);
            userHead.texture = defaultHead[random % defaultHead.Length];
            float value = 0;
            float speed = 5f;
            while (value < 1)
            {
                value += Time.deltaTime * speed;
                yield return WaitEndFrame;
                canvasGroup.alpha = value;
            }
            canvasGroup.alpha = 1;
        }
        IEnumerator LoadTexture(int index, string url)
        {
            yield return new WaitForSeconds(index * 0.15f + 0.05f);
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
                    userHead.texture = tex;
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
                            userHead.texture = tex;
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