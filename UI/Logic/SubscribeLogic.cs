using System;
using System.Globalization;
using System.Linq;
using Currency.Core.Run;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Manager;

namespace Gameplay.Script.Logic
{
    public class SubscribeLogic : Single<SubscribeLogic>
    {
        public void Subscribe(MiniWorldShopData goods)
        {
            UserDataManager.Instance.Subscribe(goods.itemId);
        }

        public bool HasSubscribed(string goodsItemId)
        {
            var subscribeList = UserDataManager.Instance.UserDataJson?.userSubscribeJson?.ToList() ??
                                                Array.Empty<UserSubscribeDataJson>().ToList();
            return subscribeList.Any(value => value.subscribeId.Equals(goodsItemId));
        }

        public bool HasGetSubscribedGoods(string goodsItemId)
        {
            var subscribeList = UserDataManager.Instance.UserDataJson?.userSubscribeJson?.ToList() ??
                                Array.Empty<UserSubscribeDataJson>().ToList();
            var subscribe = subscribeList.First(value => value.subscribeId.Equals(goodsItemId));
            var subscribeStartTime = subscribe.subscribedTime;
            string format = "yy-MM-dd";
            _ = DateTime.TryParseExact(
                subscribeStartTime, 
                format, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out DateTime parsedDate
            );
            var getIndexList = subscribe.getSubscribedItemIndex.ToList();
            var difference = DateTime.Now - parsedDate;
            int daysDifference = Math.Abs(difference.Days);
            return getIndexList.Contains(daysDifference);
        }

        public void GetSubscribedGoods(MiniWorldShopData goods)
        {
            var subscribeList = UserDataManager.Instance.UserDataJson.userSubscribeJson.ToList();
            var subscribe = subscribeList.First(value => value.subscribeId.Equals(goods.itemId));
            var subscribeStartTime = subscribe.subscribedTime;
            string format = "yy-MM-dd";
            _ = DateTime.TryParseExact(
                subscribeStartTime, 
                format, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out DateTime parsedDate
            );
            var getIndexList = subscribe.getSubscribedItemIndex.ToList();
            var difference = DateTime.Now - parsedDate;
            int daysDifference = Math.Abs(difference.Days);
            if (!getIndexList.Contains(daysDifference))
            {
                getIndexList.Add(daysDifference);
                UserDataManager.Instance.SaveCoin(goods.itemCount?.Get() ?? 0);
                subscribe.getSubscribedItemIndex = getIndexList.ToArray();
                for (int i = 0; i < subscribeList.Count; i++)
                {
                    if (subscribeList[i].subscribeId.Equals(subscribe.subscribeId))
                    {
                        subscribeList[i] = subscribe;
                        break;
                    }
                }

                lock (UserDataManager.Instance)
                    UserDataManager.Instance.UserDataJson.userSubscribeJson = subscribeList.ToArray();
            }
        }
    }
}