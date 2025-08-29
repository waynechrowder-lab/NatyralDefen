using System.Collections.Generic;
using System.Linq;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using Gameplay.Script.UI;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class MissionUI : BasedUI
    {
        [SerializeField] private Transform missionParent;
        private GameObject _missionItem;
        private bool _isPrecessing;
        private List<MiniWorldActivity> _miniWorldActivityDatas = new();
        protected override void OnEnable()
        {
            base.OnEnable();
            OnRefreshStoreAssets();
        }

        void OnRefreshStoreAssets()
        {
            if (!_missionItem)
            {
                _missionItem = missionParent.GetChild(0).gameObject;
                _missionItem.SetActive(false);
            }
            if (_isPrecessing) return;
            _isPrecessing = true;
            BmobManager.Instance.GetGameActivities(OnGetGameActivities);
        }

        private void OnGetGameActivities(List<MiniWorldActivity> list)
        {
            _miniWorldActivityDatas = list;
            _isPrecessing = false;
            int count = list?.Count ?? 0;
            for (int i = 0; i < missionParent.childCount; i++)
            {
                GameObject item = missionParent.GetChild(i).gameObject;
                if (i < count)
                {
                    var activity = list[i];
                    item.SetActive(true);
                    item.GetComponent<MissionItem>().InitItem(activity);
                }
                else
                    item.SetActive(false);
            }

            for (int i = missionParent.childCount; i < count; i++)
            {
                GameObject item = Instantiate(_missionItem, missionParent);
                var activity = list[i];
                item.SetActive(true);
                item.GetComponent<MissionItem>().InitItem(activity);
            }
        }
        
    }
}