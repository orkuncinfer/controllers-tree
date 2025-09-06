using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Game.Services
{
    public interface ISceneService
    {
        UniTask LoadSceneAsync(string sceneName, CancellationToken cancellationToken);
        UniTask UnloadSceneAsync(string sceneName, CancellationToken cancellationToken);
    }
    
    public sealed class SceneService : ISceneService
    {
        public async UniTask LoadSceneAsync(string sceneName, CancellationToken cancellationToken)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive)
                .WithCancellation(cancellationToken);
        }
        
        public async UniTask UnloadSceneAsync(string sceneName, CancellationToken cancellationToken)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(sceneName)
                    .WithCancellation(cancellationToken);
            }
        }
    }
}