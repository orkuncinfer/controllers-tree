using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Controllers.Root;
using Game.Models;
using Game.Services;
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
        private readonly ILoadingService _loadingService;
        private RootGameController _rootController;
        private CancellationTokenSource _cts;
        
        public GameEntryPoint(
            IControllerFactory controllerFactory, 
            IGameStateModel gameStateModel,
            IUIService uiService,
            ILoadingService loadingService)
        {
            _controllerFactory = controllerFactory;
            _gameStateModel = gameStateModel;
            _uiService = uiService;
            _loadingService = loadingService;
        }
        
        public async UniTask StartAsync(CancellationToken cancellation)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            
            try
            {
                var sceneType = SceneContext.GetCurrentSceneType();
                
                // Only show loading for boot scene
                if (sceneType == SceneType.Boot)
                {
                    await _loadingService.ShowLoadingAsync(cancellation);
                    _loadingService.SetLoadingText("Initializing...");
                    _loadingService.SetProgress(0.5f);
                    await UniTask.Delay(100, cancellationToken: cancellation);
                }
                
                // Create and launch root controller
                _rootController = new RootGameController(_controllerFactory, _gameStateModel);
                _rootController.LaunchTree(_cts.Token);
                
                // Hide loading if shown
                if (sceneType == SceneType.Boot)
                {
                    _loadingService.SetProgress(1f);
                    await UniTask.Delay(200, cancellationToken: cancellation);
                    await _loadingService.HideLoadingAsync(cancellation);
                }
                
                await UniTask.Never(cancellation);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during startup: {ex.Message}");
                await _loadingService.HideLoadingAsync(CancellationToken.None);
                throw;
            }
        }
    }
}