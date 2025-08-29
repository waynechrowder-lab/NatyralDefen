using BeatNote.Scripts.Manager;
using Gameplay.Script.Manager;
using Pico.Avatar;

namespace Gameplay.Script.Scene
{
    public class MRScene : BaseScene
    {
        public override void OnLoadScene()
        {
            AudioMgr.Instance.StopBGM();
        }

        public override void LoadAvatar()
        {

        }

        public override void OnUnloadScene()
        {

        }
    }
}