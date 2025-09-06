using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Services;
using Playtika.Controllers;
using VContainer;

namespace Game.Controllers.UI
{
    public enum LosePanelResult
    {
        TryAgain,
        MainMenu
    }
    
    public sealed class LosePanelController : ControllerWithResultBase<LosePanelResult>
    {
        private readonly IUIService _uiService;
        
        public LosePanelController(
            IControllerFactory controllerFactory,
            IUIService uiService) 
            : base(controllerFactory)
        {
            _uiService = uiService;
        }
        
        protected override async UniTask OnFlowAsync(CancellationToken cancellationToken)
        {
            await _uiService.EnqueuePanelAsync("LosePanel", cancellationToken);
            
            var result = LosePanelResult.TryAgain; // Get from panel
            
            Complete(result);
        }
    }
}