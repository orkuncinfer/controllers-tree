using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views.UI
{
    public sealed class WinPanel : BaseUIPanel
    {
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private TMPro.TextMeshProUGUI _titleText;
        
        public WinPanelResult Result { get; private set; }
        
        protected override void Awake()
        {
            base.Awake();
            
            if (_titleText) _titleText.text = "LEVEL COMPLETE!";
            
            _nextLevelButton.onClick.AddListener(() =>
            {
                Result = WinPanelResult.NextLevel;
                gameObject.SetActive(false);
            });
            
            _mainMenuButton.onClick.AddListener(() =>
            {
                Result = WinPanelResult.MainMenu;
                gameObject.SetActive(false);
            });
        }
        
        public override async UniTask ShowAsync(CancellationToken cancellationToken)
        {
            Result = WinPanelResult.NextLevel; // Default result
            await base.ShowAsync(cancellationToken);
        }
    }
    
    public enum WinPanelResult
    {
        NextLevel,
        MainMenu
    }
}