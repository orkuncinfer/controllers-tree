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
        private string _loadedLevelName; // Track which level we actually loaded
        private bool _isPaused;
        private bool _shouldUnloadLevel; // Whether we should unload the level
        private GameplayPanel _gameplayPanel;
        private float _gameTimer;
        
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
            try
            {
                _gameStateModel.ChangeState(GameState.Loading);
                
                // Get desired level name
                _currentLevelName = _levelList?.GetLevelName(Args.LevelIndex);
                
                if (string.IsNullOrEmpty(_currentLevelName))
                {
                    Debug.LogError($"Invalid level index: {Args.LevelIndex}");
                    Complete(GameResult.Lose);
                    return;
                }
                
                // Check current scene
                var currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                
                if (!currentSceneName.Equals(_currentLevelName, StringComparison.OrdinalIgnoreCase))
                {
                    // Different level - need to unload current and load new
                    Debug.Log($"Loading level: {_currentLevelName} (current: {currentSceneName})");
                    
                    // If current scene is a level scene, unload it first
                    if (currentSceneName.StartsWith("Level", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.Log($"Unloading current level: {currentSceneName}");
                        await _sceneService.UnloadSceneAsync(currentSceneName, cancellationToken);
                    }
                    
                    // Load the new level
                    await LoadLevelAsync(cancellationToken);
                    _loadedLevelName = _currentLevelName;
                    _shouldUnloadLevel = true; // We loaded it, so we should unload it
                }
                else
                {
                    // Already in correct level
                    Debug.Log($"Already in level: {_currentLevelName}");
                    _loadedLevelName = null; // We didn't load it
                    _shouldUnloadLevel = false; // Don't unload since we didn't load it
                }
                
                // Show gameplay panel
                await ShowGameplayPanelAsync(cancellationToken);
                
                _gameStateModel.ChangeState(GameState.Playing);
                
                // Start game timer
                _gameTimer = 0f;
                UpdateGameTimer(cancellationToken).Forget();
                
                // Wait for game result
                var gameplayResult = await WaitForGameResult(cancellationToken);
                
                // Hide gameplay panel
                await HideGameplayPanelAsync(cancellationToken);
                
                // Handle result
                var finalResult = await HandleGameResultAsync(gameplayResult, cancellationToken);
                
                // Only unload if we loaded it AND we're not retrying
                if (_shouldUnloadLevel && finalResult != GameResult.Retry && !string.IsNullOrEmpty(_loadedLevelName))
                {
                    await UnloadLevelAsync(cancellationToken);
                }
                
                Complete(finalResult);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Game session cancelled");
                await CleanupAsync();
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in game session: {ex.Message}");
                await CleanupAsync();
                Complete(GameResult.Lose);
            }
        }
        
        private async UniTask LoadLevelAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _sceneService.LoadSceneAsync(_currentLevelName, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load level {_currentLevelName}: {ex.Message}");
                throw;
            }
        }
        
        private async UniTask UnloadLevelAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_loadedLevelName))
            {
                try
                {
                    Debug.Log($"Unloading level: {_loadedLevelName}");
                    await _sceneService.UnloadSceneAsync(_loadedLevelName, cancellationToken);
                    _loadedLevelName = null;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to unload level {_loadedLevelName}: {ex.Message}");
                }
            }
        }
        
        private async UniTask ShowGameplayPanelAsync(CancellationToken cancellationToken)
        {
            try
            {
                var panel = await _uiService.GetPanelAsync("GameplayPanel", cancellationToken);
                _gameplayPanel = panel as GameplayPanel;
                
                if (_gameplayPanel != null)
                {
                    _gameplayPanel.SetLevel(Args.LevelIndex + 1);
                    _gameplayPanel.OnPauseClicked += OnPauseRequested;
                    await _gameplayPanel.ShowAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to show gameplay panel: {ex.Message}");
            }
        }
        
        private async UniTask HideGameplayPanelAsync(CancellationToken cancellationToken)
        {
            if (_gameplayPanel != null)
            {
                _gameplayPanel.OnPauseClicked -= OnPauseRequested;
                await _gameplayPanel.HideAsync(cancellationToken);
                _gameplayPanel = null;
            }
        }
        
        private void OnPauseRequested()
        {
            if (!_isPaused)
            {
                HandlePauseAsync(CancellationToken).Forget();
            }
        }
        
        private async UniTaskVoid UpdateGameTimer(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_isPaused)
                {
                    _gameTimer += Time.deltaTime;
                    _gameplayPanel?.SetTimer(_gameTimer);
                }
                
                await UniTask.Yield(cancellationToken);
            }
        }
        
        private async UniTask<GameResult> HandleGameResultAsync(GameResult gameplayResult, CancellationToken cancellationToken)
        {
            switch (gameplayResult)
            {
                case GameResult.Win:
                    return await HandleWinAsync(cancellationToken);
                case GameResult.Lose:
                    return await HandleLoseAsync(cancellationToken);
                default:
                    return gameplayResult;
            }
        }
        
        private async UniTask<GameResult> HandleWinAsync(CancellationToken cancellationToken)
        {
            try
            {
                var winResult = await ExecuteAndWaitResultAsync<WinPanelController, WinPanelResult>(
                    cancellationToken);
                
                return winResult == WinPanelResult.NextLevel ? GameResult.Win : GameResult.Lose;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling win: {ex.Message}");
                return GameResult.Lose;
            }
        }
        
        private async UniTask<GameResult> HandleLoseAsync(CancellationToken cancellationToken)
        {
            try
            {
                var loseResult = await ExecuteAndWaitResultAsync<LosePanelController, LosePanelResult>(
                    cancellationToken);
                
                return loseResult == LosePanelResult.TryAgain ? GameResult.Retry : GameResult.Lose;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling lose: {ex.Message}");
                return GameResult.Lose;
            }
        }
        
        private async UniTask<GameResult> WaitForGameResult(CancellationToken cancellationToken)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                Debug.LogWarning("No keyboard detected, using default input");
            }
            
            while (!cancellationToken.IsCancellationRequested)
            {
                // Check for pause
                if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame))
                {
                    await HandlePauseAsync(cancellationToken);
                }
                
                // Check for win/lose only when not paused
                if (!_isPaused)
                {
                    if (keyboard != null)
                    {
                        if (keyboard.wKey.wasPressedThisFrame)
                        {
                            Debug.Log("Win triggered");
                            return GameResult.Win;
                        }
                        if (keyboard.lKey.wasPressedThisFrame)
                        {
                            Debug.Log("Lose triggered");
                            return GameResult.Lose;
                        }
                    }
                }
                
                await UniTask.Yield(cancellationToken);
            }
            
            throw new OperationCanceledException();
        }
        
        private async UniTask HandlePauseAsync(CancellationToken cancellationToken)
        {
            if (_isPaused) return;
            
            try
            {
                _isPaused = true;
                _gameStateModel.ChangeState(GameState.Paused);
                
                // Pause the game
                Time.timeScale = 0f;
                
                // Show pause panel and wait for result
                await ExecuteAndWaitResultAsync<PausePanelController>(cancellationToken);
            }
            finally
            {
                // Always resume the game
                Time.timeScale = 1f;
                _gameStateModel.ChangeState(GameState.Playing);
                _isPaused = false;
            }
        }
        
        private async UniTask CleanupAsync()
        {
            Time.timeScale = 1f; // Ensure time scale is reset
            
            if (_shouldUnloadLevel && !string.IsNullOrEmpty(_loadedLevelName))
            {
                try
                {
                    await _sceneService.UnloadSceneAsync(_loadedLevelName, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to cleanup level: {ex.Message}");
                }
            }
        }
        
        protected override void OnStop()
        {
            Time.timeScale = 1f; // Ensure time scale is reset
            base.OnStop();
        }
    }
}