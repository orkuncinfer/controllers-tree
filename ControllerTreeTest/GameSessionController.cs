using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Controllers.UI;
using Game.Data;
using Game.Models;
using Game.Services;
using Game.Views.UI;
using Playtika.Controllers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Controllers.Game
{
    public readonly struct GameSessionArgs
    {
        public readonly int LevelIndex;
        
        public GameSessionArgs(int levelIndex)
        {
            LevelIndex = levelIndex;
        }
    }
    
    public enum GameResult
    {
        Win,
        Lose,
        Retry
    }
    
    public sealed class GameSessionController : ControllerWithResultBase<GameSessionArgs, GameResult>
    {
        private readonly ISceneService _sceneService;
        private readonly IGameStateModel _gameStateModel;
        private readonly LevelList _levelList;
        private readonly IUIService _uiService;
        
        private string _currentLevelName;
        private bool _isPaused;
        
        public GameSessionController(
            IControllerFactory controllerFactory,
            ISceneService sceneService,
            IGameStateModel gameStateModel,
            LevelList levelList,
            IUIService uiService) 
            : base(controllerFactory)
        {
            _sceneService = sceneService;
            _gameStateModel = gameStateModel;
            _levelList = levelList;
            _uiService = uiService;
        }
        
        protected override async UniTask OnFlowAsync(CancellationToken cancellationToken)
        {
            _gameStateModel.ChangeState(GameState.Loading);
            
            // Get level name from LevelList
            _currentLevelName = _levelList.GetLevelName(Args.LevelIndex);
            
            if (string.IsNullOrEmpty(_currentLevelName))
            {
                Debug.LogError($"No level found at index {Args.LevelIndex}");
                Complete(GameResult.Lose);
                return;
            }
            
            Debug.Log($"Loading level: {_currentLevelName} (Index: {Args.LevelIndex})");
            
            // Load level scene
            await _sceneService.LoadSceneAsync(_currentLevelName, cancellationToken);
            
            _gameStateModel.ChangeState(GameState.Playing);
            
            // Wait for game input
            var gameplayResult = await WaitForGameResult(cancellationToken);
            
            // Handle result BEFORE unloading the scene
            GameResult finalResult;
            if (gameplayResult == GameResult.Win)
            {
                finalResult = await HandleWinAsync(cancellationToken);
            }
            else
            {
                finalResult = await HandleLoseAsync(cancellationToken);
            }
            
            // Now unload the level after we know the final result
            await _sceneService.UnloadSceneAsync(_currentLevelName, cancellationToken);
            
            Complete(finalResult);
        }
        
        private async UniTask<GameResult> HandleWinAsync(CancellationToken cancellationToken)
        {
            var winResult = await ExecuteAndWaitResultAsync<WinPanelController, WinPanelResult>(
                cancellationToken);
            
            if (winResult == WinPanelResult.NextLevel)
            {
                return GameResult.Win;
            }
            else
            {
                return GameResult.Lose; // Return to menu
            }
        }
        
        private async UniTask<GameResult> HandleLoseAsync(CancellationToken cancellationToken)
        {
            var loseResult = await ExecuteAndWaitResultAsync<LosePanelController, LosePanelResult>(
                cancellationToken);
            
            if (loseResult == LosePanelResult.TryAgain)
            {
                return GameResult.Retry;
            }
            else
            {
                return GameResult.Lose; // Return to menu
            }
        }
        
        private async UniTask<GameResult> WaitForGameResult(CancellationToken cancellationToken)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                Debug.LogError("No keyboard detected!");
                throw new OperationCanceledException();
            }
            
            while (!cancellationToken.IsCancellationRequested)
            {
                // Check for pause
                if (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame)
                {
                    await HandlePauseAsync(cancellationToken);
                }
                
                // Check for win/lose only when not paused
                if (!_isPaused)
                {
                    if (keyboard.wKey.wasPressedThisFrame)
                    {
                        Debug.Log("Win pressed!");
                        return GameResult.Win;
                    }
                    if (keyboard.lKey.wasPressedThisFrame)
                    {
                        Debug.Log("Lose pressed!");
                        return GameResult.Lose;
                    }
                }
                
                await UniTask.Yield(cancellationToken);
            }
            
            throw new OperationCanceledException();
        }
        
        private async UniTask HandlePauseAsync(CancellationToken cancellationToken)
        {
            _isPaused = true;
            _gameStateModel.ChangeState(GameState.Paused);
            
            // Pause the game
            Time.timeScale = 0f;
            
            // Show pause panel and wait for result
            await ExecuteAndWaitResultAsync<PausePanelController>(cancellationToken);
            
            // Resume the game
            Time.timeScale = 1f;
            _gameStateModel.ChangeState(GameState.Playing);
            _isPaused = false;
        }
        
        protected override void OnStop()
        {
            base.OnStop();
            // Ensure time scale is reset if controller stops
            Time.timeScale = 1f;
        }
    }
}