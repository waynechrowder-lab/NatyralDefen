using Gameplay.Script.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Script.Gameplay
{
    public class ZombieHUD : MonoBehaviour
    {
        [SerializeField] private Transform leftParent;
        [SerializeField] private Transform rightParent;
        [SerializeField] private GameObject moveIcon;
        [SerializeField] private float iconMoveDuration = 1.5f;
        [Header("高级僵尸出现音效")]
        [SerializeField] private AudioClip seniorZombieAppearClip;
        private AudioSource _audioSource;
        private bool _seniorAudioPlayed = false;

        private Camera _targetCamera;
        List<ZombieBehaviour> _zombieBehaviours;

        private void Start()
        {
            moveIcon.SetActive(false);
            _targetCamera = Camera.main;
            _audioSource = gameObject.GetComponent<AudioSource>();
            if (_audioSource != null)
                _audioSource.playOnAwake = false;
            EventDispatcher.Instance.Register((int)EventID.GAMEOVER, OnGameOver);
        }

        private void OnDestroy()
        {
            EventDispatcher.Instance.UnRegister((int)EventID.GAMEOVER, OnGameOver);
        }

        public void SetZombieList(List<ZombieBehaviour> zombies)
        {
            _zombieBehaviours = zombies;
            StopCoroutine(nameof(CheckVisibilityCoroutine));
            StartCoroutine(nameof(CheckVisibilityCoroutine));
        }

        public void OnZombieSpawned(ZombieAsset asset, ZombieBehaviour behaviour)
        {
            if (asset.zombieType == ZombieType.高级 && !_seniorAudioPlayed && _audioSource != null)
            {
                _audioSource.PlayOneShot(seniorZombieAppearClip);
                _seniorAudioPlayed = true;
            }
        }

        private void OnGameOver(GameEventArg arg)
        {
            StopAllCoroutines();
            if (leftParent != null)
            {
                for (int i = 0; i < leftParent.childCount; i++)
                    leftParent.GetChild(i).gameObject.SetActive(false);
            }
            if (rightParent != null)
            {
                for (int i = 0; i < rightParent.childCount; i++)
                    rightParent.GetChild(i).gameObject.SetActive(false);
            }
            _seniorAudioPlayed = false;
        }

        IEnumerator CheckVisibilityCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(.05f);
                int left = 0, right = 0;
                for (int i = 0; i < _zombieBehaviours.Count; i++)
                {
                    if (_zombieBehaviours[i])
                    {
                        if (IsObjectVisible(_zombieBehaviours[i].transform))
                        {
                            _zombieBehaviours[i].SetVisibility((side, target) =>
                            {
                                var obj = Instantiate(moveIcon);
                                obj.SetActive(true);
                                obj.transform.position = side > 0 ? rightParent.position : leftParent.position;
                                obj.GetComponent<ZombieIcon>().Init(side, target, iconMoveDuration);
                            });
                            _zombieBehaviours[i].SetVisibilitySide(0);
                        }
                        else if (!_zombieBehaviours[i].IsVisibility)
                        {
                            Vector3 a = _targetCamera.transform.right;
                            Vector3 b = _zombieBehaviours[i].transform.position - _targetCamera.transform.position;
                            a.y = b.y = 0;
                            float dot = Vector3.Dot(a.normalized, b.normalized);
                            if (dot > 0) right++;
                            else left++;
                            _zombieBehaviours[i].SetVisibilitySide(dot > 0 ? 1 : -1);
                        }
                    }
                }
                for (int i = 0; i < leftParent.childCount; i++)
                    leftParent.GetChild(i).gameObject.SetActive(i < left);
                for (int i = leftParent.childCount; i < left; i++)
                    Instantiate(leftParent.GetChild(0).gameObject, leftParent).SetActive(true);
                for (int i = 0; i < rightParent.childCount; i++)
                    rightParent.GetChild(i).gameObject.SetActive(i < right);
                for (int i = rightParent.childCount; i < right; i++)
                    Instantiate(rightParent.GetChild(0).gameObject, rightParent).SetActive(true);
            }
        }

        bool IsObjectVisible(Transform target)
        {
            Renderer renderer = target.GetComponentInChildren<Renderer>();
            if (renderer == null)
                return false;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_targetCamera);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }
    }
}
