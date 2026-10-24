using AK2.Data;
using AK2.Models;

using AK2.Client;

namespace AK2.ServerService
{
    // This class acts like a service between the data and the client. Invalid data is rejected

    // Injecting GamesStorageAndQuery
    public sealed class ServerGamesService(GamesStorageAndQuery query)
    {

        public async Task<GameDTO> CreateGameAsync(CreateGameRequest request)
        {
            // Adds validation
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                // Throw returns automaticly
                throw new ArgumentException("Navn kan ikke være tomt");
            }

            if (string.IsNullOrWhiteSpace(request.Category))
            {
                throw new ArgumentException("Kategori kan ikke være tom");
            }

            if (request.MinPlayers > request.MaxPlayers)
            {
                throw new ArgumentException("Minimum antall spiller kan ikke være større enn maksimum");
            }

            // If all validation passes, then its safe to call the data layer
            var game = await query.CreateGameAsync(request.Name, request.MinPlayers, request.MaxPlayers, request.Category);

            return ToDto(game);
        }

        public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
        {
            // Adds validation
            if (string.IsNullOrWhiteSpace(request.Category))
            {
                throw new ArgumentException("Kategori kan ikke være tom");
            }

            // If all validation passes, then its safe to call the data layer
            return await query.UpdateCategoryAsync(id, request.Category);
        }

        public async Task<bool> UpdateRatingAsync(int id, UpdateRatingRequest request)
        {
            // Adds validation
            if (request.Rating <1)
            {
                throw new ArgumentException("Vurdering kan ikke være mindre enn 1");
            }

            if (request.Rating > 6)
            {
                throw new ArgumentException("Vurdering kan ikke være større enn 6");
            }

            // If all validation passes, then its safe to call the data layer
            return await query.UpdateRatingAsync(id, request.Rating);
        }


        private static GameDTO ToDto(BoardGame game) => new(
            game.Id,
            game.Name,
            game.Category,
            game.MinPlayers,
            game.MaxPlayers,
            game.Rating,
            game.Description);


        public async Task<IReadOnlyList<GameDTO>> GetAllGamesAsync()
        {
            var games = await query.GetAllGamesAsync();
            return games.Select(ToDto).ToArray();
        }
  

        public async Task<GameDTO?> GetGameByIdAsync(int id)
        {
            var game = await query.GetGameByIdAsync(id);
            return game is null ? null : ToDto(game);
        }

        public async Task<bool> DeleteGameAsync(int id)
        {
            return await query.DeleteGameAsync(id);
        }

        public async Task<double> GetTotalRatingByGame(int id)
        {
            return await query.GetTotalRatingByGame(id);
        }
    }
}
