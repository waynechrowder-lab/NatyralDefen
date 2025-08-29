using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Currency.Core.Run;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using Gameplay.Script.Manager;
using Gameplay.Script.MiniWorldGameplay;
using Gameplay.Scripts.Manager;
// using Script.LiteNetLibServer;
using TMPro;
using Unity.XR.PICO.TOBSupport;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Gameplay.Script.Gameplay
{
    public class ProtectPlantsGameplay : GameplayMgr
    {
        [Header("Audio")] [SerializeField] private AudioClip gamePrepare;
        [SerializeField] private AudioClip gameStart;
        [SerializeField] private AudioClip gameWin;
        [SerializeField] private AudioClip gameFailed;
        [Header("UI")]
        [SerializeField] private CanvasGroup introducePanel;
        [SerializeField] private CanvasGroup startPanel;
        [SerializeField] private AudioClip startPanelClip;
        [SerializeField] private CanvasGroup readyPanel;
        [SerializeField] private AudioClip readyPanelClip;
        // [SerializeField] private CanvasGroup gamePanel;
        [SerializeField] private CanvasGroup failText;
        [SerializeField] private AudioClip failTextClip;
        [SerializeField] private CanvasGroup failPanel;
        [SerializeField] private CanvasGroup successText;
        [SerializeField] private AudioClip successTextClip;
        [SerializeField] private CanvasGroup successPanel;
        [SerializeField] private CanvasGroup seeThroughPanel;
        [Header("玩家设置")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform rightHand;
        [Header("游戏设置")]
        [SerializeField] private float gameSecond = 5 * 60;
        [SerializeField] private int gameOriginCoin = 50;
        [SerializeField] private Transform coinMove2;
        [SerializeField] private JungleHeart jungleHeartPrefab;
        [SerializeField] private int jungleHeartHealth;
        [SerializeField] private GameObject stakes;
        [SerializeField] private float stakesDistance = 1;
        [SerializeField] private GameObject arrowsPrefab;
        [SerializeField] private GameObject ghostPrefab;
        [SerializeField] private GameScoreSystem gameScoreSystem;
        [SerializeField] private CoinBullet coinBullet;
        [SerializeField] private Vector2 radius;
        [SerializeField] private float spawnCoinTime = 5;
        [Header("自定义时长(秒)")]
        [SerializeField] int customSecond;
        public int CustomSecond => customSecond;
        public Transform CoinMove2 => coinMove2;
        public float GameSecond => gameSecond;
        public float CurrentSecond => _currentSecond;
        public int GameOriginCoin => gameOriginCoin;
        private JungleHeart _jungleHeart;
        public JungleHeart JungleHeart => _jungleHeart;
        private GameObject _stakes;
        public Transform Target { get; private set; }
        [Header("当前游戏时长（只读）")]
        [SerializeField] private float _currentSecond;
        private int _levelIndex;
        private LevelAsset _currentGameLevel;

        [Header("时间节点提示音效")]
        [SerializeField] private AudioClip timeMarkClip;
        [SerializeField] private GameObject waringIcon;
        private AudioSource _timeAudioSource;
        private Coroutine _timeMarkCoroutine;

        QuickGameMode _gameMode = QuickGameMode.Normal;

        private void Start()
        {
            // gameSecond = customSecond > 0 ? customSecond : 60 * Entry.GameTime;
            _gameMode = GameLevelLogic.Instance.QuickGameMode;
            PicoManager.SetVideoSeeThroughForLayer(true);
            introducePanel.alpha = startPanel.alpha = readyPanel.alpha = seeThroughPanel.alpha = 0;
            failText.alpha = failPanel.alpha = successText.alpha = successPanel.alpha = 0;
            EventDispatcher.Instance.Register((int)EventID.CHANGEGAMELEVEL, OnChangeGameLevel);
            GameplayState = GameplayState.None;
            InitGame();
            _timeAudioSource = gameObject.GetComponent<AudioSource>();
            _timeAudioSource.playOnAwake = false;
        }
        
        protected void OnDestroy()
        {
            EventDispatcher.Instance.UnRegister((int)EventID.CHANGEGAMELEVEL, OnChangeGameLevel);
        }
        
        private void Update()
        {
            if (GameplayState == GameplayState.Gaming)
            {
                _currentSecond += Time.deltaTime;
                // if (_currentSecond < 0)
                //     FinishGame(true);
            }
        }
        
        private void OnChangeGameLevel(GameEventArg arg)
        {
            _levelIndex++;
            // if (_levelIndex >= GameResourcesMgr.Instance.GetGameLevelCount)
            // {
            //     FinishGame();
            //     return;
            // }
            GetNextGameLevel(3);
        }

        void InitGame()
        {
            GameplayState = GameplayState.None;
            _levelIndex = 0;
            StartCoroutine(nameof(GetReady));
        }
        
        IEnumerator GetReady()
        {
            if (_stakes) DestroyImmediate(_stakes);
            if (_jungleHeart) DestroyImmediate(_jungleHeart.gameObject);
            introducePanel.alpha = 1;
            AudioMgr.Instance.PlaySoundRepeat(gamePrepare, 2 + gamePrepare.length);
            bool nextStep = false;
            GameInputMgr.Instance.RegisterRightTrigger(Performed1, Canceled);
            yield return new WaitUntil(() => nextStep);
            GameInputMgr.Instance.UnRegisterRightTrigger(Performed1, Canceled);
            AudioMgr.Instance.PlaySoundCancelRepeat();
            gameSecond = 0;
            _currentSecond = gameSecond;
            introducePanel.alpha = 0;
            float t = .5f;//AudioMgr.Instance.PlaySound(welcomeClip);
            yield return new WaitForSeconds(t + .5f);

            int index = 0;
            var tutorialNames = PlayerPrefs.HasKey("tutorialNames") ? PlayerPrefs.GetString("tutorialNames") : "";
            GameTutorialItem item = null;
            DateTime time = DateTime.Now;
            GameInputMgr.Instance.RegisterRightTrigger(Performed, Canceled);
            while (GameTutorialMgr.Instance.StartGameTutorial(index, out item))
            {
                nextStep = false;
                time = DateTime.Now;
                yield return new WaitUntil(() => nextStep);
                if (!tutorialNames.Contains(item.panel.gameObject.name))
                {
                    tutorialNames += item.panel.gameObject.name;
                }
                index++;
            }
            PlayerPrefs.SetString("tutorialNames", tutorialNames);
            GameInputMgr.Instance.UnRegisterRightTrigger(Performed, Canceled);
            
            _stakes = Instantiate(stakes, player.position + player.forward * stakesDistance, Quaternion.identity);
            Target = _stakes.transform;

            _jungleHeart = Instantiate(jungleHeartPrefab, rightHand);
            _jungleHeart.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            var targetPoint = _stakes.transform.Find("Target");
            _jungleHeart.Init(jungleHeartHealth, targetPoint);
            var pos = targetPoint.position;
            GameObject ghost = Instantiate(ghostPrefab, pos, Quaternion.identity);
            pos.y = 1.5f;
            GameObject arrow = Instantiate(arrowsPrefab, pos, Quaternion.identity);
            EventDispatcher.Instance.Dispatch((int)EventID.GAMEPREPARE);
            startPanel.alpha = 1;
            _ = AudioMgr.Instance.PlaySound(startPanelClip);
            t = 0;
            while (t < 10)
            {
                if (_jungleHeart.TriggerArea)
                {
                    break;
                }
                t += .1f;
                yield return new WaitForSeconds(.1f);
            }

            nextStep = false;
            _jungleHeart.DoAutoMove(() =>
            {
                nextStep = true;
            });
            yield return new WaitUntil(() => nextStep);
            
            startPanel.alpha = 0;
            Destroy(arrow);
            Destroy(ghost);
            GameplayState = GameplayState.None;
            EventDispatcher.Instance.Dispatch((int)EventID.GAMESTART);

            if (_gameMode == QuickGameMode.Normal)
                GameplayLogic.Instance.StartGameplay(200);
            else
                GameplayLogic.Instance.StartGameplay(5000);

            AudioMgr.Instance.PlaySound(gameStart);

            readyPanel.alpha = 1;
            t = AudioMgr.Instance.PlaySound(readyPanelClip);
            GetNextGameLevel(3);
            yield return new WaitForSeconds(t);
            readyPanel.alpha = 0;

            void Performed1(InputAction.CallbackContext obj) => nextStep = true;
            
            void Performed(InputAction.CallbackContext obj)
            {
                if (item != null && (item.skip || tutorialNames.Contains(item.panel.name)))
                {
                    nextStep = true;
                }
                else
                {
                    if (DateTime.Now - time > TimeSpan.FromSeconds(3))
                        nextStep = true;
                }
            }
            void Canceled(InputAction.CallbackContext obj) { }
        }
        
        void GetNextGameLevel(float delay)
        {
            CancelInvoke(nameof(SpawnCoin));
            GameplayState = GameplayState.Pause;
            _currentGameLevel = GameResourcesMgr.Instance.GetGameLevel(_levelIndex);
            GameEventArg arg = EventDispatcher.Instance.GetEventArg((int)EventID.INITGAMELEVEL);
            arg.SetArg(0, _currentGameLevel);
            EventDispatcher.Instance.Dispatch((int)EventID.INITGAMELEVEL);
            // gamePanel.alpha = 1;
            Invoke(nameof(StartGameLevel), delay);
        }
        
        void StartGameLevel()
        {
            GameplayState = GameplayState.Gaming;
            // gamePanel.alpha = 0;
            InvokeRepeating(nameof(SpawnCoin), spawnCoinTime, spawnCoinTime);

            // 启动每 2 分钟一次的提示
            if (_timeMarkCoroutine != null) StopCoroutine(_timeMarkCoroutine);
            _timeMarkCoroutine = StartCoroutine(TimeMarkCoroutine());
        }

        private IEnumerator TimeMarkCoroutine()
        {
            const float interval = 60f; // 1 分钟
            while (GameplayState == GameplayState.Gaming)
            {
                yield return new WaitForSeconds(interval);
                _timeAudioSource.PlayOneShot(timeMarkClip);
                waringIcon.SetActive(false);
                yield return new WaitForSeconds(0.15f);
                waringIcon.SetActive(true);
            }
        }
        void SpawnCoin()
        {
            if (GameplayState == GameplayState.Gaming)
            {
                Vector3 randomPoint = GenerateRandomPointInRing(Target.position, radius.x, radius.y);
                randomPoint.y = 2;
                var obj = Instantiate(coinBullet, randomPoint, Quaternion.identity);
                obj.DoMoveAsGravity();
            }
        }
        
        public void FinishGame(bool success)
        {
            AudioMgr.Instance.PlaySoundOneShot(success ? gameWin : gameFailed);
            // endPanel.GetComponentInChildren<TMP_Text>().text = success ? "守卫成功" : "守卫失败";
            CancelInvoke();
            StopAllCoroutines();

            GameplayState = GameplayState.Finish;
            GameEventArg arg = EventDispatcher.Instance.GetEventArg((int)EventID.GAMEOVER);
            arg.SetArg(0, success);
            EventDispatcher.Instance.Dispatch((int)EventID.GAMEOVER);
            StartCoroutine(Settlement(success));
            if (_timeMarkCoroutine != null)
                StopCoroutine(_timeMarkCoroutine);
        }
        
        IEnumerator Settlement(bool success)
        {
            var bags = GameLevelLogic.Instance.GetUserGameBags();
            var list = new List<string>();
            for (int i = 0; i < bags.Count; i++)
            {
                var userPlant = PlantUpgradeLogic.Instance.GetUserPlantData(bags[i]);
                if (userPlant == null)
                    continue;
                list.Add(userPlant.plantId);
            }
            if (_gameMode == QuickGameMode.Normal)
            {
                BmobManager.Instance.UpdateRank((int)_currentSecond, 0, list);
            }
            else if (success)
            {
                BmobManager.Instance.UpdateRankQuickMode(_gameMode, (int)_currentSecond, 0, list);
            }

            bool nextStep = false;
            var panel = success ? successText : failText;
            panel.alpha = 1;
            AudioMgr.Instance.PlaySound(success ? successTextClip : failTextClip);
            float t = 2f;//AudioMgr.Instance.PlaySound(gameOverClip);
            yield return new WaitForSeconds(t);
            t = 2f;//AudioMgr.Instance.PlaySound(gameSuccessClip);
            yield return new WaitForSeconds(t);
            float speed = 2;
            t = 1;
            while (t > 0)
            {
                t -= Time.deltaTime * speed;
                panel.alpha = t;
                yield return null;
            }
            panel.alpha = 0;
            t = 0;
            panel = success ? successPanel : failPanel;
            panel.transform.Find("Kill").GetComponentInChildren<TMP_Text>().text = gameScoreSystem.Count.ToString();
            panel.transform.Find("Score").GetComponentInChildren<TMP_Text>().text = gameScoreSystem.Score.ToString();
            TMP_Text continueCountDown = panel.transform.Find("Continue").GetComponentInChildren<TMP_Text>();
            while (t < 1)
            {
                t += Time.deltaTime * speed;
                panel.alpha = t;
                yield return null;
            }
            panel.alpha = 1;

            {

                continueCountDown.text = "按下扳机键返回";
                GameInputMgr.Instance.RegisterRightTrigger(Performed, Canceled);

                yield return new WaitUntil(() => nextStep);

                GameInputMgr.Instance.UnRegisterRightTrigger(Performed, Canceled);
                SceneLoadManager.Instance.OnLoadScene(SceneLoadManager.SceneIndex.Lobby);

            }
            
            void Canceled(InputAction.CallbackContext obj)
            {

            }

            void Performed(InputAction.CallbackContext obj)
            {
                nextStep = true;
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

    public enum QuickGameMode
    {
        [Description("normal")] Normal = 0,
        [Description("mode1")] Mode1 = 100,
        [Description("mode2")] Mode2 = 200,
        [Description("mode3")] Mode3 = 300,
    }
}