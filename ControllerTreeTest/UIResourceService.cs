// UIResourceService.cs
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Game.Services
{
    public interface IUIResourceService
    {
        UniTask<T> LoadPanelAsync<T>(string panelKey, Transform parent, CancellationToken cancellationToken) 
            where T : IUIPanel;
        void ReleasePanel(IUIPanel panel);
        UniTask PreloadPanelsAsync(string[] panelKeys, CancellationToken cancellationToken);
    }
    
    public sealed class UIResourceService : IUIResourceService, IDisposable
    {
        private readonly Dictionary<string, GameObject> _panelPrefabCache = new();
        private readonly Dictionary<IUIPanel, GameObject> _instantiatedPanels = new();
        private readonly Dictionary<string, AssetReference> _panelReferences;
        
        public UIResourceService()
        {
            // Initialize with addressable references or Resources paths
            _panelReferences = new Dictionary<string, AssetReference>
            {
                ["MainMenu"] = new AssetReference("UI/MainMenuPanel"),
                ["WinPanel"] = new AssetReference("UI/WinPanel"),
                ["LosePanel"] = new AssetReference("UI/LosePanel"),
                ["PausePanel"] = new AssetReference("UI/PausePanel"),
                ["LoadingPanel"] = new AssetReference("UI/LoadingPanel"),
                ["GameplayPanel"] = new AssetReference("UI/GameplayPanel"),
            };
        }
        
        public async UniTask<T> LoadPanelAsync<T>(string panelKey, Transform parent, CancellationToken cancellationToken) 
            where T : IUIPanel
        {
            try
            {
                // Check cache first
                if (!_panelPrefabCache.TryGetValue(panelKey, out var prefab))
                {
                    // Load from Addressables or Resources
                    prefab = await LoadPrefabAsync(panelKey, cancellationToken);
                    _panelPrefabCache[panelKey] = prefab;
                }
                
                // Instantiate panel
                var instance = Object.Instantiate(prefab, parent);
                var panel = instance.GetComponent<T>();
                
                if (panel == null)
                {
                    Object.Destroy(instance);
                    throw new InvalidOperationException($"Panel {panelKey} does not have component of type {typeof(T)}");
                }
                
                _instantiatedPanels[panel] = instance;
                instance.SetActive(false);
                
                return panel;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load panel {panelKey}: {ex.Message}");
                throw;
            }
        }
        
        public void ReleasePanel(IUIPanel panel)
        {
            if (_instantiatedPanels.TryGetValue(panel, out var gameObject))
            {
                Object.Destroy(gameObject);
                _instantiatedPanels.Remove(panel);
            }
        }
        
        public async UniTask PreloadPanelsAsync(string[] panelKeys, CancellationToken cancellationToken)
        {
            var tasks = new List<UniTask>();
            
            foreach (var key in panelKeys)
            {
                if (!_panelPrefabCache.ContainsKey(key))
                {
                    tasks.Add(LoadAndCachePrefabAsync(key, cancellationToken));
                }
            }
            
            await UniTask.WhenAll(tasks);
        }
        
        private async UniTask LoadAndCachePrefabAsync(string panelKey, CancellationToken cancellationToken)
        {
            var prefab = await LoadPrefabAsync(panelKey, cancellationToken);
            _panelPrefabCache[panelKey] = prefab;
        }
        
        private async UniTask<GameObject> LoadPrefabAsync(string panelKey, CancellationToken cancellationToken)
        {
            // Use Addressables if available, fallback to Resources
            #if USE_ADDRESSABLES
            if (_panelReferences.TryGetValue(panelKey, out var reference))
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(reference);
                return await handle.WithCancellation(cancellationToken);
            }
            #endif
            
            // Fallback to Resources
            var resourcePath = $"UI/{panelKey}";
            var request = Resources.LoadAsync<GameObject>(resourcePath);
            await request.WithCancellation(cancellationToken);
            
            if (request.asset == null)
            {
                throw new InvalidOperationException($"Failed to load panel prefab: {resourcePath}");
            }
            
            return request.asset as GameObject;
        }
        
        public void Dispose()
        {
            foreach (var panel in _instantiatedPanels.Values)
            {
                if (panel != null)
                {
                    Object.Destroy(panel);
                }
            }
            
            _instantiatedPanels.Clear();
            _panelPrefabCache.Clear();
        }
    }
}