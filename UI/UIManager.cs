using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Currency.Core.Run;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class UIManager : MonoSingle<UIManager>
{
    [SerializeField] private Transform uiParent;
    [SerializeField] private string uiDir;
    private static Dictionary<string, GameObject> _uiMgr = new Dictionary<string, GameObject>();
    
    protected override void Awake()
    {
        base.Awake();
        _uiMgr.Clear();
    }

    public bool UIObjectActive(string uiName)
    {
        if (!_uiMgr.ContainsKey(uiName))
            return false;
        return _uiMgr[uiName].activeInHierarchy;
    }
    
    public void ShowUI(string uiName, bool active, bool instant = true, string assetName = null)
    {
        if (!active && !instant && !_uiMgr.ContainsKey(uiName))
            return;
        if (string.IsNullOrEmpty(assetName))
            GetUIObject(uiName);
        else 
            GetUIObject(uiName, assetName);
        if (_uiMgr[uiName].activeSelf != active) 
            _uiMgr[uiName].SetActive(active);
    }

    public void UnLoadUI(string uiName)
    {
        if (_uiMgr.TryGetValue(uiName, out var value))
        {
            DestroyImmediate(value);
            _uiMgr.Remove(uiName);
        }
    }
    
    // public void ShowUI(string uiName, Vector3 pos, Quaternion rot)
    // {
    //     GetUIObject(uiName);
    //     _uiMgr[uiName].transform.position = pos;
    //     _uiMgr[uiName].transform.rotation = rot;
    //     if (!_uiMgr[uiName].activeSelf)
    //         _uiMgr[uiName].SetActive(true);
    // }

    public GameObject GetUIObject(string uiName)
    {
        if (!_uiMgr.ContainsKey(uiName))
        {
            GameObject res = Resources.Load<GameObject>(
                new StringBuilder(uiDir).Append(uiName).ToString());
            if (!res)
                throw new Exception($"no ui name : {uiName}");
            GameObject ui = Instantiate(res, uiParent);
            _uiMgr.Add(uiName, ui);
        }
        return _uiMgr[uiName];
    }
    
    public GameObject GetUIObject(string uiName, string assetName)
    {
        if (!_uiMgr.ContainsKey(uiName))
        {
            GameObject res = Resources.Load<GameObject>(
                new StringBuilder(uiDir).Append(assetName).ToString());
            if (!res)
                throw new Exception($"no ui name : {uiName}");
            GameObject ui = Instantiate(res, uiParent);
            _uiMgr.Add(uiName, ui);
        }
        return _uiMgr[uiName];
    }

    public T GetUIComponent<T>()
    {
        return GetUIObject(typeof(T).ToString()).GetComponent<T>();
    }
    
}
