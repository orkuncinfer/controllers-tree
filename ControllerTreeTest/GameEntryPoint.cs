using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Controllers.Root;
using Game.Models;
using Game.Services;
using Game.Views.UI;
using Playtika.Controllers;
using UnityEngine;
using VContainer.Unity;

namespace Game.Infrastructure
{
    public sealed class GameEntryPoint : IAsyncStartable
    {
        private readonly IControllerFactory _controllerFactory;
        private readonly IGameStateModel _gameStateModel;
        private readonly IUIService _uiService;
        private RootGameController _rootController;
        private CancellationTokenSource _cts;
        
        public GameEntryPoint(
            IControllerFactory controllerFactory, 
            IGameStateModel gameStateModel,
            IUIService uiService)
        {
            _controllerFactory = controllerFactory;
            _gameStateModel = gameStateModel;
            _uiService = uiService;
        }
        
        public async UniTask StartAsync(CancellationToken cancellation)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            
            // Get loading panel
            var loadingPanel = GameObject.FindObjectOfType<LoadingPanel>();
            
            try
            {
                // Initialize UI Bootstrapper
                if (loadingPanel) loadingPanel.SetProgress(0.2f);
                await UniTask.Delay(100, cancellationToken: cancellation);
                
                var uiBootstrapper = GameObject.FindObjectOfType<UIBootstrapper>();
                if (uiBootstrapper != null)
                {
                    uiBootstrapper.Initialize(_uiService);
                }
                else
                {
                    Debug.LogError("UIBootstrapper not found in scene!");
                }
                
                // Initialize services
                if (loadingPanel) loadingPanel.SetProgress(0.4f);
                await UniTask.Delay(100, cancellationToken: cancellation);
                
                // Create root controller
                if (loadingPanel) loadingPanel.SetProgress(0.6f);
                _rootController = new RootGameController(_controllerFactory, _gameStateModel);
                
                // Small delay for visual effect
                if (loadingPanel) loadingPanel.SetProgress(0.8f);
                await UniTask.Delay(100, cancellationToken: cancellation);
                
                // Launch the tree
                if (loadingPanel) loadingPanel.SetProgress(1f);
                _rootController.LaunchTree(_cts.Token);
                
                await UniTask.Delay(200, cancellationToken: cancellation);
                
                // Hide loading panel
                if (loadingPanel)
                {
                    await loadingPanel.HideAsync(cancellation);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during startup: {ex.Message}");
                if (loadingPanel) loadingPanel.gameObject.SetActive(false);
                throw;
            }
            
            await UniTask.Never(cancellation);
        }
    }
}