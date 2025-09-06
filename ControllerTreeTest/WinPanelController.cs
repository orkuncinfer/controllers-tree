using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Services;
using Playtika.Controllers;
using VContainer;

namespace Game.Controllers.UI
{
    public enum WinPanelResult
    {
        NextLevel,
        MainMenu
    }
    
    public sealed class WinPanelController : ControllerWithResultBase<WinPanelResult>
    {
        private readonly IUIService _uiService;
        
        public WinPanelController(
            IControllerFactory controllerFactory,
            IUIService uiService) 
            : base(controllerFactory)
        {
            _uiService = uiService;
        }
        
        protected override async UniTask OnFlowAsync(CancellationToken cancellationToken)
        {
            // Use UI queue to ensure panels don't overlap
            await _uiService.EnqueuePanelAsync("WinPanel", cancellationToken);
            
            // Panel will auto-close when button is clicked
            // The result is determined by which button was clicked
            var result = WinPanelResult.NextLevel; // Get from panel
            
            Complete(result);
        }
    }
}