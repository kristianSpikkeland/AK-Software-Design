using AK2.Components.Pages;
using AK2.Models;

namespace AK2.Data
{
    public class GamesStorageAndQuery
    {
        public List<BoardGame> boardGames { get; set; } = new();

        public List<BoardGame> GetAllGames()
        {
            return boardGames;
        }

        public void CreateGame(string name, int minPlayers, int maxPlayers, string category)
        {
            boardGames.Add(new BoardGame(name, minPlayers, maxPlayers, category));
        }

        public IEnumerable<BoardGame> GetBoardGameByName(string boardGame)
        {
            return boardGames.Where(b => b.Name == boardGame);
        }

        public BoardGame? GetRandomGame()
        {
            if (boardGames.Count == 0)
                return null;

            int index = Random.Shared.Next(boardGames.Count);
            return boardGames[index];
        }
    }
}
