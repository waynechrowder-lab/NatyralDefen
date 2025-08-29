using BeatNote.Scripts.Manager;
using Gameplay.Script.Manager;
using Pico.Avatar;

namespace Gameplay.Script.Scene
{
    public class LobbyScene : BaseScene
    {
        public override void OnLoadScene()
        {
            AudioMgr.Instance.PlayBGM("LobbyBGM");
        }

        public override void LoadAvatar()
        {
            RoleManager.Instance.LoadAvatar(true, null, DeviceInputReaderBuilderInputType.PicoXR);
        }

        public override void OnUnloadScene()
        {

        }
    }
}