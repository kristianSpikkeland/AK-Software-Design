using AK2.Components.Pages;
using AK2.Models;

namespace AK2.Data
{
    public class GamesStorageAndQuery
    {
        public List<BoardGame> boardGames { get; set; } = new();
        public List<(int GameId, int Rating)> boardGamesRatings { get; set; } = new();

        public async Task<List<BoardGame>> GetAllGamesAsync()
        {
            return boardGames;
        }

        public async Task<double> GetTotalRatingByGame(int id)
        {
            var ratings = boardGamesRatings.Where(r => r.GameId == id).ToList();

            if (ratings.Count == 0)
                return 0;

            return ratings.Average(r => r.Rating);
        }


        // Updates the whole game. Returns null when the id does not exist
        public async Task<BoardGame?> UpdateGameAsync(int id, string name, int minPlayers, int
            maxPlayers, string category)
        {
            var game = boardGames.FirstOrDefault(g => g.Id == id);
            if (game is null)
                return null;
            game.Name = name;
            game.MinPlayers = minPlayers;
            game.MaxPlayers = maxPlayers;
            game.Category = category;
            return game;
        }


        // The storage hands out ids, like a database would
        private int nextId = 1;

        // Create
        public async Task<BoardGame> CreateGameAsync(string name, int minPlayers, int maxPlayers, string category)
        {
            var game = new BoardGame(name, minPlayers, maxPlayers, category)
            {
                Id = nextId++
            };

            boardGames.Add(game);
            return game;
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


        // Delete by ud
        public async Task<bool> DeleteGameAsync(int id)
        {
            return boardGames.RemoveAll(g => g.Id == id) > 0;
        }












        // Update category   
        public async Task<bool> UpdateCategoryAsync(int id, string category)
        {
            var game = boardGames.FirstOrDefault(g => g.Id == id);

            if (game is null)
                return false;

            game.Category = category;
            return true;
        }

        // Update rating
        public async Task<bool> UpdateRatingAsync(int id, int rating)
        {
            var game = boardGames.FirstOrDefault(g => g.Id == id);

            if (game is null)
                return false;

            game.Rating = rating;
            game.RatingCounter++;
            boardGamesRatings.Add((id, rating));

            return true;
        }


      

     

    }
}
