using AK2.Components.Pages;
using AK2.Models;

namespace AK2.Data
{
    public class GamesStorageAndQuery
    {
        public List<BoardGame> boardGames { get; set; } = new();

        public async Task<List<BoardGame>> GetAllGamesAsync()
        {
            return boardGames;
        }

        // Create
        public async Task CreateGameAsync(string name, int minPlayers, int maxPlayers, string category)
        {
            boardGames.Add(new BoardGame(name, minPlayers, maxPlayers, category));
        }

        // Read by name
        public async Task<BoardGame?> GetGameByNameAsync(string name)
        {
            return boardGames.FirstOrDefault(b => b.Name == name);
        }

        // Read by Id
        public async Task<BoardGame?> GetGameByIdAsync(int id)
        {
            var games = await GetAllGamesAsync();
            return games.FirstOrDefault(g => g.Id == id);
        }
           

        // Update category   
        public async Task UpdateCategoryAsync(string name, string category)
        {
            var game = await GetGameByNameAsync(name);
            game.Category = category;
        }

        // Delete by ud
        public Task<bool> DeleteGameAsync(int id)
        {
            var removed = boardGames.RemoveAll(g => g.Id == id) > 0;
            return Task.FromResult(removed);
        }



        public async Task <BoardGame?> GetRandomGameAsync()
        {
            if (boardGames.Count == 0)
                return null;

            int index = Random.Shared.Next(boardGames.Count);
            return boardGames[index];
        }

        


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
