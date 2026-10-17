using AK2.Models;

namespace AK2.Components.Shared
{
    public class GameNightState
    {
        public int Count => games.Count;


        private readonly List<BoardGame> games = new();
        public IReadOnlyList<BoardGame> Games => games;
        public int TotalPlayTime => games.Sum(g => g.PlayTimeMinutes);


        // The bell
        public event Action? OnChange;

        public bool Contains(int gameId) => games.Any(g => g.Id == gameId);

        public bool Add(BoardGame game)
        {
            // Early return if BoradGame allready exists
            if (Contains(game.Id)) return false;

            games.Add(game);

            // Ring the bell
            OnChange?.Invoke();
            
            return true;            
        }

        public void Remove(int gameId)
        {
            games.RemoveAll(g => g.Id == gameId);
            OnChange?.Invoke();
        }


    }
}
