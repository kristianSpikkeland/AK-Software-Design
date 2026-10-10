using AK2.Components.Pages;
using AK2.Data;
using AK2.Dummy;
using AK2.Models;

namespace AK2Test
{
    public class UnitTest1
    {
        [Fact]
        public void FilterGames_WhenNoMatch_ShouldReturnEmpty()
        {
            // Arrange
            var query = new GamesStorageAndQuery();
            var dummy = new DummyGames(query);

            // Act
            dummy.CreateDummyGames();
            var monopolFilter = query.FilterGames("q");

            var games = query.GetAllGames();


            // Assert
            Assert.Empty(monopolFilter);
        }

        [Fact]
        public void FilterGames_WhenMatch_ShouldNotReturnEmpty()
        {
            // Arrange
            var query = new GamesStorageAndQuery();
            var dummy = new DummyGames(query);

            // Act
            dummy.CreateDummyGames();
            var monopolFilter = query.FilterGames("Monopol");

            var games = query.GetAllGames();


            // Assert
            Assert.NotEmpty(monopolFilter);
        }
    }
}
