using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using Script.Core.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Gameplay.Script.UI.LotteryUI
{
    public class LotteryItem : MonoBehaviour
    {
        [SerializeField] private TreasureType treasureType;
        [SerializeField] private int cost;

        [SerializeField] TMP_Text costText;
        [SerializeField] private Transform rotateTrans;
        private List<string> _treasureList;
        private int _count = 0;
        private void OnEnable()
        {
            StoreLogic.Instance.RegisterStoreListUpdate(OnGetGoodsItem);
        }

        private void OnDisable()
        {
            StoreLogic.Instance.UnRegisterStoreListUpdate(OnGetGoodsItem);
        }
        
        private void OnGetGoodsItem(Data.GoodsItem goodsItem)
        {
            _count = LotteryLogic.Instance.GetLotteryCount(treasureType);
            costText.text = _count > 0 ? $"x{_count}" : cost.ToString();
        }

        public void InitItem()
        {
            _count = LotteryLogic.Instance.GetLotteryCount(treasureType);
            costText.text = _count > 0 ? $"x{_count}" : cost.ToString();
            var treasureItemList = GetComponentsInChildren<TreasureItem>().ToList();
            _treasureList = LotteryLogic.Instance.GetTreasureList(treasureType);
            int i = 0;
            treasureItemList.ForEach(value =>
            {
                value.InitItem(treasureType, _treasureList[i]);
                i++;
            });
        }

        public void OnClickLottery()
        {
            if (LotteryLogic.Instance.CanCost(cost) || _count > 0)
            {
                GetComponentInChildren<Button>().interactable = false;
                StartLottery();
                _count = LotteryLogic.Instance.GetLotteryCount(treasureType);
                costText.text = _count > 0 ? $"x{_count}" : cost.ToString();
                PlayerPrefs.DeleteKey($"{treasureType.ToString()}");
            }
        }

        void StartLottery()
        {
            var randomRotate = Random.Range(360 * 5, 360 * 6);
            var currentZRotation = rotateTrans.localRotation.eulerAngles.z % 360;
            if (currentZRotation < 0) currentZRotation += 360;

            var targetRotate = randomRotate + currentZRotation;
            var targetRotateAbs = targetRotate % 360;
            while (targetRotateAbs > 0)
                targetRotateAbs -= 360;
            targetRotateAbs -= 22.5f;
            int index = 0;
            while (true)
            {
                if (targetRotateAbs > (-index - 1) * 45) break;   
                index++;
                if (index >= 7) break;
            }
            Debug.Log($"targetRotate : {targetRotate},treasureIndex : {index}");
            string treasureId = _treasureList[index];
            LotteryLogic.Instance.SaveTreasure(cost, treasureType, treasureId);
            StartCoroutine(RotateToTarget(targetRotate, 3f));
            // Quaternion quaternion = Quaternion.Euler(0, 0, targetRotate);
            // rotateTrans.DOLocalRotateQuaternion(quaternion, 3).SetEase(Ease.InOutQuint).OnComplete(() =>
            // {
            //     GetComponentInChildren<Button>().interactable = true;
            // });
        }
        
        private IEnumerator RotateToTarget(float targetAngle, float duration)
        {
            float startAngle = rotateTrans.localEulerAngles.z;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                t = EaseInOutQuint(t); // 自定义缓动函数
                float currentAngle = Mathf.Lerp(startAngle, targetAngle, t);
                rotateTrans.localRotation = Quaternion.Euler(0, 0, currentAngle);
                yield return null;
            }
            rotateTrans.localRotation = Quaternion.Euler(0, 0, targetAngle);
            yield return new WaitForSeconds(3);
            InitItem();
            GetComponentInChildren<Button>().interactable = true;
        }

        private float EaseInOutQuint(float t)
        {
            return t < 0.5f ? 16f * t * t * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 5f) / 2f;
        }

    }
}