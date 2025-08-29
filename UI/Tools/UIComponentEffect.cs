using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIComponentEffect : MonoBehaviour
{
    [SerializeField] private GameObject normal;
    [SerializeField] private GameObject heighlight;
    [SerializeField] private GameObject pressed;
    [SerializeField] private GameObject toggleon;
    private void Start()
    {
        EventTrigger trigger;
        if (!gameObject.GetComponent<EventTrigger>())
            gameObject.AddComponent<EventTrigger>();
        trigger = transform.GetComponent<EventTrigger>();
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener(OnPointEnter);
        trigger.triggers.Add(enter);
        
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener(OnPointExit);
        trigger.triggers.Add(exit);
        
        EventTrigger.Entry down = new EventTrigger.Entry();
        down.eventID = EventTriggerType.PointerDown;
        down.callback.AddListener(OnPointDown);
        trigger.triggers.Add(down);
        
        EventTrigger.Entry up = new EventTrigger.Entry();
        up.eventID = EventTriggerType.PointerUp;
        up.callback.AddListener(OnPointUp);
        trigger.triggers.Add(up);
    }

    private void Update()
    {
        if (gameObject.GetComponent<Toggle>())
        {
            OnToggle(gameObject.GetComponent<Toggle>().isOn);
        }
    }

    private void OnEnable()
    {
        if(normal)
            normal.SetActive(true);
        if(heighlight)
            heighlight.SetActive(false);
        if(pressed)
            pressed.SetActive(false);
        if (toggleon)
            toggleon.SetActive(false);
    }

    private void OnToggle(bool b)
    {
        if (toggleon)
            toggleon.SetActive(b);
    }
    
    private void OnPointUp(BaseEventData arg0)
    {
        if(pressed)
            pressed.SetActive(false);
    }

    private void OnPointDown(BaseEventData arg0)
    {
        if(pressed)
            pressed.SetActive(true);
    }

    private void OnPointExit(BaseEventData arg0)
    {
        if(heighlight)
            heighlight.SetActive(false);
    }

    private void OnPointEnter(BaseEventData arg0)
    {
        if(heighlight)
            heighlight.SetActive(true);
    }
}
