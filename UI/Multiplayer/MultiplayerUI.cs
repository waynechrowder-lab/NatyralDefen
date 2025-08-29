using System;
using Gameplay.Script.Logic;
using UnityEngine;


namespace Gameplay.Script.UI.Multiplayer
{
    public class MultiplayerUI : BasedUI
    {
        [SerializeField] GameObject roomListPanel;
        [SerializeField] GameObject joinRoomPanel;
        [SerializeField] GameObject inRoomPanel;

        private void Awake()
        {
            roomListPanel.SetActive(false);
            joinRoomPanel.SetActive(false);
            inRoomPanel.SetActive(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (MultiplayerLogic.Instance.IsMultiPlay() && MultiplayerLogic.Instance.CurrentRoom != null)
            {
                roomListPanel.SetActive(false);
                inRoomPanel.SetActive(true);
            }
            else
            {
                roomListPanel.SetActive(true);
                inRoomPanel.SetActive(false);
            }
        }
        
    }
}