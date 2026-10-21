using AK2.Models;
using AK2.Data;

namespace AK2.Dummy
{
    public class DummyGames(GamesStorageAndQuery gamesQuery)
    {
        public async Task CreateDummyGames()
        {

            await gamesQuery.CreateGameAsync("Monopol", 2, 6, "Economics");
            await gamesQuery.CreateGameAsync("Den forsvunnede diament", 2, 5, "Adventure");
            await gamesQuery.CreateGameAsync("Settlers", 2, 8, "Adventure");
            await gamesQuery.CreateGameAsync("Risk", 2, 6, "Adventure");
            await gamesQuery.CreateGameAsync("Exploding kittens", 2, 5, "Logic");
            
        }
    }
}
