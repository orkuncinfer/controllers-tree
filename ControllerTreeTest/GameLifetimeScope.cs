using Game.Controllers.Game;
using Game.Controllers.Root;
using Game.Controllers.UI;
using Game.Models;
using Game.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Playtika.Controllers;

namespace Game.Infrastructure
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // Register factory as Scoped (per scope instance)
            builder.Register<IControllerFactory, VContainerControllerFactory>(Lifetime.Scoped);
            
            // Register services as Singleton
            builder.Register<ISceneService, SceneService>(Lifetime.Singleton);
            builder.Register<IUIService, UIService>(Lifetime.Singleton);
            
            // Register models as Singleton
            builder.Register<IGameStateModel, GameStateModel>(Lifetime.Singleton);
            builder.Register<IPlayerProgressModel, PlayerProgressModel>(Lifetime.Singleton);
            
            builder.Register<RootGameController>(Lifetime.Transient);
            builder.Register<MainMenuController>(Lifetime.Transient);
            builder.Register<GameSessionController>(Lifetime.Transient);
            builder.Register<WinPanelController>(Lifetime.Transient);
            builder.Register<LosePanelController>(Lifetime.Transient);
            
            // Register root controller
            builder.RegisterEntryPoint<GameEntryPoint>();
        }
    }
}