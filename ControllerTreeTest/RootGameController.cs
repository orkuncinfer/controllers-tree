using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Controllers.UI;
using Game.Models;
using Playtika.Controllers;
using VContainer;

namespace Game.Controllers.Root
{
    public sealed class RootGameController : RootController
    {
        private readonly IGameStateModel _gameStateModel;
        
        public RootGameController(
            IControllerFactory controllerFactory,
            IGameStateModel gameStateModel) 
            : base(controllerFactory)
        {
            _gameStateModel = gameStateModel;
        }
        
        protected override void OnStart()
        {
            base.OnStart();
            Execute<MainMenuController>();
        }
    }
}