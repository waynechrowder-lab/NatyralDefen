namespace Gameplay.Script.Scene
{
    public class LoginScene : BaseScene
    {
        protected override void Start()
        {
            base.Start();
            PicoManager.SetVideoSeeThroughForLayer(true);
        }

        public override void OnLoadScene()
        {

        }

        public override void LoadAvatar()
        {

        }

        public override void OnUnloadScene()
        {

        }
    }
}