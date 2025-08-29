using System.Collections.Generic;
using Gameplay.Script.Bmob;
using Gameplay.Script.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class SettingsUI : BasedUI
    {
        [SerializeField] private GameObject privacyPolicyPanel;
        [SerializeField] private GameObject aboutPanel;
        [SerializeField] private TMP_Text aboutText;
        
        [SerializeField] private Toggle audioEffectMute;
        [SerializeField] private Toggle audioBackgroundMute;
        
        [SerializeField] private Slider audioEffectVolume;
        [SerializeField] private Slider audioBackgroundVolume;

        [SerializeField] private TMP_Text feedbackText;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            privacyPolicyPanel.SetActive(false);
            aboutPanel.SetActive(false);
            SettingsLogic.Instance.GetArchived();
            audioEffectMute.isOn = SettingsLogic.Instance.AudioEffectMute;
            audioBackgroundMute.isOn = SettingsLogic.Instance.AudioBackgroundMute;
            audioEffectVolume.value = SettingsLogic.Instance.AudioEffectVolume;
            audioBackgroundVolume.value = SettingsLogic.Instance.AudioBackgroundVolume;
            EventDispatcher.Instance.Register((int)EventID.SetNotice, OnSetNotice);
            aboutText.text = $"游戏名称：自然防线\n" +
                             $"版本号：{Application.version}\n" +
                             $"发布日期：2025.07.31\n" +
                             $"版权声明：@2025 上海墟介科技有限公司\n" +
                             $"联系我们：18052636968@163.com";
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventDispatcher.Instance.UnRegister((int)EventID.SetNotice, OnSetNotice);
        }

        private void OnSetNotice(GameEventArg arg)
        {
            int type = arg.GetArg<int>(0);
            if (type == 0)
            {
                var list = arg.GetArg<List<MiniWorldUNotice>>(1);
                feedbackText.text = list[0].content;
            }
        }

        public void OnClickAudioEffectMute(bool mute)
        {
            SettingsLogic.Instance.SetAudioEffectMute(mute);
        }
        
        public void OnClickAudioBackgroundMute(bool mute)
        {
            SettingsLogic.Instance.SetAudioBackgroundMute(mute);
        }

        public void OnClickAudioEffectVolume(float volume)
        {
            SettingsLogic.Instance.SetAudioEffectVolume(volume);
        }

        public void OnClickAudioBackgroundVolume(float volume)
        {
            SettingsLogic.Instance.SetAudioBackgroundVolume(volume);
        }

        public void OnClickAccount()
        {
            
        }

        public void OnClickAbout()
        {
            aboutPanel.SetActive(true);
        }

        public void OnClickSupport()
        {
            privacyPolicyPanel.SetActive(true);
        }

        public void OnClickClose()
        {
            aboutPanel.SetActive(false);
            privacyPolicyPanel.SetActive(false);
        }
    }
}