using Game.Controllers.Game;
using Game.Controllers.Root;
using Game.Controllers.UI;
using Game.Data;
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
        [SerializeField] private LevelList _levelList;
        
        protected override void Configure(IContainerBuilder builder)
        {
            // Register ScriptableObject
            builder.RegisterInstance(_levelList);
            
            // Register factory as Scoped (per scope instance)
            builder.Register<IControllerFactory, VContainerControllerFactory>(Lifetime.Scoped);
            
            // Register services as Singleton
            builder.Register<ISceneService, SceneService>(Lifetime.Singleton);
            builder.Register<IUIService, UIService>(Lifetime.Singleton);
            
            // Register models as Singleton
            builder.Register<IGameStateModel, GameStateModel>(Lifetime.Singleton);
            builder.Register<IPlayerProgressModel, PlayerProgressModel>(Lifetime.Singleton);
            
            // Register controllers as Transient (new instance each time)
            builder.Register<RootGameController>(Lifetime.Transient);
            builder.Register<GameLoopController>(Lifetime.Transient);
            builder.Register<GameSessionController>(Lifetime.Transient);
            builder.Register<WinPanelController>(Lifetime.Transient);
            builder.Register<LosePanelController>(Lifetime.Transient);
            builder.Register<PausePanelController>(Lifetime.Transient); // NEW!
            
            // Register entry point
            builder.RegisterEntryPoint<GameEntryPoint>();
        }
    }
}