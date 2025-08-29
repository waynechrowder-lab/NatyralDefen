using Currency.Core.Run;
using UnityEngine;

namespace Gameplay.Script.Logic
{
    public class SettingsLogic : Single<SettingsLogic>
    {
        public bool AudioEffectMute { get; private set; } = false;
        public bool AudioBackgroundMute { get; private set; } = false;
        public float AudioEffectVolume { get; private set; } = .5f;
        public float AudioBackgroundVolume { get; private set; } = .5f;

        public void GetArchived()
        {
            if (PlayerPrefs.HasKey("SettingsArchived"))
            {
                string value = PlayerPrefs.GetString("SettingsArchived");
                if (!string.IsNullOrEmpty(value))
                {
                    var strs = value.Split(',');
                    if (strs.Length > 0)
                    {
                        if (bool.TryParse(strs[0], out bool mute))
                            AudioEffectMute = mute;
                    }
                    if (strs.Length > 1)
                    {
                        if (bool.TryParse(strs[1], out bool mute))
                            AudioBackgroundMute = mute;
                    }
                    if (strs.Length > 2)
                    {
                        if (float.TryParse(strs[2], out float volume))
                            AudioEffectVolume = volume;
                    }
                    if (strs.Length > 3)
                    {
                        if (float.TryParse(strs[3], out float volume))
                            AudioBackgroundVolume = volume;
                    }
                }
            }
        }
        public void SetAudioEffectMute(bool mute)
        {
            AudioEffectMute = mute;
            SaveArchived();
        }

        public void SetAudioBackgroundMute(bool mute)
        {
            AudioBackgroundMute = mute;
            SaveArchived();
        }

        public void SetAudioEffectVolume(float volume)
        {
            AudioEffectVolume = volume;
            SaveArchived();
        }

        public void SetAudioBackgroundVolume(float volume)
        {
            AudioBackgroundVolume = volume;
            SaveArchived();
        }

        void SaveArchived()
        {
            string archived = $"{AudioEffectMute},{AudioBackgroundMute},{AudioEffectVolume},{AudioBackgroundVolume}";
            PlayerPrefs.SetString("SettingsArchived", archived);
        }
    }
}