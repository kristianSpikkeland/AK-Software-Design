using System.Net;

namespace AK2.Client
{
    public sealed class GameApiClient(HttpClient http)
    {
        public async Task<IReadOnlyList<GameDTO>> GetGamesAsync()
        {
            return await http.GetFromJsonAsync<GameDTO[]>("api/games") ?? [];
        }

        public async Task CreateGameAsync(CreateGameRequest request)
        {
            var response = await http.PostAsJsonAsync("api/games", request);
            response.EnsureSuccessStatusCode();
        } 

        public async Task UpdateCategoryAsync(UpdateCategoryRequest request)
        {
            var response = await http.PutAsJsonAsync("api/games", request);

            response.EnsureSuccessStatusCode();
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
    }
}
