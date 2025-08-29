using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class ZombieAnimator : MonoBehaviour
    {
        private Animator _animator;
        
        private void Start()
        {
            _animator = GetComponent<Animator>();
            _animator.speed = 1;
        }
        
        public void StartAnimation(ZombieState zombieState)
        {
            if (_animator)
            {
                switch (zombieState)
                {
                    case ZombieState.Idle:
                        if (HasParameter("ZIdle"))
                            _animator.SetTrigger("ZIdle");
                        break;
                    case ZombieState.Attacking:
                        string randomAttack = Random.Range(0, 10) < 5 ? "ZAttack1" : "ZAttack2";
                        if (HasParameter(randomAttack))
                            _animator.SetTrigger(randomAttack);
                        else
                            _animator.SetTrigger("ZAttack1");
                        break;
                    case ZombieState.UnderAttacking:
                        // _animator.SetTrigger(GetHit);
                        break;
                    case ZombieState.Dead:
                        if (HasParameter("ZDie"))
                            _animator.SetTrigger("ZDie");
                        break;
                    case ZombieState.DeadBomb:
                        if (HasParameter("ZBoomDie"))
                            _animator.SetTrigger("ZBoomDie");
                        break;
                    case ZombieState.Celebrate:
                        if (HasParameter("ZWin"))
                            _animator.SetTrigger("ZWin");
                        break;
                    case ZombieState.Moving:
                        if (HasParameter("ZWalk"))
                            _animator.SetTrigger("ZWalk");
                        break;
                }
            }
        }

        public void Init()
        {
            
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