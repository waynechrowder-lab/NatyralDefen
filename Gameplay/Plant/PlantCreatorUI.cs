using System.Collections.Generic;
using Gameplay.Script.MiniWorldGameplay;
using TMPro;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class PlantCreatorUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private Transform plantItemParent;
        private GameObject _plantItem;

        public void Update()
        {
            coinText.text = GameplayLogic.Instance.SunCoin.ToString();
        }

        public void Init(List<int> list)
        {
            PrepareItem(list.Count);
            SetItem(list);
        }

        void PrepareItem(int count)
        {
            for (int i = 0; i < plantItemParent.childCount; i++)
                plantItemParent.GetChild(i).gameObject.SetActive(i < count);
            for (int i = plantItemParent.childCount; i < count; i++)
            {
                if (!_plantItem) _plantItem = plantItemParent.GetChild(0).gameObject;
                Instantiate(_plantItem, plantItemParent).SetActive(true);
            }
        }

        void SetItem(List<int> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                plantItemParent.GetChild(i).GetComponent<PlantItem>().InitItem(list[i], cost =>
                {
                    GameplayLogic.Instance.CoinChanged(-cost);
                });
            }
        }
    }
}