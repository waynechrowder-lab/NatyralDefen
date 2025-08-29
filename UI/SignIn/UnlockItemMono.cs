using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.UI
{
    public class UnlockItemMono : MonoBehaviour
    {
        [SerializeField] private TMP_Text unlockName;
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private TMP_Text itemDescription;
        [SerializeField] private TMP_Text unlockDay;
        [SerializeField] private Image itemIcon;
        public void InitItem(MiniWorldUnlockItemData itemData, 
            UnlockItem unlockItem)
        {
            unlockName.text = itemData.names;
            var id = itemData.itemIds;
            var plantInherentData = PlantData.Instance.GetPlantInherentData(id);
            if (plantInherentData == null)
            {
                gameObject.SetActive(false);
                return;
            }
            itemName.text = plantInherentData.name;
            itemDescription.text = plantInherentData.description;

            var days = new List<string>();
            if (unlockItem is { unlockDays: not null })
                days = unlockItem.unlockDays.Split(',').ToList();
            if (itemData.needTime.Get() == 0)
                unlockDay.text = unlockItem is { unlocked: true } ? "已拥有" : "未拥有";
            else
                unlockDay.text = $"{days?.Count ?? 0}/{itemData.needTime.Get()}";
            var asset = UIAssetsBindData.Instance.GetPlantIconAsset(id);
            itemIcon.sprite = asset.plantIcon;
            var btn = GetComponent<Button>();
            var day = DateTime.Today;
            var dayStr = day.ToString("yyyy-MM-dd");
            var userCount = days?.Count ?? 0;
            if (days.Contains(dayStr) || userCount >= itemData.needTime.Get() || unlockItem.unlocked)
            {
                btn.interactable = false;
                return;
            }
            btn.interactable = true;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (itemData.needTime.Get() == 0) return;
                day = DateTime.Today;
                dayStr = day.ToString("yyyy-MM-dd");
                if (!days.Contains(dayStr))
                {
                    days.Add(dayStr);
                    userCount = days?.Count ?? 0;
                    if (unlockItem == null)
                    {
                        unlockItem = new UnlockItem()
                        {
                            id = itemData.id.Get(),
                            unlockDays = string.Empty,
                            unlocked = userCount >= itemData.needTime.Get()
                        };
                    }
                    unlockItem.unlockDays = string.Join(",", days.ToArray());
                    unlockItem.unlocked = days.Count >= itemData.needTime.Get();
                    unlockDay.text = $"{userCount}/{itemData.needTime.Get()}";
                    UserDataManager.Instance.UpdateUnlockItem(unlockItem);
                    if (unlockItem.unlocked)
                    {
                        UserDataManager.Instance.AddPlant(id);
                    }
                    btn.interactable = false;
                }
            });
        }
    }
}