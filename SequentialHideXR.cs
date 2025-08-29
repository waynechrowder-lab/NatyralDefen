using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

[DisallowMultipleComponent]
public class SequentialHideXR : MonoBehaviour
{
    [Header("排序设置")]
    [Tooltip("是否按 Hierarchy 面板的顺序隐藏")]
    public bool sortByHierarchy = true;

    // 管理器单例
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
            // 收集所有挂脚本的 GameObject（包含 inactive）
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

            // Pico4UE：右手柄 primaryButton（A 键）
            var rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (rightHand.isValid)
            {
                rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out pressed);
            }

            // 编辑器测试：F1 键
            if (Input.GetKeyDown(KeyCode.F1))
                pressed = true;

            // 检测到从未按下到按下的瞬间
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

        // 辅助：生成可比较的 Hierarchy 路径字符串
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
