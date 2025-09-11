using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace Game.Views.UI
{
    public sealed class LoadingPanel : BaseUIPanel
    {
        [Header("Loading Components")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private TextMeshProUGUI _percentageText;
        [SerializeField] private Image _backgroundImage;
        
        [Header("Animation Settings")]
        [SerializeField] private float _progressSmoothSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 180f;
        [SerializeField] private Transform _spinnerIcon;
        
        private float _targetProgress;
        private float _currentProgress;
        private Tween _spinnerTween;
        
        protected override void InitializePanel()
        {
            base.InitializePanel();
            
            // Set initial state
            if (_loadingText != null) 
                _loadingText.text = "LOADING...";
            
            if (_progressBar != null) 
            {
                _progressBar.value = 0f;
                // Disable interactable for progress bar
                _progressBar.interactable = false;
            }
            
            if (_percentageText != null) 
                _percentageText.text = "0%";
            
            // Start spinner animation if exists
            if (_spinnerIcon != null)
            {
                StartSpinnerAnimation();
            }
            
            // Make background non-interactable but visible
            if (_backgroundImage != null)
            {
                _backgroundImage.raycastTarget = true; // Block input
            }
        }
        
        protected override void CleanupPanel()
        {
            base.CleanupPanel();
            StopSpinnerAnimation();
        }
        
        public void SetProgress(float progress)
        {
            _targetProgress = Mathf.Clamp01(progress);
        }
        
        public void SetLoadingText(string text)
        {
            if (_loadingText != null)
            {
                _loadingText.text = text;
            }
        }
        
        private void Update()
        {
            if (_progressBar != null && !Mathf.Approximately(_currentProgress, _targetProgress))
            {
                _currentProgress = Mathf.Lerp(_currentProgress, _targetProgress, Time.unscaledDeltaTime * _progressSmoothSpeed);
                _progressBar.value = _currentProgress;
                
                if (_percentageText != null)
                {
                    int percentage = Mathf.RoundToInt(_currentProgress * 100);
                    _percentageText.text = $"{percentage}%";
                }
            }
        }
        
        public override async UniTask ShowAsync(CancellationToken cancellationToken)
        {
            // Reset progress
            _currentProgress = 0f;
            _targetProgress = 0f;
            
            if (_progressBar != null)
                _progressBar.value = 0f;
            
            if (_percentageText != null)
                _percentageText.text = "0%";
            
            await base.ShowAsync(cancellationToken);
        }
        
        public override async UniTask HideAsync(CancellationToken cancellationToken)
        {
            // Animate progress to 100% before hiding
            if (_progressBar != null)
            {
                await _progressBar.DOValue(1f, 0.3f)
                    .SetUpdate(UpdateType.Normal, true)
                    .WithCancellation(cancellationToken);
                
                if (_percentageText != null)
                    _percentageText.text = "100%";
            }
            
            await UniTask.Delay(200, ignoreTimeScale: true, cancellationToken: cancellationToken);
            
            await base.HideAsync(cancellationToken);
        }
        
        private void StartSpinnerAnimation()
        {
            if (_spinnerIcon != null)
            {
                StopSpinnerAnimation();
                _spinnerTween = _spinnerIcon
                    .DORotate(new Vector3(0, 0, -360), 360f / _rotationSpeed, RotateMode.FastBeyond360)
                    .SetLoops(-1, LoopType.Restart)
                    .SetEase(Ease.Linear)
                    .SetUpdate(UpdateType.Normal, true);
            }
        }
        
        private void StopSpinnerAnimation()
        {
            _spinnerTween?.Kill();
            _spinnerTween = null;
        }
    }
}