using AK2.Models;
using AK2.Data;

namespace AK2.Dummy
{
    public class DummyGames(GamesStorageAndQuery gamesQuery)
    {
        public async Task CreateDummyGames()
        {

            await gamesQuery.CreateGameAsync("Monopol", 2, 6, "Economics");
            await gamesQuery.CreateGameAsync("Den forsvunnede diamant", 2, 5, "Adventure");
            await gamesQuery.CreateGameAsync("Settlers", 2, 8, "Adventure");
            await gamesQuery.CreateGameAsync("Risk", 2, 6, "Adventure");
            await gamesQuery.CreateGameAsync("Exploding kittens", 2, 5, "Logic");
            await gamesQuery.CreateGameAsync("Lars Monsen spillet", 2, 8, "Adventure");
            await gamesQuery.CreateGameAsync("Catch the fire", 2, 10, "Stress");
            await gamesQuery.CreateGameAsync("Who are you", 2, 6, "Identity");

        }
    }
}
