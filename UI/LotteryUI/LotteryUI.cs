using System.Linq;
using UnityEngine;

namespace Gameplay.Script.UI.LotteryUI
{
    public class LotteryUI : BasedUI
    {
        [SerializeField] private Transform treasureParent;
        protected override void OnEnable()
        {
            base.OnEnable();
            SetTreasureItem();
        }

        void SetTreasureItem()
        {
            var lotteryList = treasureParent.GetComponentsInChildren<LotteryItem>().ToList();
            lotteryList.ForEach(value => value.InitItem());
        }
    }
}