using AK2.Client;
using AK2.Components.Shared;
using AngleSharp.Html;
using Microsoft.Extensions.DependencyInjection;

namespace AK2Test
{
    public class DiceRatingTests: BunitContext
    {
        [Fact]
        public void SaveAsync_SendsValue()
        {
            // Arrange
            int? receivedValue = null;

            // Adding GameApiClient since total rating is fetched in OnInitializedAsync
            Services.AddSingleton(new GameApiClient(new HttpClient { BaseAddress = new Uri("http://localhost/") }));

            // Setting start value to 3
            var cut = Render<DiceRating>(parameters => parameters
                .Add(p => p.Value, 3)
                .Add(p => p.ReadOnly, false)
                .Add(p => p.ValueChanged, (int newValue) => receivedValue = newValue));


            // Act
            cut.Find("input").Change("5");
            cut.Find("button").Click();

            // Assert
            Assert.Equal(5, receivedValue);
        }


        [Theory]
        [InlineData(7)]
        [InlineData(0)]
        [InlineData(-1)]
        public void SaveAsync_DoesNotSendValue_IfValueIsInvalid(int value)
        {
            // Arrange
            int? receivedValue = null;

            // Adding GameApiClient since total rating is fetched in OnInitializedAsync
            Services.AddSingleton(new GameApiClient(new HttpClient { BaseAddress = new Uri("http://localhost/") }));

            // Setting start value to 3
            var cut = Render<DiceRating>(parameters => parameters
                .Add(p => p.Value, 3)
                .Add(p => p.ReadOnly, false)
                .Add(p => p.ValueChanged, (int newValue) => receivedValue = newValue));


            // Act
            cut.Find("input").Change(value.ToString());
            cut.Find("button").Click();

            // Assert
            Assert.Null(receivedValue);
        }
    }
}
