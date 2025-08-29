using System;
using Gameplay.Script.Logic;
using UnityEngine;

namespace Gameplay.Script.UI
{
    public class GameLevelUI : BasedUI
    {
        [SerializeField] private GameObject gameLevelSelectUI;
        [SerializeField] private GameObject gameLevelSelectedUI;
        [SerializeField] private GameObject gameLevelBagUI;
        [SerializeField] private GameObject gameLevelPlantSelectUI;

        private void Awake()
        {
            gameLevelSelectUI.SetActive(true);
            gameLevelSelectedUI.SetActive(false);
            gameLevelBagUI.SetActive(false);
            gameLevelPlantSelectUI.SetActive(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

        }
    }
}