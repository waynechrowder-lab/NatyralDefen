using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomToggleHandler : MonoBehaviour
{
    [SerializeField] GameObject[] graphics;
    [SerializeField] Color onColor = Color.white;
    [SerializeField] Color offColor = Color.gray;
    Toggle _toggle;
    private void Update()
    {
        if (_toggle == null)
            _toggle = GetComponent<Toggle>();
        if (_toggle != null && graphics != null && graphics.Length > 0)
        {
            graphics.ToList().ForEach(value =>
            {
                if (value.TryGetComponent(out TMP_Text txt))
                {
                    txt.color = _toggle.isOn ? onColor : offColor;
                }
                else if (value.TryGetComponent(out Image img))
                {
                    img.color = _toggle.isOn ? onColor : offColor;
                }
            });
        }
    }
}

[System.Serializable]
public class HandlerGroup
{
    public GameObject[] graphics;
    public Color onColor = Color.white;
    public Color offColor = Color.gray;
}