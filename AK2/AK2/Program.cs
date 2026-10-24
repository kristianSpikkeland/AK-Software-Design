using AK2.Client;
using AK2.Components;
using AK2.Components.Shared;
using AK2.Data;
using AK2.Dummy;
using AK2.ServerService;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registers Storage service
builder.Services.AddSingleton<GamesStorageAndQuery>();

// Registers Dummy service
builder.Services.AddScoped<DummyGames>();

builder.Services.AddScoped<GameNightState>();

builder.Services.AddScoped<ServerGamesService>();

builder.Services.AddHttpClient<GameApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();




// API 

// Creates a group. Authorization can be added later
var gamesApi = app.MapGroup("/api/games");

gamesApi.MapPost("", async (CreateGameRequest request, ServerGamesService gameService) =>
{
    try
    {
        var game = await gameService.CreateGameAsync(request);

        return Results.Created($"/api/games/{game.Id}", game);
    }

    catch (ArgumentException exception)
    {
        return Results.BadRequest(new
        {
            error = exception.Message
        });
    }
});


gamesApi.MapGet("", async (ServerGamesService gameService) =>
{
    var games = await gameService.GetAllGamesAsync();
    return Results.Ok(games);
});



gamesApi.MapGet("{id:int}", async (int id, ServerGamesService gameService) =>
{
    var game = await gameService.GetGameByIdAsync(id);
    return game is null ? Results.NotFound() : Results.Ok(game);
});


gamesApi.MapGet("{id:int}/totalRatingByGame", async (int id, ServerGamesService gameService) =>
{
    var rating = await gameService.GetTotalRatingByGame(id);
    return Results.Ok(rating);
});


// Updates the whole game. 200 with the updated game, 400 on invalid data, 404 when the id does not exist
gamesApi.MapPut("{id:int}", async (int id, UpdateGameRequest request, ServerGamesService gamesService) =>
{
    try
    {
        var game = await gamesService.UpdateGameAsync(id, request);

        return game is null ? Results.NotFound(new { error = "Spillet finnes ikke" }) : Results.Ok(game);
    }

    catch (ArgumentException exception)
    {
        return Results.BadRequest(new
        {
            error = exception.Message
        });
    }
});


// Updates game category
gamesApi.MapPut("{id:int}/category", async (int id, ServerGamesService gamesService, UpdateCategoryRequest request) =>
{
    try
    {
        var updated = await gamesService.UpdateCategoryAsync(id, request);

        return updated ? Results.NoContent() : Results.NotFound(new { error = "Spillet finnes ikke" });
    }

    catch (ArgumentException exception)
    {
        return Results.BadRequest(new
        {
            error = exception.Message
        });
    }
});

gamesApi.MapPut("{id:int}/rating", async (int id, ServerGamesService gamesService, UpdateRatingRequest request) =>
{
    try
    {
        var updated = await gamesService.UpdateRatingAsync(id, request);

        return updated ? Results.NoContent() : Results.NotFound(new { error = "Spillet finnes ikke" });
    }

    catch (ArgumentException exception)
    {
        return Results.BadRequest(new
        {
            error = exception.Message
        });
    }
});


gamesApi.MapDelete("{id:int}", async (int id, ServerGamesService gamesService) =>
{
    var deleted = await gamesService.DeleteGameAsync(id);

    return deleted ? Results.NoContent() : Results.NotFound(new { error = "Spillet finnes ikke" });
}); 



using (var scope = app.Services.CreateScope())
{
    var dummy = scope.ServiceProvider.GetRequiredService<DummyGames>();
    await dummy.CreateDummyGames();
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
