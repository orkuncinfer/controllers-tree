using Game.Models;
using Game.Services;
using VContainer;
using VContainer.Unity;

public sealed class RootLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Register core services as Singleton
        builder.Register<IUIResourceService, UIResourceService>(Lifetime.Singleton);
        builder.Register<ISceneService, SceneService>(Lifetime.Singleton);
        builder.Register<IUIService, UIService>(Lifetime.Singleton);
        
        // Register models as Singleton
        builder.Register<IGameStateModel, GameStateModel>(Lifetime.Singleton);
        builder.Register<IPlayerProgressModel, PlayerProgressModel>(Lifetime.Singleton);
    }
}
