using Gameplay.Script.Logic;
using Pico.Platform.Models;
using UnityEngine;

namespace Gameplay.Script.UI.Multiplayer
{
    public class FriendUI : MonoBehaviour
    {
        [SerializeField] private Transform friendParent;
        private GameObject _friendItem;
        
        private void OnEnable()
        {
            InvokeRepeating(nameof(OnClickGetFriends), 0, 3);
        }
        
        public void OnClickGetFriends()
        {
            FriendLogic.Instance.GetPicoFriend(OnGetPicoFriends);
        }

        private void OnGetPicoFriends(string error, UserList friends)
        {
            SetFriendItem(friends);
        }

        void SetFriendItem(UserList friends)
        {
            int count = friends?.Count ?? 0;
            PrepareItem(count);
            for (int i = 0; i < count; i++)
            {
                var friendItem = friendParent.GetChild(i).GetComponent<FriendItem>();
                var friend = friends[i];
                friendItem.InitItem(friend, OnClickFriend);
            }
        }

        void PrepareItem(int count)
        {
            if (!_friendItem)
                _friendItem = friendParent.GetChild(0).gameObject;
            for (int i = 0; i < friendParent.childCount; i++)
                friendParent.GetChild(i).gameObject.SetActive(i < count);
            for (int i = friendParent.childCount; i < count; i++)
                Instantiate(_friendItem, friendParent).SetActive(true);
        }
        
        private void OnClickFriend(User friend)
        {
            
        }
    }
}