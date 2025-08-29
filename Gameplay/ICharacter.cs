namespace Gameplay.Script.Gameplay
{
    public interface ICharacter
    {
        int Health { get; }
        bool IsAlive { get; }
        void UnderAttack(int damage);
        void Kill();
        void PlayAnimation(int state);
    }
}
