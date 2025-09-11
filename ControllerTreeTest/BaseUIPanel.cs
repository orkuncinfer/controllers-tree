using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Game.Services;
using UnityEngine.UI;

namespace Game.Views.UI
{
    public abstract class BaseUIPanel : MonoBehaviour, IUIPanel
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.3f;
        
        private readonly CompositeDisposable _disposables = new();
        private Tween _currentTween;
        
        public GameObject GameObject => gameObject;
        public bool IsVisible => gameObject.activeSelf;
        
        protected virtual void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            
            InitializePanel();
        }
        
        protected virtual void InitializePanel()
        {
            // Override in derived classes for initialization
        }
        
        protected virtual void OnDestroy()
        {
            CleanupPanel();
            _disposables.Dispose();
            KillTween();
        }
        
        protected virtual void CleanupPanel()
        {
            // Override in derived classes for cleanup
        }
        
        public virtual async UniTask ShowAsync(CancellationToken cancellationToken)
        {
            KillTween();
            
            gameObject.SetActive(true);
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            
            _currentTween = _canvasGroup.DOFade(1f, _fadeDuration)
                .SetUpdate(UpdateType.Normal, true)
                .OnComplete(() =>
                {
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                });
            
            await _currentTween.WithCancellation(cancellationToken);
        }
        
        public virtual async UniTask HideAsync(CancellationToken cancellationToken)
        {
            KillTween();
            
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            
            _currentTween = _canvasGroup.DOFade(0f, _fadeDuration)
                .SetUpdate(UpdateType.Normal, true)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
            
            await _currentTween.WithCancellation(cancellationToken);
        }
        
        protected void RegisterButtonListener(Button button, Action callback)
        {
            if (button == null) return;
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => callback?.Invoke());
            
            _disposables.Add(new DisposableAction(() => 
            {
                if (button != null)
                    button.onClick.RemoveAllListeners();
            }));
        }
        
        private void KillTween()
        {
            _currentTween?.Kill();
            _currentTween = null;
        }
        
        private class DisposableAction : IDisposable
        {
            private readonly Action _action;
            
            public DisposableAction(Action action)
            {
                _action = action;
            }
            
            public void Dispose()
            {
                _action?.Invoke();
            }
        }
        
        private class CompositeDisposable : IDisposable
        {
            private readonly List<IDisposable> _disposables = new();
            
            public void Add(IDisposable disposable)
            {
                _disposables.Add(disposable);
            }
            
            public void Dispose()
            {
                foreach (var disposable in _disposables)
                {
                    disposable?.Dispose();
                }
                _disposables.Clear();
            }
        }
    }
}