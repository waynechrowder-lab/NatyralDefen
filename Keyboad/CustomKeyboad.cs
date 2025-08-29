using System;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script
{
    public class CustomKeyboad : MonoBehaviour
    {
        [SerializeField] private CanvasGroup codeParent;
        private Action<StringBuilder> _inputCallback;
        private StringBuilder _codeStr = new StringBuilder();
        private int _codeLength;
        private const string chars = "QWERTYUIOPASDFGHJKLZXCVBNM1234567890";
        private void Start()
        {
            var buttons = codeParent.GetComponentsInChildren<Button>();
            int i = 0;
            buttons.ToList().ForEach(value =>
            {
                int k = i;
                if (k >= chars.Length) return;
                value.onClick.RemoveAllListeners();
                value.onClick.AddListener(() =>
                {
                    OnClickCode(chars[k].ToString());
                });
                value.GetComponentInChildren<TMP_Text>().text = chars[k].ToString();
                i++;
            });
            buttons[^1].onClick.RemoveAllListeners();
            buttons[^1].onClick.AddListener(() =>
            {
                OnClickCode(null);
            });
        }

        public void RegisterInput(int length, Action<StringBuilder> callback)
        {
            _codeLength = length;
            _inputCallback = callback;
            Clear();
        }

        public void Clear()
        {
            _codeStr.Clear();
        }
        
        public void Enable()
        {
            codeParent.interactable = true;
        }
        
        public void Disable()
        {
            codeParent.interactable = false;
        }

        public void OnClickCode(string code)
        {
            if (!string.IsNullOrEmpty(code))
            {
                if (_codeStr.Length < _codeLength)
                    _codeStr.Append(code);
            }
            else
            {
                if (_codeStr.Length > 0)
                    _codeStr.Length--;
            }
            _inputCallback?.Invoke(_codeStr);
        }
    }
}