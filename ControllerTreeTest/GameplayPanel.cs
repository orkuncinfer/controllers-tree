using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Views.UI
{
    public sealed class GameplayPanel : BaseUIPanel
    {
        [Header("UI Elements")]
        [SerializeField] private Button _pauseButton;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _levelText;
        
        public event Action OnPauseClicked;
        
        protected override void InitializePanel()
        {
            base.InitializePanel();
            
            RegisterButtonListener(_pauseButton, () => OnPauseClicked?.Invoke());
            
            // Initialize default values
            SetScore(0);
            SetTimer(0);
            SetLevel(1);
        }
        
        public void SetScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {score}";
        }
        
        public void SetTimer(float time)
        {
            if (_timerText != null)
            {
                int minutes = Mathf.FloorToInt(time / 60);
                int seconds = Mathf.FloorToInt(time % 60);
                _timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }
        
        public void SetLevel(int levelNumber)
        {
            if (_levelText != null)
                _levelText.text = $"Level {levelNumber}";
        }
        
        public override async UniTask ShowAsync(CancellationToken cancellationToken)
        {
            // Quick fade in for gameplay UI
            var canvasGroup = GetComponent<CanvasGroup>();
            gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            
            await canvasGroup.DOFade(1f, 0.2f)
                .SetUpdate(UpdateType.Normal, true)
                .WithCancellation(cancellationToken);
            
            canvasGroup.interactable = true;
        }
    }
}