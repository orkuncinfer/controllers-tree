using System;

namespace Game.Models
{
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        GameOver
    }
    
    public interface IGameStateModel
    {
        GameState CurrentState { get; }
        event Action<GameState> StateChanged;
        void ChangeState(GameState newState);
    }
    
    public sealed class GameStateModel : IGameStateModel
    {
        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public event Action<GameState> StateChanged;
        
        public void ChangeState(GameState newState)
        {
            if (CurrentState != newState)
            {
                CurrentState = newState;
                StateChanged?.Invoke(newState);
            }
        }
    }
}