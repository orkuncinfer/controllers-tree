// 09/07/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Controllers.Root;
using Game.Models;
using Playtika.Controllers;
using VContainer.Unity;

namespace Game.Infrastructure
{
    public sealed class GameEntryPoint : IAsyncStartable
    {
        private readonly IControllerFactory _controllerFactory;
        private readonly IGameStateModel _gameStateModel;
        private RootGameController _rootController;
        private CancellationTokenSource _cts;
        
        public GameEntryPoint(IControllerFactory controllerFactory, IGameStateModel gameStateModel)
        {
            _controllerFactory = controllerFactory;
            _gameStateModel = gameStateModel;
        }
        
        public async UniTask StartAsync(CancellationToken cancellation)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            
            _rootController = new RootGameController(_controllerFactory, _gameStateModel);
            _rootController.LaunchTree(_cts.Token);
            
            await UniTask.Never(cancellation);
        }
    }
}