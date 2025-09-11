using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Views.UI;
using UnityEngine;

namespace Game.Services
{
    public interface ILoadingService
    {
        bool IsLoading { get; }
        UniTask ShowLoadingAsync(CancellationToken cancellationToken);
        UniTask HideLoadingAsync(CancellationToken cancellationToken);
        void SetProgress(float progress);
        void SetLoadingText(string text);
    }
    
    public sealed class LoadingService : ILoadingService, IDisposable
    {
        private readonly IUIService _uiService;
        private LoadingPanel _currentLoadingPanel;
        private bool _isLoading;
        private float _targetProgress;
        private string _loadingText = "LOADING...";
        
        public bool IsLoading => _isLoading;
        
        public LoadingService(IUIService uiService)
        {
            _uiService = uiService;
        }
        
        public async UniTask ShowLoadingAsync(CancellationToken cancellationToken)
        {
            if (_isLoading) return;
            
            _isLoading = true;
            _targetProgress = 0f;
            
            try
            {
                var panel = await _uiService.GetPanelAsync("LoadingPanel", cancellationToken);
                _currentLoadingPanel = panel as LoadingPanel;
                
                if (_currentLoadingPanel != null)
                {
                    _currentLoadingPanel.SetLoadingText(_loadingText);
                    _currentLoadingPanel.SetProgress(_targetProgress);
                    await _currentLoadingPanel.ShowAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to show loading panel: {ex.Message}");
                _isLoading = false;
                throw;
            }
        }
        
        public async UniTask HideLoadingAsync(CancellationToken cancellationToken)
        {
            if (!_isLoading) return;
            
            try
            {
                if (_currentLoadingPanel != null)
                {
                    // Ensure progress reaches 100% before hiding
                    _currentLoadingPanel.SetProgress(1f);
                    await UniTask.Delay(200, cancellationToken: cancellationToken);
                    
                    await _currentLoadingPanel.HideAsync(cancellationToken);
                    _currentLoadingPanel = null;
                }
            }
            finally
            {
                _isLoading = false;
                _targetProgress = 0f;
            }
        }
        
        public void SetProgress(float progress)
        {
            _targetProgress = Mathf.Clamp01(progress);
            _currentLoadingPanel?.SetProgress(_targetProgress);
        }
        
        public void SetLoadingText(string text)
        {
            _loadingText = text;
            _currentLoadingPanel?.SetLoadingText(text);
        }
        
        public void Dispose()
        {
            _currentLoadingPanel = null;
            _isLoading = false;
        }
    }
}