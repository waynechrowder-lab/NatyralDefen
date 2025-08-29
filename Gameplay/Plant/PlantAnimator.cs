using System;
using System.Collections.Generic;
using Script.Core.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Script.Gameplay
{
    public class PlantAnimator : MonoBehaviour
    {
        private Animator _animator;
        private PlantState _plantState = PlantState.Idle;
        private AnimationEventTrigger _animationEvent;
        List<Action<string>> _actions = new();
        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            if (_animator && !_animator.TryGetComponent(out _animationEvent))
                _animationEvent = _animator.gameObject.AddComponent<AnimationEventTrigger>();
        }

        public void InitAnimator(Transform trans)
        {
            _animator = trans.GetComponentInChildren<Animator>();
            if (_animator && !_animator.TryGetComponent(out _animationEvent))
                _animationEvent = _animator.gameObject.AddComponent<AnimationEventTrigger>();
            foreach(var action in _actions)
            {
                _animationEvent.RegisterAnimationEvent(action);
            }
        }

        public void RegisterAnimationEvent(Action<string> action)
        {
            if (!_actions.Contains(action))
                _actions.Add(action);
            if (_animationEvent) _animationEvent.RegisterAnimationEvent(action);
        }

        // public void UnRegisterAnimationEvent(string eventName, Action<string> action)
        // {
        //     if (_animationEvent) _animationEvent.UnRegisterAnimationEvent(eventName, action);
        // }
        
        public void SetAnimatorActive(bool active)
        {
            if (_animator)
                _animator.enabled = active;
        }
        
        public void StartAnimation(PlantState zombieState, string customParameter = null)
        {
            if (_plantState >= PlantState.Dead)
                return;
            _plantState = zombieState;
            if (_animator)
            {
                if (!string.IsNullOrEmpty(customParameter) && HasParameter(customParameter))
                {
                    _animator.SetTrigger(customParameter);
                    return;
                }
                switch (zombieState)
                {
                    case PlantState.Idle:
                        if (HasParameter("Idle"))
                            _animator.SetTrigger("Idle");
                        break;
                    case PlantState.Attacking:
                        string randomAttack = Random.Range(0, 10) < 5 ? "Attack1" : "Attack2";
                        if (HasParameter(randomAttack))
                            _animator.SetTrigger(randomAttack);
                        else
                            _animator.SetTrigger("Attack1");
                        break;
                    case PlantState.UnderAttacking:
                        // _animator.SetTrigger(GetHit);
                        break;
                    case PlantState.Dead:
                        if (HasParameter("Die"))
                            _animator.SetTrigger("Die");
                        break;
                    case PlantState.Celebrate:
                        if (HasParameter("Win"))
                            _animator.SetTrigger("Win");
                        break;
                }
            }
        }
        
        public bool HasParameter(string paramName, 
            AnimatorControllerParameterType type = AnimatorControllerParameterType.Trigger)
        {
            if (_animator.parameterCount == 0) 
                return false;

            foreach (AnimatorControllerParameter param in _animator.parameters)
            {
                if (param.name == paramName && param.type == type)
                    return true;
            }
            return false;
        }

        public void SetSpeed(float f)
        {
            if (_animator) _animator.speed = f;
        }
    }
}