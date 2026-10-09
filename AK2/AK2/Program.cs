using AK2.Components;
using AK2.Data;
using AK2.Dummy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registers Storage service
builder.Services.AddSingleton<GamesStorageAndQuery>();

// Registers Dummy service
builder.Services.AddScoped<DummyGames>();

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

using (var scope = app.Services.CreateScope())
{
    var dummy = scope.ServiceProvider.GetRequiredService<DummyGames>();
    dummy.CreateDummyGames();
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
