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


        // Update category   
        public async Task<bool> UpdateCategoryAsync(int id, string category)
        {
            var game = boardGames.FirstOrDefault(g => g.Id == id);

            if (game is null)
                return false;

            game.Category = category;
            return true;
        }

        // Delete by ud
        public async Task<bool> DeleteGameAsync(int id)
        {
            return boardGames.RemoveAll(g => g.Id == id) > 0;
        }

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
