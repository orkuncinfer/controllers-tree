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
        void RegisterPanel(string panelId, IUIPanel panel);
        void UnregisterPanel(string panelId);
        UniTask<IUIPanel> GetPanelAsync(string panelId, CancellationToken cancellationToken);
        UniTask EnqueuePanelAsync(string panelId, CancellationToken cancellationToken);
    }
    
    public sealed class UIService : IUIService
    {
        private readonly Dictionary<string, IUIPanel> _panels = new();
        private readonly Queue<(string panelId, UniTaskCompletionSource tcs)> _panelQueue = new();
        private bool _isProcessingQueue;
        
        public void RegisterPanel(string panelId, IUIPanel panel)
        {
            _panels[panelId] = panel;
        }
        
        public void UnregisterPanel(string panelId)
        {
            _panels.Remove(panelId);
        }
        
        public async UniTask<IUIPanel> GetPanelAsync(string panelId, CancellationToken cancellationToken)
        {
            if (_panels.TryGetValue(panelId, out var panel))
            {
                return panel;
            }
            
            // Wait for panel to be registered
            while (!cancellationToken.IsCancellationRequested)
            {
                await UniTask.Yield(cancellationToken);
                if (_panels.TryGetValue(panelId, out panel))
                {
                    return panel;
                }
            }
            
            throw new OperationCanceledException();
        }
        
        public async UniTask EnqueuePanelAsync(string panelId, CancellationToken cancellationToken)
        {
            var tcs = new UniTaskCompletionSource();
            _panelQueue.Enqueue((panelId, tcs));
            
            if (!_isProcessingQueue)
            {
                ProcessQueueAsync(cancellationToken).Forget();
            }
            
            await tcs.Task;
        }
        
        private async UniTaskVoid ProcessQueueAsync(CancellationToken cancellationToken)
        {
            _isProcessingQueue = true;
            
            while (_panelQueue.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                var (panelId, tcs) = _panelQueue.Dequeue();
                
                try
                {
                    var panel = await GetPanelAsync(panelId, cancellationToken);
                    await panel.ShowAsync(cancellationToken);
                    
                    // Wait for panel to close
                    await UniTask.WaitUntil(() => !panel.GameObject.activeSelf, 
                        cancellationToken: cancellationToken);
                    
                    tcs.TrySetResult();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }
            
            _isProcessingQueue = false;
        }
    }
}