using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class AnimationEventTrigger : MonoBehaviour
    {
        List<Action<string>> _actions = new();
        
        public void RegisterAnimationEvent(Action<string> action)
        {
            if (!_actions.Contains(action))
                _actions.Add(action);
        }
        
        // public void UnRegisterAnimationEvent(string key, Action<string> action)
        // {
        //     if (_actions.TryGetValue(key, out var actions) )
        //     {
        //         actions.Remove(action);
        //     }
        // }
        
        public void PlayEventTrigger(string triggerName)
        {
            _actions.ForEach(value => value?.Invoke(triggerName));   
        }
    }
}