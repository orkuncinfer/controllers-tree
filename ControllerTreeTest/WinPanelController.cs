using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Services;
using Game.Views.UI;
using Playtika.Controllers;

namespace Game.Controllers.UI
{
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
            var panel = await _uiService.GetPanelAsync("WinPanel", cancellationToken) as WinPanel;
            
            if (panel == null)
            {
                UnityEngine.Debug.LogError("WinPanel not found!");
                Complete(WinPanelResult.MainMenu);
                return;
            }
            
            await panel.ShowAsync(cancellationToken);
            
            // Wait for panel to close
            await UniTask.WaitUntil(() => !panel.GameObject.activeSelf, 
                cancellationToken: cancellationToken);
            
            Complete(panel.Result);
        }
    }
}