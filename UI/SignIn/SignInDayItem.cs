using System;
using Gameplay.Script.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class SignInDayItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text dayName;
        [SerializeField] private TMP_Text dayAwardsName;
        [SerializeField] private TMP_Text dayAwardsCount;
        [SerializeField] private Image dayAwardsIcon;
        [SerializeField] private Button signInBtn;
        [SerializeField] private GameObject isSignInObj;
        [SerializeField] private GameObject isFocusObj;
        private Action<string, SignInInherentData> _callback;
        private SignInInherentData _data;
        private bool _isSignIn;
        private DateTime _today;
        public void InitItem(SignInInherentData data, DateTime today, bool isSignIn,
            Action<string, SignInInherentData> callback)
        {
            _callback = callback;
            _data = data;
            _today = today;
            _isSignIn = isSignIn;
            dayName.text = $"Day {data.day}";
            dayAwardsName.text = $"{data.awardsName}";
            dayAwardsCount.text = $"{data.awardsCount}";
            var iconAsset = UIAssetsBindData.Instance.GetAwardIconAsset(data.awardsId);
            dayAwardsIcon.sprite = iconAsset.awardIcon;
            signInBtn.interactable = _today >= DateTime.Today;
            bool isToday = _today == DateTime.Today;
            isFocusObj.gameObject.SetActive(isToday);
            if (isToday)
                isFocusObj.GetComponentInChildren<TMP_Text>().text = "今日奖励";
            isSignInObj.SetActive(isSignIn);
        }

        public void OnClickSignIn()
        {
            string message = "";
            if (_isSignIn)
                message = "请勿重复签到";
            if (_today != DateTime.Today)
                message = "不可签到";
            if (string.IsNullOrEmpty(message))
                isSignInObj.SetActive(true);
            _callback?.Invoke(message, _data);
        }
    }
}