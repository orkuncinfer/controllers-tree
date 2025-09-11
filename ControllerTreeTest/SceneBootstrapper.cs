using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Infrastructure
{
    /// <summary>
    /// Ensures the game can start from any scene
    /// </summary>
    public sealed class SceneBootstrapper : MonoBehaviour
    {
        private static bool _isBootstrapped;
        private static LifetimeScope _rootScope;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            _isBootstrapped = false;
        }
        
        private async void Awake()
        {
            if (_isBootstrapped)
            {
                Destroy(gameObject);
                return;
            }
            
            await BootstrapAsync();
        }
        
        private async UniTask BootstrapAsync()
        {
            // Check if we're already bootstrapped
            if (_isBootstrapped) return;
            
            _isBootstrapped = true;
            
            // Create root scope if it doesn't exist
            if (_rootScope == null)
            {
                var rootScopeGO = new GameObject("[RootLifetimeScope]");
                DontDestroyOnLoad(rootScopeGO);
                
                _rootScope = rootScopeGO.AddComponent<RootLifetimeScope>();
                await UniTask.Yield(); // Wait for scope to initialize
            }
            
            // Load boot scene additively if we're not in it
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (currentScene.name != "Boot")
            {
                await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                    "Boot", 
                    UnityEngine.SceneManagement.LoadSceneMode.Additive
                ).WithCancellation(destroyCancellationToken);
            }
        }
    }
}