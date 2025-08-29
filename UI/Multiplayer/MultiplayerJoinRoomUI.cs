using System;
using Gameplay.Script.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.UI.Multiplayer
{
    public class MultiplayerJoinRoomUI : MonoBehaviour
    {
        [SerializeField] private GameObject roomListPanel;
        [SerializeField] private GameObject inRoomPanel;
        
        [SerializeField] private TMP_InputField passwordField;

        private void OnEnable()
        {
            if (!MultiplayerLogic.Instance.CurrentRoomHasPassword())
            {
                OnClickConfirm();
            }
        }

        public void OnClickConfirm()
        {
            string password = passwordField.text;
            if (MultiplayerLogic.Instance.CurrentRoomHasPassword())
            {
                if (password.Length != 5)
                    return;
            }
            MultiplayerLogic.Instance.JoinRoom(password, error =>
            {
                if (string.IsNullOrEmpty(error))
                {
                    inRoomPanel.SetActive(true);
                    gameObject.SetActive(false);
                }
            });
        }

        public void OnClickClose()
        {
            roomListPanel.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}