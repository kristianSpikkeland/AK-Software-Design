using AK2.Data;
using AK2.Models;

using AK2.Client;

namespace AK2.ServerService
{
    // This class acts like a service between the data and the client. Invalid data is rejected

    // Injecting GamesStorageAndQuery
    public sealed class ServerGamesService(GamesStorageAndQuery query)
    {

        public async Task CreateGameAsync(CreateGameRequest request)
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

            // If all validation passes, then its safe to call the data layer
            await query.CreateGameAsync(request.Name, request.MinPlayers, request.MaxPlayers, request.Category);

        }

        public async Task UpdateCategoryAsync(UpdateCategoryRequest request)
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

            // If all validation passes, then its safe to call the data layer
            await query.UpdateCategoryAsync(request.Name, request.Category);            
        }


        public async Task<IReadOnlyList<GameDTO>> GetAllGamesAsync()
        {
            var games = await query.GetAllGamesAsync();

            return games.Select(game => new GameDTO(
                game.Id,
                game.Name,
                game.Category,
                game.MinPlayers,
                game.MaxPlayers,
                game.Rating,
                game.Description))
                .ToArray();
        }

        public async Task<bool> DeleteGameAsync(int id)
        {
            return await query.DeleteGameAsync(id);
        }
    }
}
