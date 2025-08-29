using Gameplay.Script.Bmob;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Script.Gameplay
{
    public class MissionItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text description;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text sliderText;
        [SerializeField] private GameObject button;
        [SerializeField] private AwardItem award;
        public void InitItem(MiniWorldActivity activity)
        {
            
        }
    }
}