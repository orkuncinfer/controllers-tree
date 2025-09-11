using System;

namespace Game.Models
{
    public interface IPlayerProgressModel
    {
        int CurrentLevelIndex { get; }
        event Action<int> LevelChanged;
        void SetLevelIndex(int index);
        void NextLevel();
    }
    
    public sealed class PlayerProgressModel : IPlayerProgressModel
    {
        private const string LEVEL_INDEX_KEY = "CurrentLevelIndex";
        
        public int CurrentLevelIndex { get; private set; }
        public event Action<int> LevelChanged;
        
        public PlayerProgressModel()
        {
            CurrentLevelIndex = UnityEngine.PlayerPrefs.GetInt(LEVEL_INDEX_KEY, 0);
        }
        
        public void SetLevelIndex(int index)
        {
            CurrentLevelIndex = index;
            UnityEngine.PlayerPrefs.SetInt(LEVEL_INDEX_KEY, index);
            UnityEngine.PlayerPrefs.Save();
            LevelChanged?.Invoke(index);
        }
        
        public void NextLevel()
        {
            SetLevelIndex(CurrentLevelIndex + 1);
        }
    }
}