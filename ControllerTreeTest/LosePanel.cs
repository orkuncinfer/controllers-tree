using System;

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views.UI
{
    public sealed class LosePanel : BaseUIPanel
    {
        [SerializeField] private Button _tryAgainButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private TMPro.TextMeshProUGUI _titleText;
        
        public LosePanelResult Result { get; private set; }
        
        protected override void Awake()
        {
            base.Awake();
            
            if (_titleText) _titleText.text = "GAME OVER";
            
            _tryAgainButton.onClick.AddListener(() =>
            {
                Result = LosePanelResult.TryAgain;
                gameObject.SetActive(false);
            });
            
            _mainMenuButton.onClick.AddListener(() =>
            {
                Result = LosePanelResult.MainMenu;
                gameObject.SetActive(false);
            });
        }
        
        public override async UniTask ShowAsync(CancellationToken cancellationToken)
        {
            Result = LosePanelResult.TryAgain; // Default result
            await base.ShowAsync(cancellationToken);
        }
    }
    
    public enum LosePanelResult
    {
        TryAgain,
        MainMenu
    }
}