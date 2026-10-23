using AK2.Models;
using System.Net;

namespace AK2.Client
{
    public sealed class GameApiClient(HttpClient http)
    {
        public async Task CreateGameAsync(CreateGameRequest request)
        {
            var response = await http.PostAsJsonAsync("api/games", request);
            response.EnsureSuccessStatusCode();
        } 

        public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
        {
            var response = await http.PutAsJsonAsync($"api/games/{id}/category", request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<bool> UpdateRatingAsync(int id, UpdateRatingRequest request)
        {
            var response = await http.PutAsJsonAsync($"api/games/{id}/rating", request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<bool> DeleteGameAsync(int id)
        {
            var response = await http.DeleteAsync($"api/games/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;

        }

        private static BoardGame ToBoardGame(GameDTO dto) =>
            new(dto.Name, dto.MinPlayers, dto.MaxPlayers, dto.Category)
            {
                Id = dto.Id,
                Rating = dto.Rating,
                Description = dto.Description
            };

        public async Task<BoardGame?> GetGameByIdAsync(int id)
        {
            var response = await http.GetAsync($"api/games/{id}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            var dto = await response.Content.ReadFromJsonAsync<GameDTO>();
            return dto is null ? null : ToBoardGame(dto);
        }

        public async Task<IReadOnlyList<BoardGame>> GetGamesAsync()
        {
            var dtos = await http.GetFromJsonAsync<GameDTO[]>("api/games") ?? [];
            return dtos.Select(ToBoardGame).ToArray();
        }

        public async Task<double?> GetTotalRatingByGame(int id)
        {
            var response = await http.GetAsync($"api/games/{id}/totalRatingByGame");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<double>();
        }
    }
}
