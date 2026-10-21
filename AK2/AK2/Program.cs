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
        await gameService.CreateGameAsync(request);
        return Results.NoContent();
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


// Updates game category
gamesApi.MapPut("", async (UpdateCategoryRequest request, ServerGamesService gamesService) =>
{
    try
    {
        await gamesService.UpdateCategoryAsync(request);
        return Results.NoContent();
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

    return deleted ? Results.NoContent() : Results.NotFound();
}); 



using (var scope = app.Services.CreateScope())
{
    var dummy = scope.ServiceProvider.GetRequiredService<DummyGames>();
    await dummy.CreateDummyGames();
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
