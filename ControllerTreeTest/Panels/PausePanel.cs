using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Game.Views.UI
{
    public sealed class PausePanel : BaseUIPanel
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private TMPro.TextMeshProUGUI _titleText;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (_titleText) _titleText.text = "PAUSED";
            
            _resumeButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
            });
        }
        
        public override async UniTask ShowAsync(CancellationToken cancellationToken)
        {
            // Important: Use unscaled time for pause menu animations
            gameObject.SetActive(true);
            var canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            
            // Use unscaled time for animation
            await canvasGroup.DOFade(1f, 0.3f)
                .SetUpdate(UpdateType.Normal, true) // true = unscaled time
                .WithCancellation(cancellationToken);
            
            canvasGroup.interactable = true;
        }
        
        public override async UniTask HideAsync(CancellationToken cancellationToken)
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            
            // Use unscaled time for animation
            await canvasGroup.DOFade(0f, 0.3f)
                .SetUpdate(UpdateType.Normal, true) // true = unscaled time
                .WithCancellation(cancellationToken);
            
            gameObject.SetActive(false);
        }
    }
}