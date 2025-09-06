using System;

namespace Game.Models
{
    public interface IPlayerProgressModel
    {
        int CurrentLevel { get; }
        event Action<int> LevelChanged;
        void SetLevel(int level);
        void NextLevel();
    }
    
    public sealed class PlayerProgressModel : IPlayerProgressModel
    {
        private const string LEVEL_KEY = "CurrentLevel";
        
        public int CurrentLevel { get; private set; }
        public event Action<int> LevelChanged;
        
        public PlayerProgressModel()
        {
            CurrentLevel = UnityEngine.PlayerPrefs.GetInt(LEVEL_KEY, 1);
        }
        
        public void SetLevel(int level)
        {
            CurrentLevel = level;
            UnityEngine.PlayerPrefs.SetInt(LEVEL_KEY, level);
            LevelChanged?.Invoke(level);
        }
        
        public void NextLevel()
        {
            SetLevel(CurrentLevel + 1);
        }
    }
}