using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Game.Views.UI
{
    public sealed class LoadingPanel : BaseUIPanel
    {
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TMPro.TextMeshProUGUI _loadingText;
        [SerializeField] private TMPro.TextMeshProUGUI _percentageText;
        
        private float _targetProgress;
        private float _currentProgress;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Start with panel visible
            gameObject.SetActive(true);
            var canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            
            if (_loadingText) _loadingText.text = "LOADING...";
            if (_progressBar) _progressBar.value = 0f;
            if (_percentageText) _percentageText.text = "0%";
        }
        
        public void SetProgress(float progress)
        {
            _targetProgress = Mathf.Clamp01(progress);
        }
        
        private void Update()
        {
            if (_progressBar && _currentProgress != _targetProgress)
            {
                _currentProgress = Mathf.Lerp(_currentProgress, _targetProgress, Time.deltaTime * 5f);
                _progressBar.value = _currentProgress;
                
                if (_percentageText)
                {
                    _percentageText.text = $"{Mathf.RoundToInt(_currentProgress * 100)}%";
                }
            }
        }
        
        public override async UniTask ShowAsync(CancellationToken cancellationToken)
        {
            // Already visible at start
            await UniTask.CompletedTask;
        }
        
        public override async UniTask HideAsync(CancellationToken cancellationToken)
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            
            // Make sure progress is at 100% before hiding
            if (_progressBar)
            {
                await _progressBar.DOValue(1f, 0.3f)
                    .WithCancellation(cancellationToken);
            }
            
            await UniTask.Delay(200, cancellationToken: cancellationToken);
            
            await canvasGroup.DOFade(0f, 0.5f)
                .WithCancellation(cancellationToken);
            
            gameObject.SetActive(false);
        }
    }
}