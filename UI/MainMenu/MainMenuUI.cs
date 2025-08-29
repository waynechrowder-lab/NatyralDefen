using Gameplay.Script.Gameplay;
using Gameplay.Script.Logic;
using Gameplay.Script.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Script.Bmob;
using Gameplay.Script.Data;
using Gameplay.Script.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : BasedUI
{
    [SerializeField] GameLevelBagUI gameLevelBag;

    [SerializeField] GameObject[] panels;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text currentExpText;
    [SerializeField] private TMP_Text uuidText;
    [SerializeField] private Slider expSlider;

    [SerializeField] private GameObject[] rightPanels;
    [SerializeField] private GameObject noticePanel;

    protected override void OnEnable()
    {
        base.OnEnable();
        UserDataManager.Instance.RegisterUserExpendCallback(OnExpend);
        var userData = UserDataManager.Instance.UserDataJson;
        var user = UserData.Instance.CurrentUser;
        coinText.text = userData.coin.ToString();
        nameText.text = user.displayName;
        expText.text = "1";
        currentExpText.text = $"{userData.exp}/1000";
        uuidText.text = $"#{BmobManager.Instance.MiniWorldUser.uuid}";
        expSlider.value = 1;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UserDataManager.Instance.UnRegisterUserExpendCallback(OnExpend);
    }

    private void OnExpend(UserDataJson userData)
    {
        coinText.text = userData.coin.ToString();
    }

    public void OnClickStartGame(int mode)
    {
        var gameMode = (QuickGameMode)mode;
        GameLevelLogic.Instance.SetQuickGameMode(gameMode);
        gameLevelBag.gameObject.SetActive(true);
    }

    public void OnClickBottomMenuButton(int index)
    {
        gameLevelBag.gameObject.SetActive(false);
        panels.ToList().ForEach(panel => panel.SetActive(false));
        if (index < panels.Length && index >= 0)
            panels[index].SetActive(true);
    }
    
    public void OnClickRightMenuButton(int index)
    {
        noticePanel.gameObject.SetActive(index == 0);
        rightPanels.ToList().ForEach(panel => panel.SetActive(false));
        if (index < rightPanels.Length && index >= 0)
            rightPanels[index].SetActive(true);
    }
}
