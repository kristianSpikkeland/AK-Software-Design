using AK2.Components.Pages;
using AK2.Data;
using AK2.Dummy;
using AK2.Models;
using AK2.Filtering;

namespace AK2Test
{
    public class Oppgave1
    {
        private readonly GamesStorageAndQuery _query;
        private readonly DummyGames _dummy;


        public Oppgave1()
        {
            _query = new GamesStorageAndQuery();
            _dummy = new DummyGames(_query);
        }


        // Creates the dummy games and returns them as a list
        private async Task<List<BoardGame>> GetDummyGamesAsync()
        {
            await _dummy.CreateDummyGames();
            return await _query.GetAllGamesAsync();
        }

        [Fact]
        public async Task FilterGames_WhenNoMatchOnName_ShouldReturnEmpty()
        {
            // Arrange
            var games = await GetDummyGamesAsync();

            // Act
            var filter = GameFilter.FilterGames("q", games);
            

            // Assert
            Assert.Empty(filter);
        }

        [Fact]
        public async Task FilterGames_WhenMatcOnName_ShouldNotReturnEmpty()
        {
            // Arrange
            var games = await GetDummyGamesAsync();


            // Act
            var monopolFilter = GameFilter.FilterGames("Monopol", games);


            // Assert
            Assert.NotEmpty(monopolFilter);
        }


        [Fact]
        public async Task FilterGames_WhenMatcOnName_ShoulReturnRightAmount()
        {
            // Arrange
            var games = await GetDummyGamesAsync();

            // Act
            var monopolFilter = GameFilter.FilterGames("Monopol", games);


            // Assert
            Assert.Equal(1, monopolFilter.Count());
        }


        // This method checks that the filter is case insensitive
        [Theory]
        [InlineData("mOnOpOl")]
        [InlineData("RISK")]
        [InlineData("settlers")]
        public async Task FilterGames_WhenMixInUpperAndLowerCase_ShouldNotCare(string value)
        {
            // Arrange
            var games = await GetDummyGamesAsync();

            // Act
            var filter = GameFilter.FilterGames(value, games);

            // Assert
            Assert.NotEmpty(filter);
        }

        [Theory]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        [InlineData("5")]
        public async Task FilterGames_WhenMatchOnNumberOfPlayers_ShouldNotReturnEmpty(string value)
        {
            // Arrange
            var games = await GetDummyGamesAsync();

            // Act

            var filter = GameFilter.FilterGames(value, games);


            // Assert
            Assert.NotEmpty(filter);
        }


        [Theory]
        [InlineData("0")]
        [InlineData("100")]
        public async Task FilterGames_WhenNoMatchOnNumberOfPlayers_ShouldReturnEmpty(string value)
        {
            // Arrange
            var games = await GetDummyGamesAsync();

            // Act
            var filter = GameFilter.FilterGames(value, games);

            // Assert
            Assert.Empty(filter);
        }
    }
}
