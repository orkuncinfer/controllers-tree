using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Infrastructure
{
    public enum SceneType
    {
        Boot,
        Level,
        Unknown
    }
    
    public static class SceneContext
    {
        public static SceneType GetCurrentSceneType()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            
            if (sceneName.Equals("Boot", System.StringComparison.OrdinalIgnoreCase))
                return SceneType.Boot;
            
            if (sceneName.StartsWith("Level", System.StringComparison.OrdinalIgnoreCase))
                return SceneType.Level;
            
            return SceneType.Unknown;
        }
        
        public static int GetLevelIndex()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            
            // Extract level number from scene name (e.g., "Level1" -> 0, "Level2" -> 1)
            if (sceneName.StartsWith("Level"))
            {
                var numberStr = sceneName.Replace("Level", "");
                if (int.TryParse(numberStr, out int levelNumber))
                {
                    return levelNumber - 1; // Convert to 0-based index
                }
            }
            
            return 0; // Default to first level
        }
    }
}