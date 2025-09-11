using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Controllers.Game;
using Game.Models;
using Game.Services;
using Game.Infrastructure;
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
                // Check if we started from a level scene
                var sceneType = SceneContext.GetCurrentSceneType();
                
                if (sceneType == SceneType.Level)
                {
                    // Started from level - skip main menu and go directly to game
                    Debug.Log("Started from level scene, skipping main menu");
                    
                    // Set the correct level index based on current scene
                    var levelIndex = SceneContext.GetLevelIndex();
                    _playerProgressModel.SetLevelIndex(levelIndex);
                    
                    // Run single game session
                    await RunSingleGameSessionAsync(levelIndex, cancellationToken);
                }
                else
                {
                    // Normal game loop with main menu
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        // Show main menu and wait for start
                        await ShowMainMenuAsync(cancellationToken);
                        
                        // Run game sessions until player returns to menu
                        await RunGameSessionsAsync(cancellationToken);
                    }
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
        
        private async UniTask RunSingleGameSessionAsync(int levelIndex, CancellationToken cancellationToken)
        {
            // Run single session for direct level start
            var gameArgs = new GameSessionArgs(levelIndex);
            var gameResult = await ExecuteAndWaitResultAsync<GameSessionController, GameSessionArgs, GameResult>(
                gameArgs, cancellationToken);
            
            // After game ends from direct start
            switch (gameResult)
            {
                case GameResult.Win:
                    _playerProgressModel.NextLevel();
                    // Continue with normal game sessions
                    await RunGameSessionsAsync(cancellationToken);
                    break;
                    
                case GameResult.Retry:
                    // Retry the same level
                    await RunSingleGameSessionAsync(levelIndex, cancellationToken);
                    break;
                    
                case GameResult.Lose:
                    // Load boot scene and show main menu
                    await TransitionToMainMenu(cancellationToken);
                    break;
            }
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
        
        private async UniTask TransitionToMainMenu(CancellationToken cancellationToken)
        {
            // If not in boot scene, load it first
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (currentScene.name != "Boot")
            {
                await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Boot")
                    .WithCancellation(cancellationToken);
            }
            
            // Show main menu
            await ShowMainMenuAsync(cancellationToken);
            await RunGameSessionsAsync(cancellationToken);
        }
    }
}