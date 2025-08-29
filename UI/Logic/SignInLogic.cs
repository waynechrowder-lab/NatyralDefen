using System;
using System.Collections.Generic;
using System.Linq;
using Currency.Core.Run;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Manager;

namespace Gameplay.Script.Logic
{
    public class SignInLogic : Single<SignInLogic>
    {
        public List<SignInInherentData> GetSignInDataList() => SignInData.Instance.GetSignInDataList();

        public List<string> GetUserSignInList()
        {
            var list = UserDataManager.Instance.UserDataJson?.signIn.ToList();
            return list;
        }

        public void SignIn(SignInInherentData data)
        {
            UserDataManager.Instance.SetSignInData();
            var awardsId = data.awardsId;
            var awardsCount = data.awardsCount;
            UserDataManager.Instance.SaveGoods(awardsId, awardsCount);
        }

        public void GetGameUnlockItemList(Action<List<MiniWorldUnlockItemData>> action)
        {
            BmobManager.Instance.GetGameUnlockItems(action);
        }

        public List<UnlockItem> GetUserUnlockList()
        {
            var list = UserDataManager.Instance.UserStoreJson?.userUnlockItemsJson?.goodsItem.ToList();
            return list;
        }
    }
}