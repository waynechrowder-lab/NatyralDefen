using System;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using UnityEngine;

namespace Gameplay.Script.UI
{
    public class SignInUI : BasedUI
    {
        [SerializeField] private Transform signInDayParent;
        private GameObject _signInDay;

        private void Awake()
        {
            _signInDay = signInDayParent.GetChild(0).gameObject;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            PrepareSignInData();
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
        }

        void PrepareSignInData()
        {
            var inherentDataList = SignInLogic.Instance.GetSignInDataList();
            var userDataList = SignInLogic.Instance.GetUserSignInList();
            var now = DateTime.Today;
            var dayOne = new DateTime(now.Year, now.Month, 1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            var signInItems = signInDayParent.GetComponentsInChildren<SignInDayItem>(true);
            for (int i = 0; i < days; i++)
            {
                var day = dayOne + new TimeSpan(i, 0, 0, 0);
                var inhereData = i < inherentDataList.Count ? inherentDataList[i] : inherentDataList[^1];
                bool isSignIn = userDataList.Contains(day.ToString("yyyy-MM-dd"));
                GameObject item;
                if (i < signInItems.Length)
                    item = signInItems[i].gameObject;
                else
                    throw new Exception($"{i} index out of signInDayParent");
                item.SetActive(true);
                item.GetComponent<SignInDayItem>().InitItem(inhereData, day, isSignIn, OnClickSignIn);
            }

            for (int i = days; i < signInItems.Length; i++)
                signInItems[i].gameObject.SetActive(false);
        }

        private void OnClickSignIn(string message, SignInInherentData data)
        {
            if (string.IsNullOrEmpty(message))
                SignInLogic.Instance.SignIn(data);
            else
                Debug.Log($"sign in error : {message}");
        }
    }
}