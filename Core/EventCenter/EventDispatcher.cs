using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Currency.Core.Run;

public class EventDispatcher : Single<EventDispatcher>
{
    Dictionary<int, GameEvent> mEvents = new Dictionary<int, GameEvent>();
    public delegate void EventListernerType(GameEventArg arg);
    GameEvent GetEvent(int id)
    {
        if (!mEvents.ContainsKey(id))
        {
            mEvents[id] = new GameEvent();
        }
        return mEvents[id];
    }
    public GameEventArg GetEventArg(int id)
    {
        if (!mEvents.ContainsKey(id))
        {
            mEvents[id] = new GameEvent();
        }
        return mEvents[id].GetEventArg();
    }
    public void Dispatch(int id)
    {
        GameEvent evt = GetEvent(id);
        evt.Dispatch();
    }
    public void Register(int id, EventListernerType onCallBack)
    {
        GameEvent evt = GetEvent(id);
        evt.RegisterEvent(onCallBack);
    }
    public void UnRegister(int id, EventListernerType onCallBack)
    {
        GameEvent evt = GetEvent(id);
        evt.UnRegisterEvent(onCallBack);
    }

    public void OnDestroy()
    {
        mEvents.Clear();
    }

}
