using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    Action _continueAction;

    public void OnClickBack()
    {
        SceneLoadManager.Instance.OnLoadScene(SceneLoadManager.SceneIndex.Lobby);
    }

    public void RegisterContinueAction(Action action)
    {
        _continueAction = action;
    }

    public void OnClickContinue()
    {
        _continueAction?.Invoke();
    }

    public void OnClickRetry()
    {
        SceneLoadManager.Instance.OnLoadScene(SceneLoadManager.SceneIndex.MRScene, true);
    }

}
