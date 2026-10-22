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

        // The storage hands out ids, like a database would
        private int nextId = 1;

        // Create
        public async Task CreateGameAsync(string name, int minPlayers, int maxPlayers, string category)
        {
            boardGames.Add(new BoardGame(name, minPlayers, maxPlayers, category)
            {
                Id = nextId++
            });
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
        public Task<bool> UpdateCategoryAsync(int id, string category)
        {
            var game = boardGames.FirstOrDefault(g => g.Id == id);

            if (game is null)
                return Task.FromResult(false);

            game.Category = category;
            return Task.FromResult(true);
        }

        // Delete by ud
        public Task<bool> DeleteGameAsync(int id)
        {
            var removed = boardGames.RemoveAll(g => g.Id == id) > 0;
            return Task.FromResult(removed);
        }


      

     

    }
}
