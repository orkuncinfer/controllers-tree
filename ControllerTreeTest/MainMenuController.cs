using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Controllers.Game;
using Game.Models;
using Game.Services;
using Game.Views.UI;
using Playtika.Controllers;
using UnityEngine;

namespace Game.Controllers.UI
{
    public sealed class MainMenuController : ControllerWithResultBase
    {
        private readonly IUIService _uiService;
        private readonly IGameStateModel _gameStateModel;
        private readonly IPlayerProgressModel _playerProgressModel;
        
        public MainMenuController(
            IControllerFactory controllerFactory,
            IUIService uiService,
            IGameStateModel gameStateModel,
            IPlayerProgressModel playerProgressModel) 
            : base(controllerFactory)
        {
            _uiService = uiService;
            _gameStateModel = gameStateModel;
            _playerProgressModel = playerProgressModel;
        }
        
        protected override async UniTask OnFlowAsync(CancellationToken cancellationToken)
        {
            _gameStateModel.ChangeState(GameState.MainMenu);
            
            var panel = await _uiService.GetPanelAsync("MainMenu", cancellationToken);
            await panel.ShowAsync(cancellationToken);
            
            // Wait for start button (Space key)
            await UniTask.WaitUntil(() => !panel.GameObject.activeSelf, 
                cancellationToken: cancellationToken);
            
            await panel.HideAsync(cancellationToken);
            
            // Start game loop
            await StartGameLoopAsync(cancellationToken);
            
            Complete();
        }
        
        private async UniTask StartGameLoopAsync(CancellationToken cancellationToken)
        {
            bool continuePlaying = true;
            
            while (continuePlaying && !cancellationToken.IsCancellationRequested)
            {
                // Start game session with current level
                var gameArgs = new GameSessionArgs(_playerProgressModel.CurrentLevelIndex);
                var gameResult = await ExecuteAndWaitResultAsync<GameSessionController, GameSessionArgs, GameResult>(
                    gameArgs, cancellationToken);
                
                // Handle game result
                if (gameResult == GameResult.Win)
                {
                    // Player won, check win panel result
                    var winResult = await ExecuteAndWaitResultAsync<WinPanelController, WinPanelResult>(
                        cancellationToken);
                    
                    if (winResult == WinPanelResult.NextLevel)
                    {
                        // Continue to next level
                        _playerProgressModel.NextLevel();
                        continuePlaying = true;
                    }
                    else // MainMenu
                    {
                        continuePlaying = false;
                    }
                }
                else // Lose
                {
                    // Player lost, check lose panel result
                    var loseResult = await ExecuteAndWaitResultAsync<LosePanelController, LosePanelResult>(
                        cancellationToken);
                    
                    if (loseResult == LosePanelResult.TryAgain)
                    {
                        // Retry same level
                        continuePlaying = true;
                    }
                    else // MainMenu
                    {
                        continuePlaying = false;
                    }
                }
            }
            
            // Return to main menu
            if (!cancellationToken.IsCancellationRequested)
            {
                await ExecuteAndWaitResultAsync<MainMenuController>(cancellationToken);
            }
        }
    }
}