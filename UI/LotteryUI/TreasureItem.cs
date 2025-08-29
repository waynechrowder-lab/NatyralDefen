using System;
using Gameplay.Script.Data;
using Gameplay.Script.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.UI.LotteryUI
{
    public class TreasureItem : MonoBehaviour
    {
        [SerializeField] string treasureId;
        public void InitItem(TreasureType type, string treasureId)
        {
            this.treasureId = treasureId;
            var treasure = LotteryLogic.Instance.GetTreasure(type, treasureId);
            GetComponentInChildren<TMP_Text>().text = treasure.count.ToString();
            var sprite = UIAssetsBindData.Instance.GetAwardIconAsset(treasure.id).awardIcon;
            GetComponentInChildren<Image>().sprite = sprite;
        }
    }
}