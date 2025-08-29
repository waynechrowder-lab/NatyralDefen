using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Script.Data;
using Gameplay.Script.Gameplay;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using Gameplay.Script.MultiplayerModule;
using Gameplay.Script.Scene;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Script.MiniWorldGameplay
{
    public class EnemyManager : MonoBehaviour
    {
        static List<ZombieBehaviour> _zombieBehaviours = new();
        public static List<ZombieBehaviour> ZombieBehaviours => _zombieBehaviours;

        int _deadCount;
        int _waveDeadCount;
        private GameScene _gameScene;
        private void Start()
        {
            EventDispatcher.Instance.Register((int)EventID.StartSpawnEnemy, OnStartSpawnEnemy);
            EventDispatcher.Instance.Register((int)EventID.EnemyDead, OnEnemyDead);
            EventDispatcher.Instance.Register((int)EventID.GAMEOVER, OnGameOver);
            
            GameplayMgr.Instance.RegisterGameplayStateChange(OnGameplayStateChanged);
        }
        
        private void OnDestroy()
        {
            EventDispatcher.Instance.UnRegister((int)EventID.StartSpawnEnemy, OnStartSpawnEnemy);
            EventDispatcher.Instance.UnRegister((int)EventID.EnemyDead, OnEnemyDead);
            EventDispatcher.Instance.UnRegister((int)EventID.GAMEOVER, OnGameOver);
            if (GameplayMgr.Instance)
                GameplayMgr.Instance.UnRegisterGameplayStateChange(OnGameplayStateChanged);
        }

        private void OnStartSpawnEnemy(GameEventArg arg)
        {
            InitSpawnPool();
        }

        private void OnEnemyDead(GameEventArg arg)
        {
            _deadCount++;
            _waveDeadCount++;
            var id = arg.GetArg<string>(0);
            var level = arg.GetArg<int>(1);
            var exp = arg.GetArg<int>(2);
            GameplayLogic.Instance.GetEnemyExp(id, level, exp);
        }

        private void OnGameOver(GameEventArg arg)
        {
            StopAllCoroutines();
        }
        
        private void OnGameplayStateChanged(GameplayState last, GameplayState current)
        {
            if (current == GameplayState.Pause)
            {
                _zombieBehaviours.ForEach(value =>
                {
                    if (value)
                    {
                        value.OnGamePause();
                    }
                });
            }
            else if (current == GameplayState.Gaming)
            {
                _zombieBehaviours.ForEach(value =>
                {
                    if (value)
                    {
                        value.OnGameUnPause();
                    }
                });
            }
        }
        
        void InitSpawnPool()
        {
            if (MultiplayerLogic.Instance.IsMultiPlay() && !MultiplayerLogic.Instance.IsHost())
                return;
            _deadCount = 0;
            _waveDeadCount = 0;
            _zombieBehaviours.Clear();
            StopCoroutine(nameof(CreateMonsterCoroutine));
            StartCoroutine(nameof(CreateMonsterCoroutine));
        }
        
        IEnumerator CreateMonsterCoroutine()
        {
            int currentWave = 1;
            int waveCount = GameplayLogic.Instance.GetGameLevelWaveCount();
            GameLevelItemData currentData = GameplayLogic.Instance.GetCurrentGameLevelWaveData(currentWave);
            if (currentData == null) throw new Exception("GameLevelItemData is null");
            float waveTimer = 0;
            float readyTime = currentData.waveIntervalTime;
            float gameTimer = 0;
            GameEventArg waveArg = EventDispatcher.Instance.GetEventArg((int)EventID.GameWave);
            waveArg.SetArg(0, $"第{currentWave}波/共{waveCount}波");
            EventDispatcher.Instance.Dispatch((int)EventID.GameWave);
            yield return new WaitUntil(() => GameplayMgr.Instance.GameplayState == GameplayState.Gaming);
            int totalCount = 0;
            while (true)
            {
                waveTimer += 0.1f;
                yield return new WaitForSeconds(0.1f);
                yield return new WaitUntil(() => GameplayMgr.Instance.GameplayState == GameplayState.Gaming);
                if (waveTimer < readyTime)
                {
                    continue;
                }
                waveTimer = 0;

                totalCount += currentData.waveEnemyCounts.Sum();
                float waveTime = currentData.waveTime;
                string[] waveEnemyIds = currentData.waveEnemyIds;
                int[] waveEnemyCounts = currentData.waveEnemyCounts;
                int[] waveEnemyLevs = currentData.waveEnemyLevs;
                int enemyCount = waveEnemyCounts.Sum();
                int enemyCreateCount = 0;
                while (true)
                {
                    gameTimer += .1f;
                    GameplayLogic.Instance.UpdateGameTimeUsage(gameTimer);
                    waveTimer += .1f;
                    yield return new WaitForSeconds(0.1f);
                    yield return new WaitUntil(() => GameplayMgr.Instance.GameplayState == GameplayState.Gaming);
                    if (currentWave == waveCount)
                    {
                        if (_deadCount >= totalCount)
                            break;
                    }
                    else if (waveTimer >= waveTime || _waveDeadCount >= enemyCount)
                        break;
                    if (enemyCreateCount < enemyCount)
                    {
                        string enemyId = null;
                        int enemyLevel = 1;
                        int record = 0;
                        for (int i = 0; i < waveEnemyCounts.Length; i++)
                        {
                            if (i > 0) record += waveEnemyCounts[i - 1];
                            if (waveEnemyCounts[i] - (enemyCreateCount - record) > 0 && i < waveEnemyIds.Length)
                            {
                                enemyId = waveEnemyIds[i];
                                enemyLevel = i < waveEnemyLevs.Length ? waveEnemyLevs[i] : 1;
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(enemyId))
                            CreateEnemy(enemyId, enemyLevel);
                        enemyCreateCount++;
                    }
                }
                currentWave++;
                if (currentWave <= waveCount)
                {
                    _waveDeadCount = 0;
                    currentData = GameplayLogic.Instance.GetCurrentGameLevelWaveData(currentWave);
                    if (currentData == null)
                    {
                        Debug.LogError("GameLevelItemData is null");
                        break;
                    }
                    waveArg = EventDispatcher.Instance.GetEventArg((int)EventID.GameWave);
                    waveArg.SetArg(0, $"第{currentWave}波/共{waveCount}波");
                    EventDispatcher.Instance.Dispatch((int)EventID.GameWave);
                    readyTime = currentData.waveIntervalTime;
                }
                else
                    break;
            }

            GameplayLogic.Instance.GetGameLevelAward();
            GameEventArg arg = EventDispatcher.Instance.GetEventArg((int)EventID.GAMEOVER);
            arg.SetArg(0, true);
            EventDispatcher.Instance.Dispatch((int)EventID.GAMEOVER);
        }

        private void CreateEnemy(string enemyId, int enemyLevel)
        {
            float radius = 25;
            if (!_gameScene)
                _gameScene = FindObjectOfType<GameScene>();
            if (_gameScene)
                radius = 1f;
            radius *= 100;
            float x = Random.Range(-radius, radius);
            float z = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(x, 2));
            x /= 100;
            z /= 100;
            Transform target = null;
            if (_gameScene)
            {
                if (_gameScene.enemyTargetPoint)
                    target = _gameScene.enemyTargetPoint;
                if (_gameScene.EnemySpawnPoints.Length > 0)
                {
                    int index = UnityEngine.Random.Range(0, _gameScene.EnemySpawnPoints.Length);
                    x += _gameScene.EnemySpawnPoints[index].position.x;
                    z += _gameScene.EnemySpawnPoints[index].position.z;
                }
            }
            else
            {
                target = GameObject.Find("Crystal")?.transform;
                if (target)
                {
                    x = target.position.x;
                    z = target.position.z;
                }
            }
            if (!target) target = new GameObject("Target").transform;
            Vector3 originPos = new Vector3(x, target.position.y, z);
            Quaternion originRot = Quaternion.LookRotation(target.position - originPos);
            if (!MultiplayerLogic.Instance.IsMultiPlay())
            {
                var res = Resources.Load<GameObject>($"Enemy/{enemyId}");
                if (!res)
                {
                    Debug.LogError($"file doesnt exit : {enemyId}");
                    return;
                }
                var obj = Instantiate(res).GetComponent<ZombieBehaviour>();
                obj.Initialize(GameplayMgr.Instance);
                obj.transform.SetPositionAndRotation(originPos, originRot);
                obj.OnSpawn(enemyId, enemyLevel, originPos, originRot, target);
                _zombieBehaviours.Add(obj);
                return;
            }
            var networkObj = Spawner.Instance.SpawnEnemy(enemyId);
            if (!networkObj)
            {
                Debug.LogError($"file doesnt exit : {enemyId}");
                return;
            }
            var zombieObj = networkObj.GetComponent<ZombieBehaviour>();
            zombieObj.Initialize(GameplayMgr.Instance);
            zombieObj.transform.SetPositionAndRotation(originPos, originRot);
            zombieObj.OnSpawn(enemyId, enemyLevel, originPos, originRot, target);
            _zombieBehaviours.Add(zombieObj);
        }
    }
}