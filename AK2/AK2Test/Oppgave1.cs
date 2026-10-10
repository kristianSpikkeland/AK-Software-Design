using AK2.Components.Pages;
using AK2.Data;
using AK2.Dummy;
using AK2.Models;

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

        [Fact]
        public void FilterGames_WhenNoMatchOnName_ShouldReturnEmpty()
        {      
            // Act
            _dummy.CreateDummyGames();
            var filter = _query.FilterGames("q");
            var games = _query.GetAllGames();

            // Assert
            Assert.Empty(filter);
        }

        [Fact]
        public void FilterGames_WhenMatcOnName_ShouldNotReturnEmpty()
        {
            // Act
            _dummy.CreateDummyGames();
            var monopolFilter = _query.FilterGames("Monopol");
            var games = _query.GetAllGames();

            // Assert
            Assert.NotEmpty(monopolFilter);
        }


        [Fact]
        public void FilterGames_WhenMatcOnName_ShoulReturnRightAmount()
        {
            // Act
            _dummy.CreateDummyGames();
            var monopolFilter = _query.FilterGames("Monopol");
            var games = _query.GetAllGames();

            // Assert
            Assert.Equal(1, monopolFilter.Count());
        }


        // This method checks that the filter is case insensitive
        [Theory]
        [InlineData("mOnOpOl")]
        [InlineData("RISK")]
        [InlineData("settlers")]
        public void FilterGames_WhenMixInUpperAndLowerCase_ShouldNotCare(string value)
        {
            // Act
            _dummy.CreateDummyGames();
            var filter = _query.FilterGames(value);
            var games = _query.GetAllGames();

            // Assert
            Assert.NotEmpty(filter);
        }

        [Theory]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        [InlineData("5")]
        public void FilterGames_WhenMatchOnNumberOfPlayers_ShouldNotReturnEmpty(string value)
        {
            // Act
            _dummy.CreateDummyGames();

            var filter = _query.FilterGames(value);

            var games = _query.GetAllGames();

            // Assert
            Assert.NotEmpty(filter);
        }


        [Theory]
        [InlineData("0")]
        [InlineData("100")]
        public void FilterGames_WhenNoMatchOnNumberOfPlayers_ShouldReturnEmpty(string value)
        {
            // Act
            _dummy.CreateDummyGames();

            var filter = _query.FilterGames(value);

            var games = _query.GetAllGames();

            // Assert
            Assert.Empty(filter);
        }
    }
}
