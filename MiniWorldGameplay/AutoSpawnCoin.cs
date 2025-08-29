using System.Collections;
using Gameplay.Script.Gameplay;
using Gameplay.Script.Manager;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.Script.MiniWorldGameplay
{
    public class AutoSpawnCoin : MonoBehaviour
    {
        [FormerlySerializedAs("coinBullet")] [SerializeField] private CoinUserBullet coinUserBullet;
        [SerializeField] private Transform originPoint;
        [SerializeField] private Vector2 radius = new(1.5f, 3);
        [SerializeField] private float spawnCoinTime = 5;
        private void Start()
        {
            StartCoroutine(nameof(AutoSpawnCoinCoroutine));
        }

        IEnumerator AutoSpawnCoinCoroutine()
        {
            yield return new WaitForSeconds(1f);
            float t = spawnCoinTime;
            while (true)
            {
                t -= .1f;
                yield return new WaitForSeconds(.1f);
                yield return new WaitUntil(() => GameplayMgr.Instance.GameplayState == GameplayState.Gaming);
                if (t <= 0)
                {
                    t = spawnCoinTime;
                    Vector3 randomPoint = GenerateRandomPointInRing(originPoint.position, radius.x, radius.y);
                    randomPoint.y += 2f;
                    var obj = Instantiate(coinUserBullet, randomPoint, Quaternion.identity);
                    obj.DoMoveAsGravity();
                }
            }
        }
        
        Vector3 GenerateRandomPointInRing(Vector3 center, float minRadius, float maxRadius)
        {
            Vector2 randomDir2D = Random.insideUnitCircle.normalized;
            Vector3 direction = new Vector3(randomDir2D.x, 0, randomDir2D.y);
            
            float distance = Random.Range(minRadius, maxRadius);
    
            return center + direction * distance;
        }
    }
    
}