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

        public BoardGame? GetGameById(int id) =>
            GetAllGames().FirstOrDefault(g => g.Id == id);


        public IEnumerable<BoardGame> FilterGames(string nameFilter)
        {
            if (string.IsNullOrWhiteSpace(nameFilter))
                return boardGames;

            var filter = nameFilter.Trim();

            if (int.TryParse(filter, out int players))
            {
                return boardGames.Where(g =>
                    g.MinPlayers <= players && players <= g.MaxPlayers);
            }

            return boardGames.Where(g =>
                g.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
