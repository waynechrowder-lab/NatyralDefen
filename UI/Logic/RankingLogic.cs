using Currency.Core.Run;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Gameplay;
using System;
using System.Collections.Generic;

namespace Gameplay.Script.Logic
{
    public class RankingLogic : Single<RankingLogic>
    {
        private MiniWorldUserRankingData _miniWorldUserRankingData;
        private string _objectId = null;
        private List<MiniWorldUserRankingData> _dataList = new List<MiniWorldUserRankingData>();
        private Action<List<MiniWorldUserRankingData>> _rankingListCallback;
        private Action<MiniWorldUserRankingData> _userRankingCallback;

        public void RegisterRankingCallback(Action<List<MiniWorldUserRankingData>> rankingListCallback, 
            Action<MiniWorldUserRankingData> userRankingCallback)
        {
            _rankingListCallback += rankingListCallback;
            _userRankingCallback += userRankingCallback;
        }

        public void UnRegisterRankingCallback(Action<List<MiniWorldUserRankingData>> rankingListCallback, 
            Action<MiniWorldUserRankingData> userRankingCallback)
        {
            _rankingListCallback -= rankingListCallback;
            _userRankingCallback -= userRankingCallback;
        }

        public void QueryRanking(QuickGameMode gameMode, Action<List<MiniWorldUser>> onQueryCallback)
        {
            if (gameMode == QuickGameMode.Normal)
                BmobManager.Instance.QueryRank(onQueryCallback);
            else
            {
                BmobManager.Instance.QueryRankQuickMode(gameMode, onQueryCallback);
            }
        }
        
        public void GetRankingList()
        {
            BmobManager.Instance.GetMiniWorldUserRankingList("fightingCapacity", 50, list =>
            {
                _dataList = list;
                _rankingListCallback?.Invoke(list);
            });
        }

        public void GetUserRanking()
        {
            string relevanceId = UserData.Instance.CurrentUser.relevanceId;
            BmobManager.Instance.GetMiniWorldUserRanking(relevanceId, data =>
            {
                if (data == null)
                {
                    MiniWorldUserRankingData userData = new MiniWorldUserRankingData()
                    {
                        relevanceId = relevanceId,
                        fightingCapacity = 0
                    };
                    _userRankingCallback?.Invoke(userData);
                    BmobManager.Instance.CreateMiniWorldUserRanking(userData, objectId =>
                    {
                        _objectId = objectId;
                    });
                }
                else
                {
                    _miniWorldUserRankingData = data;
                    _objectId = data.objectId;
                    _userRankingCallback?.Invoke(data);
                }
            });
        }

        public void UpdateUserRanking(int fightingCapacity)
        {
            if (_objectId != null)
            {
                if (_miniWorldUserRankingData != null &&
                    _miniWorldUserRankingData.fightingCapacity.Get().Equals(fightingCapacity)) return;
                MiniWorldUserRankingData userData = new MiniWorldUserRankingData()
                {
                    fightingCapacity = fightingCapacity
                };
                BmobManager.Instance.UpdateMiniWorldUserRanking(_objectId, userData, null);
                userData.relevanceId = UserData.Instance.CurrentUser.relevanceId;
                _userRankingCallback?.Invoke(userData);
            }
        }

        public MiniWorldUserRankingData GetRanking(int index)
        {
            MiniWorldUserRankingData data = null;
            if (index < _dataList.Count)
                data = _dataList[index];
            return data;
        }
    }
}