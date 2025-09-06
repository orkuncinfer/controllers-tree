using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Controllers.Game;
using Game.Models;
using Game.Services;
using Playtika.Controllers;
using UnityEngine;
using VContainer;

namespace Game.Controllers.UI
{
    public sealed class MainMenuController : ControllerWithResultBase
    {
        private readonly IUIService _uiService;
        private readonly IGameStateModel _gameStateModel;
        
        public MainMenuController(
            IControllerFactory controllerFactory,
            IUIService uiService,
            IGameStateModel gameStateModel) 
            : base(controllerFactory)
        {
            _uiService = uiService;
            _gameStateModel = gameStateModel;
        }
        
        protected override async UniTask OnFlowAsync(CancellationToken cancellationToken)
        {
            _gameStateModel.ChangeState(GameState.MainMenu);
            
            var panel = await _uiService.GetPanelAsync("MainMenu", cancellationToken);
            await panel.ShowAsync(cancellationToken);
            
            // Wait for start button
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || 
                                          !panel.GameObject.activeSelf, 
                cancellationToken: cancellationToken);
            
            await panel.HideAsync(cancellationToken);
            
            // Start game
            await ExecuteAndWaitResultAsync<GameSessionController>(cancellationToken);
            
            // Return to main menu after game ends
            await ExecuteAndWaitResultAsync<MainMenuController>(cancellationToken);
            
            Complete();
        }
    }
}