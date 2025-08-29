using System.Collections.Generic;
using Gameplay.Script.Bmob;
using Gameplay.Script.Logic;
using TMPro;
using UnityEngine;

namespace Gameplay.Script.UI
{
    public class NoticeUI : BasedUI
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text content;
        protected override void OnEnable()
        {
            base.OnEnable();
            Invoke(nameof(DelayGetNotice), 0.2f);
            EventDispatcher.Instance.Register((int)EventID.SetNotice, OnSetNotice);
        }

        void DelayGetNotice()
        {
            NoticeLogic.Instance.GetNoticeList();
        }

        protected override void OnDisable()
        {
            EventDispatcher.Instance.UnRegister((int)EventID.SetNotice, OnSetNotice);
            base.OnDisable();
        }

        private void OnSetNotice(GameEventArg arg)
        {
            int type = arg.GetArg<int>(0);
            if (type == 1)
            {
                var list = arg.GetArg<List<MiniWorldUNotice>>(1);
                title.text = list[0].title;
                content.text = list[0].content;
            }
        }
    }
}