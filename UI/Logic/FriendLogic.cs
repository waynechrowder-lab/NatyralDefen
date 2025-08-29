using System;
using System.Linq;
using Currency.Core.Run;
using Pico.Platform;
using Pico.Platform.Models;

namespace Gameplay.Script.Logic
{
    public class FriendLogic : Single<FriendLogic>
    {
        private UserList _friends;
        public void GetPicoFriend(Action<string, UserList> onGetFriends)
        {
            // 获取好友列表
            UserService.GetFriends().OnComplete(msg =>
            {
                if (msg.IsError)
                {
                    onGetFriends(msg.Error.Message, null);
                    return;
                }

                _friends = msg.Data;
                onGetFriends(null, _friends);
            });
        }

        public bool IsExitFriend(string targetUserId)
        {
            if (_friends == null) return false;
            return _friends.Any(value => value.ID.Equals(targetUserId));
        }
        
        public void AddPicoFriend(string targetUserId, Action<string> onAdd, Action onCanceled)
        {
            // 向目标用户发出好友申请
            UserService.LaunchFriendRequestFlow(targetUserId).OnComplete(msg =>
            {
                if (msg.IsError)
                {
                    onAdd(msg.Error.Message);
                    return;
                }

                if (msg.Data.DidCancel)
                {
                    onCanceled();
                    return;
                }

                onAdd(null);
            });
        }
    }
}