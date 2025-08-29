using System;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class PlantUpgradeUIMono : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Transform gradeParent;
        [SerializeField] private float canvasActiveTime = 2;
        private float _timer = 0;

        private void Start()
        {
            if (canvasGroup)
                canvasGroup.alpha = 0;
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            _timer = Mathf.Max(0, _timer);
            if (canvasGroup)
                canvasGroup.alpha = Mathf.Clamp01(_timer);
        }

        public void DoUpgrade(int? currentGrade = null)
        {
            _timer = canvasActiveTime;
            if (currentGrade is > 0)
            {
                var childCount = Mathf.Min(currentGrade.Value, gradeParent.childCount);
                gradeParent.GetChild(childCount - 1).gameObject.SetActive(true);
            }
        }
    }
}