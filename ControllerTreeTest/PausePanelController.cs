using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Services;
using Game.Views.UI;
using Playtika.Controllers;

namespace Game.Controllers.UI
{
    public sealed class PausePanelController : ControllerWithResultBase
    {
        private readonly IUIService _uiService;
        
        public PausePanelController(
            IControllerFactory controllerFactory,
            IUIService uiService) 
            : base(controllerFactory)
        {
            _uiService = uiService;
        }
        
        protected override async UniTask OnFlowAsync(CancellationToken cancellationToken)
        {
            var panel = await _uiService.GetPanelAsync("PausePanel", cancellationToken) as PausePanel;
            
            if (panel == null)
            {
                UnityEngine.Debug.LogError("PausePanel not found!");
                Complete();
                return;
            }
            
            await panel.ShowAsync(cancellationToken);
            
            // Wait for panel to close
            await UniTask.WaitUntil(() => !panel.GameObject.activeSelf, 
                cancellationToken: cancellationToken);
            
            Complete();
        }
    }
}