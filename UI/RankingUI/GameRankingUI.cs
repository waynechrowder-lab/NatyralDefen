using System.Collections.Generic;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Gameplay;
using Gameplay.Script.Logic;
using UnityEngine;

namespace Gameplay.Script.UI
{
    public class GameRankingUI : BasedUI
    {
        [SerializeField] private Transform userRankingParentThird;
        [SerializeField] private Transform userRankingParent;
        [SerializeField] private GameObject userRanking;
        private GameObject _collectionItem;
        QuickGameMode _currentGameMode = QuickGameMode.Normal;

        protected override void OnEnable()
        {
            base.OnEnable();
            // RankingLogic.Instance.RegisterRankingCallback(OnRankingListCallback, OnUserRankingCallback);
            // InvokeRepeating(nameof(UpdateRankingList), 1, 60);
            // Invoke(nameof(UpdateUserRanking), 1);
            Invoke(nameof(UpdateRanking), 1);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // RankingLogic.Instance.UnRegisterRankingCallback(OnRankingListCallback, OnUserRankingCallback);
            CancelInvoke();
        }

        void UpdateRanking()
        {
            Debug.Log($"UpdateRanking: {_currentGameMode}");
            RankingLogic.Instance.QueryRanking(_currentGameMode, SetRankingListItem);
        }

        private void SetRankingListItem(List<MiniWorldUser> dataList)
        {
            int mineIndex = -1;
            if (!_collectionItem)
            {
                _collectionItem = userRankingParent.GetChild(0).gameObject;
                _collectionItem.GetComponent<CanvasGroup>().alpha = 0;
            }
            var rankingDatas = dataList;
            int count = rankingDatas?.Count ?? 0;
            for (int i = 0; i < userRankingParentThird.childCount; i++)
            {
                GameObject item = userRankingParentThird.GetChild(i).gameObject;
                item.GetComponent<CanvasGroup>().alpha = 0;
                if (i < count)
                {
                    var rankingData = rankingDatas[i];
                    item.GetComponent<UserRankingItem>().InitItem(_currentGameMode, rankingData, null, i);
                    if (rankingData.relevanceId.Equals(UserData.Instance.CurrentUser.relevanceId))
                        mineIndex = i;
                }
            }
            
            for (int i = 0; i < userRankingParent.childCount; i++)
            {
                GameObject item = userRankingParent.GetChild(i).gameObject;
                item.GetComponent<CanvasGroup>().alpha = 0;
                if (i + 3 < count)
                {
                    var rankingData = rankingDatas[i + 3];
                    item.GetComponent<UserRankingItem>().InitItem(_currentGameMode, rankingData, null, i + 3);
                    if (rankingData.relevanceId.Equals(UserData.Instance.CurrentUser.relevanceId))
                        mineIndex = i + 3;
                }
                else
                {
                    item.GetComponent<UserRankingItem>().StopAllCoroutines();
                }
            }

            for (int i = userRankingParent.childCount + 3; i < count; i++)
            {
                var item = Instantiate(_collectionItem, userRankingParent);
                var rankingData = rankingDatas[i];
                item.GetComponent<CanvasGroup>().alpha = 0;
                item.GetComponent<UserRankingItem>().InitItem(_currentGameMode,rankingData, null, i);
                if (rankingData.relevanceId.Equals(UserData.Instance.CurrentUser.relevanceId))
                    mineIndex = i;
            }
            
            userRanking.GetComponent<UserRankingItem>().InitItem(_currentGameMode, BmobManager.Instance.MiniWorldUser, null, mineIndex);
        }

        void OnRankingListCallback(List<MiniWorldUserRankingData> dataList)
        {
            SetRankingListItem(dataList);
        }

        void OnUserRankingCallback(MiniWorldUserRankingData userData)
        {
            SetUserRankingItem(userData);
        }

        void SetRankingListItem(List<MiniWorldUserRankingData> dataList)
        {
            if (!_collectionItem)
            {
                _collectionItem = userRankingParent.GetChild(0).gameObject;
                _collectionItem.GetComponent<CanvasGroup>().alpha = 0;
            }
            var rankingDatas = dataList;
            int count = rankingDatas?.Count ?? 0;
            for (int i = 0; i < userRankingParentThird.childCount; i++)
            {
                GameObject item = userRankingParentThird.GetChild(i).gameObject;
                item.GetComponent<CanvasGroup>().alpha = 0;
                if (i < count)
                {
                    var rankingData = rankingDatas[i];
                    item.GetComponent<UserRankingItem>().InitItem(rankingData, null, i);
                }
            }
            
            for (int i = 0; i < userRankingParent.childCount; i++)
            {
                GameObject item = userRankingParent.GetChild(i).gameObject;
                item.GetComponent<CanvasGroup>().alpha = 0;
                if (i + 3 < count)
                {
                    var rankingData = rankingDatas[i + 3];
                    item.GetComponent<UserRankingItem>().InitItem(rankingData, null, i + 3);
                }
            }

            for (int i = userRankingParent.childCount + 3; i < count; i++)
            {
                var item = Instantiate(_collectionItem, userRankingParent);
                var rankingData = rankingDatas[i];
                item.GetComponent<CanvasGroup>().alpha = 0;
                item.GetComponent<UserRankingItem>().InitItem(rankingData, null, i);
            }
        }

        void SetUserRankingItem(MiniWorldUserRankingData userData)
        {
            userRanking.GetComponent<UserRankingItem>().InitItem(userData, null, 0);
        }

        void UpdateRankingList()
        {
            RankingLogic.Instance.GetRankingList();
        }

        void UpdateUserRanking()
        {
            RankingLogic.Instance.GetUserRanking();
        }

        public void OnClickQueryDefault(bool b)
        {
            if (!b) return;
            _currentGameMode = QuickGameMode.Normal;
            userRankingParent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            UpdateRanking();
        }

        public void OnClickQueryByMode1(bool b)
        {
            if (!b) return;
            _currentGameMode = QuickGameMode.Mode1;
            userRankingParent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            UpdateRanking();
        }
        public void OnClickQueryByMode2(bool b)
        {
            if (!b) return;
            _currentGameMode = QuickGameMode.Mode2;
            userRankingParent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            UpdateRanking();
        }
        public void OnClickQueryByMode3(bool b)
        {
            if (!b) return;
            _currentGameMode = QuickGameMode.Mode3;
            userRankingParent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            UpdateRanking();
        }
    }
}