using System;
using Playtika.Controllers;
using VContainer;

namespace Game.Infrastructure
{
    public sealed class VContainerControllerFactory : IControllerFactory
    {
        private readonly IObjectResolver _resolver;
        
        public VContainerControllerFactory(IObjectResolver resolver)
        {
            _resolver = resolver;
        }
        
        public IController Create<T>() where T : class, IController
        {
            return _resolver.Resolve<T>();
        }
        
        public override string ToString() => "GameScope";
    }
}