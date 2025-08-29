using System;
using UnityEngine;

namespace Gameplay.Script.UI
{
    public class AutoHideUI : MonoBehaviour
    {
        [SerializeField] private float hideAway = 3;
        [SerializeField] private float fadeDuration = 0.5f;

        private RectTransform _uiRect;
        private Camera _camera;
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;

        private float _targetAlpha = 1f;
        private float _currentAlphaVelocity;

        private void Start()
        {
            _canvas = GetComponentInChildren<Canvas>();
            _uiRect = _canvas.GetComponent<RectTransform>();
            _camera = Camera.main;
            if (!_canvas.TryGetComponent(out _canvasGroup))
                _canvasGroup = _canvas.gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvas.enabled = false;
        }

        private void Update()
        {
            Vector3 uiPosition = _uiRect.position;
            Vector3 cameraPosition = _camera.transform.position;
            uiPosition.y = 0;
            cameraPosition.y = 0;
            float horizontalDistance = Vector3.Distance(uiPosition, cameraPosition);
            
            _targetAlpha = horizontalDistance < hideAway ? 1 : 0;
            _canvasGroup.alpha = Mathf.SmoothDamp(
                _canvasGroup.alpha, _targetAlpha, ref _currentAlphaVelocity, fadeDuration);
            _canvasGroup.interactable = _canvasGroup.alpha > 0.01f;
            _canvasGroup.blocksRaycasts = _canvasGroup.alpha > 0.01f;
            _canvas.enabled = _canvasGroup.alpha > 0.01f;
        }

    }
}