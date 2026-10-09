using AK2.Models;
using AK2.Data;

namespace AK2.Dummy
{
    public class DummyGames(GamesStorageAndQuery gamesQuery)
    {
        public void CreateDummyGames()
        {

            gamesQuery.CreateGame("Monopol", 2, 6, "Economics");
            gamesQuery.CreateGame("Den forsvunnede diament", 2, 5, "Adventure");
            gamesQuery.CreateGame("Settlers", 2, 8, "Adventure");
            gamesQuery.CreateGame("Risk", 2, 6, "Adventure");
            gamesQuery.CreateGame("Exploding kittens", 2, 5, "Logic");
            
        }
    }
}
