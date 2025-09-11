using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "LevelList", menuName = "Game/Level List")]
    public class LevelList : ScriptableObject
    {
        [SerializeField] private List<string> _levelSceneNames = new List<string>();
        
        public IReadOnlyList<string> LevelSceneNames => _levelSceneNames;
        
        public string GetLevelName(int index)
        {
            if (_levelSceneNames.Count == 0) return null;
            
            // Wrap around to first level if we reach the end
            int wrappedIndex = index % _levelSceneNames.Count;
            return _levelSceneNames[wrappedIndex];
        }
        
        public int GetNextLevelIndex(int currentIndex)
        {
            if (_levelSceneNames.Count == 0) return 0;
            return (currentIndex + 1) % _levelSceneNames.Count;
        }
    }
}