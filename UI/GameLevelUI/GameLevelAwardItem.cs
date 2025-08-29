using System;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class GameLevelAwardItem : MonoBehaviour
    {
        [SerializeField] private Image awardIcon;
        [SerializeField] private TMP_Text awardName;
        [SerializeField] private TMP_Text awardCount;
        public void InitItem(string awardId, Action<AwardInherentData> onClick)
        {
            var awardData = GameLevelLogic.Instance.GetAward(awardId);
            awardName.text = awardData.name;
            awardIcon.sprite = UIAssetsBindData.Instance.GetAwardIconAsset(awardId).awardIcon;
        }
        
        public void InitItem(Data.GoodsItem goods)
        {
            string awardId = goods.goodsId;
            int count = goods.goodsNum;
            var awardData = GameLevelLogic.Instance.GetAward(awardId);
            awardName.text = awardData.name;
            awardCount.text = count.ToString();
            awardIcon.sprite = UIAssetsBindData.Instance.GetAwardIconAsset(awardId).awardIcon;
        }

        public void OnClickAward()
        {
            
        }
    }
}