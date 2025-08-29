namespace Gameplay.Script.Manager
{
    public interface IGameplayManager
    {
        GameplayState GameplayState { get; }
        void RegisterGameplayStateChange(System.Action<GameplayState, GameplayState> callback);
        void UnRegisterGameplayStateChange(System.Action<GameplayState, GameplayState> callback);
        void SetGameplayState(GameplayState state);
    }
}
