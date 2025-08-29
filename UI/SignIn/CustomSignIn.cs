using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using UnityEngine;

namespace Gameplay.Script.UI
{
    public class CustomSignIn : BasedUI
    {
        [SerializeField] private Transform unlockItemParent;
        private GameObject _unlockItem;

        private void Start()
        {
            _unlockItem = unlockItemParent.GetChild(0).gameObject;
            var signInItems = unlockItemParent.GetComponentsInChildren<UnlockItemMono>(true);
            for (int i = 0; i < signInItems.Length; i++)
                signInItems[i].gameObject.SetActive(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SignInLogic.Instance.GetGameUnlockItemList(PrepareSignInData);
        }

        void PrepareSignInData(List<MiniWorldUnlockItemData> list)
        {
            var userDataList = SignInLogic.Instance.GetUserUnlockList();
            var signInItems = unlockItemParent.GetComponentsInChildren<UnlockItemMono>(true);
            for (int i = 0; i < signInItems.Length; i++)
            {
                GameObject item = signInItems[i].gameObject;;
                if (i < list.Count)
                {
                    item.SetActive(true);
                    Debug.Log($"MiniWorldUnlockItemData {list[i].itemIds}");
                    var unlockItem = userDataList.FirstOrDefault(value => value.id == list[i].id.Get());
                    if (unlockItem == null)
                    {
                        unlockItem = new UnlockItem()
                        {
                            id = list[i].id.Get(),
                            unlockDays = "",
                            unlocked = false
                        };
                    }
                    item.GetComponent<UnlockItemMono>().InitItem(list[i], unlockItem);
                }
                else
                    item.SetActive(false);
            }

            for (int i = signInItems.Length; i < list.Count; i++)
                signInItems[i].gameObject.SetActive(false);
        }
        
    }
}