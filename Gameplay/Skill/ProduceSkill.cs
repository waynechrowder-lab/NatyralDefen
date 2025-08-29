using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class ProduceSkill : MonoBehaviour, IPlantSkill
    {
        public ProduceSkillAsset Data;
        private PlantContext _ctx;
        private float _timer;
        private Transform _origin;

        public void Init(PlantContext context, PlantSkillAsset asset)
        {
            _ctx = context;
            Data = (ProduceSkillAsset)Instantiate(asset);
            _timer = Data.cooldown;
            if (!string.IsNullOrEmpty(Data.originName))
                _origin = transform.Find(Data.originName);
            else
                _origin = transform;
        }


        public void Tick(float dt)
        {
            _timer += dt;
            if (_timer < Data.cooldown) return;
            _timer = 0f;
            
            if (Data.coinPrefab && _origin)
            {
                var obj = Instantiate(Data.coinPrefab, _origin.position, Quaternion.identity);
                var coin = obj as CoinUserBullet;
                if (coin != null)
                {
                    coin.DoMoveAsParabola(Data.throwRange, Data.produceValue);
                }
            }
        }


        public void OnUpgrade(IntensifyData data)
        {
            var currentLevel = _ctx.Level;
            if (currentLevel <= 5)
            {
                Data.produceValue = (int)(data.attackValueK * Data.produceValue);
                Data.cooldown *= data.attackInterval;
                _timer = Data.cooldown;
            }
        }
        public void OnPaused(bool paused) { }
        public void OnPlantDeath() { }
    }
}