using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

[DisallowMultipleComponent]
public class SequentialHideXR : MonoBehaviour
{
    [Header("��������")]
    [Tooltip("�Ƿ� Hierarchy ����˳������")]
    public bool sortByHierarchy = true;

    // ����������
    private static HideManager _manager;

    void Awake()
    {
        if (_manager == null)
        {
            var go = new GameObject("SequentialHideXR_Manager");
            DontDestroyOnLoad(go);
            _manager = go.AddComponent<HideManager>();
            _manager.sortByHierarchy = sortByHierarchy;
        }
    }

    private class HideManager : MonoBehaviour
    {
        [HideInInspector] public bool sortByHierarchy;

        private List<GameObject> _targets;
        private int _currentIndex;
        private bool _prevPressed;

        void Start()
        {
            // �ռ����йҽű��� GameObject������ inactive��
            _targets = FindObjectsOfType<SequentialHideXR>(true)
                .Select(s => s.gameObject)
                .ToList();

            if (sortByHierarchy)
            {
                _targets = _targets
                    .OrderBy(go => GetHierarchyPath(go.transform))
                    .ToList();
            }
        }

        void Update()
        {
            bool pressed = false;

            // Pico4UE�����ֱ� primaryButton��A ����
            var rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (rightHand.isValid)
            {
                rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out pressed);
            }

            // �༭�����ԣ�F1 ��
            if (Input.GetKeyDown(KeyCode.F1))
                pressed = true;

            // ��⵽��δ���µ����µ�˲��
            if (pressed && !_prevPressed)
            {
                if (_currentIndex < _targets.Count)
                {
                    _targets[_currentIndex].SetActive(false);
                    _currentIndex++;
                }
            }

            _prevPressed = pressed;
        }

        // ���������ɿɱȽϵ� Hierarchy ·���ַ���
        private static string GetHierarchyPath(Transform t)
        {
            var indices = new List<int>();
            while (t != null)
            {
                indices.Add(t.GetSiblingIndex());
                t = t.parent;
            }
            indices.Reverse();
            return string.Join(",", indices);
        }
    }
}
