using System;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class GameLevelEnemyItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text enemyName;
        [SerializeField] private Image enemyIcon;
        [SerializeField] private Image enemyCardBackground;
        [SerializeField] private Image enemyGlow;
        [SerializeField] private Image enemyGradient;
        [SerializeField] private Image enemyBorder;
        [SerializeField] private Transform enemyGradeParent;
        public void InitItem(string enemyId, Action<EnemyInherentData> onClick)
        {
            var enemyData = GameLevelLogic.Instance.GetEnemy(enemyId);
            enemyName.text = enemyData.name;
            enemyIcon.sprite = UIAssetsBindData.Instance.GetEnemyIconAsset(enemyId).enemyIcon;
            var maxLevel = GameLevelLogic.Instance.GetGameLevelEnemyLevel(enemyId);
            var levelEnemy = GameLevelLogic.Instance.GetLevelEnemy(enemyId, maxLevel);
            int grade = levelEnemy.grade;
            for (int i = 0; i < enemyGradeParent.childCount; i++)
                enemyGradeParent.GetChild(i).GetChild(0).gameObject.SetActive(i < grade);
        }

        public void OnClickEnemy()
        {
            
        }
    }
}