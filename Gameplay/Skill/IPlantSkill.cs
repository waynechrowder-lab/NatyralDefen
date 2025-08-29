namespace Gameplay.Script.Gameplay
{
    public interface IPlantSkill
    {
        public const int MaxLevel = 5;
        void Init(PlantContext context, PlantSkillAsset asset);
        void Tick(float dt);
        void OnUpgrade(IntensifyData data);
        void OnPaused(bool paused);
        void OnPlantDeath();
    }
}