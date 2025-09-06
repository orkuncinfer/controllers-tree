using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Game.Services;

namespace Game.Views.UI
{
    public abstract class BaseUIPanel : MonoBehaviour, IUIPanel
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.3f;
        
        public GameObject GameObject => gameObject;
        
        protected virtual void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }
        
        public virtual async UniTask ShowAsync(CancellationToken cancellationToken)
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            
            await _canvasGroup.DOFade(1f, _fadeDuration)
                .WithCancellation(cancellationToken);
            
            _canvasGroup.interactable = true;
        }
        
        public virtual async UniTask HideAsync(CancellationToken cancellationToken)
        {
            _canvasGroup.interactable = false;
            
            await _canvasGroup.DOFade(0f, _fadeDuration)
                .WithCancellation(cancellationToken);
            
            gameObject.SetActive(false);
        }
    }
}