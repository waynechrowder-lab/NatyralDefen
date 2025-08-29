using System.Collections.Generic;
using System.Linq;
using Currency.Core.Run;
using Gameplay.Script.Bmob;
using UnityEngine;

namespace Gameplay.Script.Logic
{
    public class NoticeLogic : Single<NoticeLogic>
    {
        List<MiniWorldUNotice> _noticeList = new();

        public void GetNoticeList()
        {
            if (_noticeList.Count == 0)
            {
                BmobManager.Instance.GetGameNotice(GetGameNoticeCallback);
            }
            else
                GetGameNoticeCallback(_noticeList);
        }

        void GetGameNoticeCallback(List<MiniWorldUNotice> noticeList)
        {
            _noticeList = noticeList;
            if (_noticeList.Count == 0) return;
            string version = Application.version;
            List<MiniWorldUNotice> feedbackNoticeList = new();
            List<MiniWorldUNotice> defaultNoticeList = new();
            _noticeList.ForEach(value =>
            {
                if (value.version.Equals("0") && value.title.Equals("反馈"))
                {
                    feedbackNoticeList.Add(value);
                }
                else if (value.version.Equals("0") || value.version.Equals(version))
                {
                    defaultNoticeList.Add(value);
                }
            });
            if (feedbackNoticeList.Count > 0)
            {
                GameEventArg arg1 = EventDispatcher.Instance.GetEventArg((int)EventID.SetNotice);
                arg1.SetArg(0, 0);
                arg1.SetArg(1, feedbackNoticeList);
                EventDispatcher.Instance.Dispatch((int)EventID.SetNotice);
            }

            if (defaultNoticeList.Count > 0)
            {
                GameEventArg arg2 = EventDispatcher.Instance.GetEventArg((int)EventID.SetNotice);
                arg2.SetArg(0, 1);
                arg2.SetArg(1, defaultNoticeList);
                EventDispatcher.Instance.Dispatch((int)EventID.SetNotice);
            }

        }
    }
}