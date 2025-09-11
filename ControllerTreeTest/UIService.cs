using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Services
{
    public interface IUIPanel
    {
        GameObject GameObject { get; }
        UniTask ShowAsync(CancellationToken cancellationToken);
        UniTask HideAsync(CancellationToken cancellationToken);
    }
    
    public interface IUIService
    {
        UniTask<IUIPanel> GetPanelAsync(string panelId, CancellationToken cancellationToken);
    }
    
    public sealed class UIService : IUIService, IDisposable
    {
        private readonly IUIResourceService _resourceService;
        private readonly Dictionary<string, IUIPanel> _activePanels = new();
        private readonly Queue<IUIPanel> _panelPool = new();
        private Transform _uiRoot;
        
        public UIService(IUIResourceService resourceService)
        {
            _resourceService = resourceService;
        }
        
        public void Initialize(Transform uiRoot)
        {
            _uiRoot = uiRoot;
        }
        
        public async UniTask<IUIPanel> GetPanelAsync(string panelId, CancellationToken cancellationToken)
        {
            // Check if panel is already active
            if (_activePanels.TryGetValue(panelId, out var activePanel))
            {
                return activePanel;
            }
            
            // Ensure UI root exists
            if (_uiRoot == null)
            {
                _uiRoot = await EnsureUIRootAsync(cancellationToken);
            }
            
            // Load panel from resources
            var panel = await _resourceService.LoadPanelAsync<IUIPanel>(panelId, _uiRoot, cancellationToken);
            _activePanels[panelId] = panel;
            
            return panel;
        }
        
        public async UniTask<T> GetPanelAsync<T>(string panelId, CancellationToken cancellationToken) 
            where T : class, IUIPanel
        {
            var panel = await GetPanelAsync(panelId, cancellationToken);
            return panel as T;
        }
        
        public void ReleasePanel(string panelId)
        {
            if (_activePanels.TryGetValue(panelId, out var panel))
            {
                _resourceService.ReleasePanel(panel);
                _activePanels.Remove(panelId);
            }
        }
        
        private async UniTask<Transform> EnsureUIRootAsync(CancellationToken cancellationToken)
        {
            // Find or create UI root
            var uiRootGO = GameObject.Find("[UI Root]");
            
            if (uiRootGO == null)
            {
                // Create UI root with Canvas
                uiRootGO = new GameObject("[UI Root]");
                var canvas = new GameObject("Canvas");
                canvas.transform.SetParent(uiRootGO.transform);
                
                var canvasComponent = canvas.AddComponent<Canvas>();
                canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasComponent.sortingOrder = 100;
                
                canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                
                GameObject.DontDestroyOnLoad(uiRootGO);
            }
            
            var canvasTransform = uiRootGO.transform.Find("Canvas");
            return canvasTransform ?? uiRootGO.transform;
        }
        
        public void Dispose()
        {
            foreach (var panel in _activePanels.Values)
            {
                _resourceService.ReleasePanel(panel);
            }
            
            _activePanels.Clear();
        }
    }
}