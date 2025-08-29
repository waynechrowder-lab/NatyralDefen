namespace Gameplay.Script.Gameplay
{
    public interface IZombieSkill
    {
        void Init(ZombieContext context, ZombieSkillAsset asset);
        void Tick(float dt);
        void OnPaused(bool paused);
        void OnZombieDeath();
    }
}
