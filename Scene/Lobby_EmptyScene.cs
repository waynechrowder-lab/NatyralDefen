namespace Gameplay.Script.Scene
{
    public class Lobby_EmptyScene : BaseScene
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