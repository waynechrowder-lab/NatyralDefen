using System;
using Currency.Core.Run;
using Gameplay.Script.Manager;
using TMPro;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class GameScoreSystem : MonoBehaviour
    {
        [SerializeField] private GameObject scoreText;

        public int Score { get; private set; }
        
        public int Coin { get; private set; }
        public int Exp { get; private set; }

        private int _lastCoin = 0;
        private int _lastExp = 0;
        private DateTime _lastSavedTimer;

        public int Count { get; private set; }

        private void Start()
        {
            _lastSavedTimer = DateTime.Now;
            EventDispatcher.Instance.Register((int)EventID.GAMEPREPARE, OnPrepare);
            EventDispatcher.Instance.Register((int)EventID.MONSTERDEAD, OnScore);
            EventDispatcher.Instance.Register((int)EventID.GAMEOVER, OnOver);
        }
        
        protected void OnDestroy()
        {
            EventDispatcher.Instance.UnRegister((int)EventID.GAMEPREPARE, OnPrepare);
            EventDispatcher.Instance.UnRegister((int)EventID.MONSTERDEAD, OnScore);
            EventDispatcher.Instance.UnRegister((int)EventID.GAMEOVER, OnOver);
        }
        
        private void OnPrepare(GameEventArg arg)
        {
            Score = 0;
            Count = 0;
            if (scoreText)
            {
                scoreText.SetActive(true);
                scoreText.GetComponentInChildren<TMP_Text>().text = Score.ToString();
            }
        }
        
        private void OnScore(GameEventArg arg)
        {
            int score = arg.GetArg<int>(0);
            // Vector3 pos = arg.GetArg<Vector3>(1);
            int coin = arg.GetArg<int>(1);
            int exp = arg.GetArg<int>(2);
            Coin += coin;
            Exp += exp;
            Debug.Log($"coin{coin}, exp{exp}");
            Score += score;
            Count += 1;
            scoreText.GetComponentInChildren<TMP_Text>().text = Score.ToString();
            TrySave();
        }
        
        private void OnOver(GameEventArg arg)
        {
            if (scoreText)
                scoreText.SetActive(false);
            TrySave(true);
        }

        void TrySave(bool force = false)
        {
            var deltaTime = DateTime.Now - _lastSavedTimer;
            if (deltaTime.TotalSeconds > 30 || force)
            {
                _lastSavedTimer = DateTime.Now;
                var deltaCoin = Coin - _lastCoin;
                var deltaExp = Exp - _lastExp;
                UserDataManager.Instance.SaveCoinAndExp(deltaCoin, deltaExp);
                _lastCoin = Coin;
                _lastExp = Exp;
            }
        }
    }
}