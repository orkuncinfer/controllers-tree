using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Controllers.UI;
using Game.Models;
using Game.Services;
using Playtika.Controllers;
using VContainer;
using UnityEngine;

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
        Lose
    }
    
    public sealed class GameSessionController : ControllerWithResultBase<GameSessionArgs, GameResult>
    {
        private readonly ISceneService _sceneService;
        private readonly IGameStateModel _gameStateModel;
        private readonly IPlayerProgressModel _playerProgressModel;
        
        public GameSessionController(
            IControllerFactory controllerFactory,
            ISceneService sceneService,
            IGameStateModel gameStateModel,
            IPlayerProgressModel playerProgressModel) 
            : base(controllerFactory)
        {
            _sceneService = sceneService;
            _gameStateModel = gameStateModel;
            _playerProgressModel = playerProgressModel;
        }
        
        protected override async UniTask OnFlowAsync(CancellationToken cancellationToken)
        {
            _gameStateModel.ChangeState(GameState.Loading);
            
            // Load level scene
            var levelName = $"Level{Args.LevelIndex}";
            await _sceneService.LoadSceneAsync(levelName, cancellationToken);
            
            _gameStateModel.ChangeState(GameState.Playing);
            
            // Wait for game input
            var result = await WaitForGameResult(cancellationToken);
            
            // Show result UI
            if (result == GameResult.Win)
            {
                _playerProgressModel.NextLevel();
                await ExecuteAndWaitResultAsync<WinPanelController>(cancellationToken);
            }
            else
            {
                await ExecuteAndWaitResultAsync<LosePanelController>(cancellationToken);
            }
            
            // Unload level
            await _sceneService.UnloadSceneAsync(levelName, cancellationToken);
            
            Complete(result);
        }
        
        private async UniTask<GameResult> WaitForGameResult(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    return GameResult.Win;
                }
                if (Input.GetKeyDown(KeyCode.L))
                {
                    return GameResult.Lose;
                }
                
                await UniTask.Yield(cancellationToken);
            }
            
            throw new OperationCanceledException();
        }
    }
}