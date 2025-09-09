using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Controllers.Game;
using Game.Models;
using Game.Services;
using Playtika.Controllers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Controllers.Root
{
    public sealed class GameLoopController : ControllerBase
    {
        private readonly IUIService _uiService;
        private readonly IGameStateModel _gameStateModel;
        private readonly IPlayerProgressModel _playerProgressModel;
        
        public GameLoopController(
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
        
        protected override void OnStart()
        {
            base.OnStart();
            RunGameLoopAsync(CancellationToken).Forget();
        }
        
        private async UniTaskVoid RunGameLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Show main menu and wait for start
                    await ShowMainMenuAsync(cancellationToken);
                    
                    // Run game sessions until player returns to menu
                    await RunGameSessionsAsync(cancellationToken);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Expected when game is closing
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in game loop: {ex.Message}");
            }
        }
        
        private async UniTask ShowMainMenuAsync(CancellationToken cancellationToken)
        {
            _gameStateModel.ChangeState(GameState.MainMenu);
            
            var panel = await _uiService.GetPanelAsync("MainMenu", cancellationToken);
            await panel.ShowAsync(cancellationToken);
            
            var keyboard = Keyboard.current;
            
            // Wait for start button (Space key)
            await UniTask.WaitUntil(() => 
                (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) || 
                !panel.GameObject.activeSelf, 
                cancellationToken: cancellationToken);
            
            await panel.HideAsync(cancellationToken);
        }
        
        private async UniTask RunGameSessionsAsync(CancellationToken cancellationToken)
        {
            bool continuePlaying = true;
            
            while (continuePlaying && !cancellationToken.IsCancellationRequested)
            {
                // Start game session with current level
                var gameArgs = new GameSessionArgs(_playerProgressModel.CurrentLevelIndex);
                var gameResult = await ExecuteAndWaitResultAsync<GameSessionController, GameSessionArgs, GameResult>(
                    gameArgs, cancellationToken);
                
                // Handle game result
                switch (gameResult)
                {
                    case GameResult.Win:
                        // Move to next level
                        _playerProgressModel.NextLevel();
                        continuePlaying = true;
                        break;
                        
                    case GameResult.Retry:
                        // Retry same level
                        continuePlaying = true;
                        break;
                        
                    case GameResult.Lose:
                        // Return to main menu
                        continuePlaying = false;
                        break;
                }
            }
        }
    }
}