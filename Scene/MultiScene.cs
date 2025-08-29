using Gameplay.Script.Logic;
using Gameplay.Script.MultiplayerModule;

namespace Gameplay.Script.Scene
{
    public class MultiScene : GameScene
    {
        public override void OnLoadScene()
        {
            base.OnLoadScene();
            if (MultiplayerLogic.Instance.IsHost())
                Spawner.Instance.SpawnGameSession();
        }

        public override void OnUnloadScene()
        {
            base.OnUnloadScene();
            if (MultiplayerLogic.Instance.IsHost())
                Spawner.Instance.UnSpawnGameSession();
        }
    }
}