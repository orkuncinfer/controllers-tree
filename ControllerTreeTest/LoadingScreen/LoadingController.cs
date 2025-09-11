using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Services;
using Playtika.Controllers;

namespace Game.Controllers.UI
{
    public sealed class LoadingController : ControllerWithResultBase
    {
        private readonly ILoadingService _loadingService;
        private readonly Func<CancellationToken, UniTask> _loadingOperation;
        private readonly LoadingProgressReporter _progressReporter;
        
        public LoadingController(
            IControllerFactory controllerFactory,
            ILoadingService loadingService,
            Func<CancellationToken, UniTask> loadingOperation,
            LoadingProgressReporter progressReporter = null) 
            : base(controllerFactory)
        {
            _loadingService = loadingService;
            _loadingOperation = loadingOperation;
            _progressReporter = progressReporter;
        }
        
        protected override async UniTask OnFlowAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Show loading screen
                await _loadingService.ShowLoadingAsync(cancellationToken);
                
                // Subscribe to progress updates if reporter is provided
                if (_progressReporter != null)
                {
                    _progressReporter.OnProgressChanged += OnProgressChanged;
                }
                
                // Execute the loading operation
                await _loadingOperation(cancellationToken);
                
                // Hide loading screen
                await _loadingService.HideLoadingAsync(cancellationToken);
                
                Complete();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Loading failed: {ex.Message}");
                await _loadingService.HideLoadingAsync(CancellationToken.None);
                throw;
            }
            finally
            {
                if (_progressReporter != null)
                {
                    _progressReporter.OnProgressChanged -= OnProgressChanged;
                }
            }
        }
        
        private void OnProgressChanged(float progress, string message)
        {
            _loadingService.SetProgress(progress);
            if (!string.IsNullOrEmpty(message))
            {
                _loadingService.SetLoadingText(message);
            }
        }
    }
    
    public class LoadingProgressReporter
    {
        public event Action<float, string> OnProgressChanged;
        
        public void Report(float progress, string message = null)
        {
            OnProgressChanged?.Invoke(progress, message);
        }
    }
}