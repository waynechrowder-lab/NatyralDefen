using BeatNote.Scripts.Manager;

namespace Gameplay.Script.UI
{
    public class AvatarUI : BasedUI
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            
        }

        public void OnClickOpenAvatarCenter()
        {
            RoleManager.Instance.OpenPicoAvatarCenter();
        }
    }
}