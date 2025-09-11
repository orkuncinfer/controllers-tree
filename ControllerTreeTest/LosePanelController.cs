using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Services;
using Game.Views.UI;
using Playtika.Controllers;
using UnityEngine;

namespace Game.Controllers.UI
{
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
            var panel = await _uiService.GetPanelAsync("LosePanel", cancellationToken) as LosePanel;
            
            if (panel == null)
            {
                Debug.LogError("LosePanel not found!");
                Complete(LosePanelResult.MainMenu);
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