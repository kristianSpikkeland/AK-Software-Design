# Fra Blazor Server til Blazor Web App med server **og** klient

Denne versjonen bruker vanlig Markdown uten farger eller HTML-styling.

- **Beholdt** betyr kode fra det opprinnelige Blazor Server-opplegget.
- **Ny eller endret** betyr kode som trengs for `InteractiveAuto`, WebAssembly-klienten eller API-et.

Dette dokumentet bygger videre på opplegget **«Fra MVC til Blazor Server»**.

Målet er ikke å lage den mest avanserte arkitekturen. Målet er å vise én ting tydelig:

> Den samme `Products.razor`-komponenten kan bruke én felles kontrakt, men få data på to forskjellige måter:
>
> - direkte fra databasen når komponenten kjører på serveren
> - via HTTP og JSON når komponenten kjører i nettleseren


---

## 0. Er dette fornuftig?

For det lille produktprosjektet er ren `InteractiveServer` den enkleste løsningen. Alle viktige operasjoner bruker serveren uansett:

- autentisering
- database
- oppretting av produkter

`InteractiveAuto` er likevel pedagogisk nyttig fordi det lærer bort:

- forskjellen på kode som kjører på serveren og i nettleseren
- hvorfor klienten trenger et API
- hvordan JSON overfører data
- hvorfor et interface kan skjule hvor dataene kommer fra
- hvorfor databasekode aldri skal ligge i klientprosjektet

Vi gjør derfor bare **produktsiden** i stand til å kjøre begge steder. Login, registrering og logout beholdes på serveren.

---

## 1. Mentalmodellen

### Før: bare Blazor Server

```text
Products.razor
    ↓
Sql
    ↓
MySQL
```

Komponenten og all C#-kode kjører på serveren. Nettleseren viser brukergrensesnittet gjennom Blazor-forbindelsen.

### Etter: `InteractiveAuto`

Ved første besøk kan komponenten kjøre på serveren:

```text
Products.razor
    ↓
IProductService
    ↓
ServerProductService
    ↓
Sql
    ↓
MySQL
```

Etter at WebAssembly-pakken er lastet ned og lagret i nettleseren, kan et senere besøk kjøre komponenten i nettleseren:

```text
Products.razor i nettleseren
    ↓
IProductService
    ↓
ClientProductService
    ↓
HTTP + JSON
    ↓
/api/products på serveren
    ↓
ServerProductService
    ↓
Sql
    ↓
MySQL
```

Det viktige er at `Products.razor` bare kjenner `IProductService`. Komponenten trenger ikke vite hvilken implementasjon den har fått.

---

## 2. Opprett prosjektet

Opprett en **Blazor Web App** med:

- Framework: `.NET 10`
- Authentication type: `None`
- Interactive render mode: `Auto`
- Interactivity location: `Per page/component`

CLI:

```bash
dotnet new blazor -n W3 --interactivity Auto
```

Dette gir normalt to prosjekter:

```text
W3/
├── W3/                 serverprosjektet
└── W3.Client/          kode som kan lastes ned til nettleseren
```

Vi lager ikke et tredje Contracts-prosjekt. For å holde undervisningen enkel legger vi de delte kontraktene i `W3.Client`. Serverprosjektet refererer allerede til klientprosjektet.

---

## 3. Endelig, forenklet prosjektstruktur

```text
W3/
├── W3/
│   ├── Components/
│   │   ├── Pages/
│   │   │   ├── Home.razor
│   │   │   ├── Login.razor
│   │   │   ├── Register.razor
│   │   │   └── Logout.razor
│   │   ├── RedirectToLogin.razor
│   │   ├── Routes.razor
│   │   └── _Imports.razor
│   ├── Infra/
│   │   └── Sql.cs
│   ├── Models/
│   │   ├── Product.cs
│   │   └── User.cs
│   ├── Services/
│   │   └── ServerProductService.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── W3.csproj
│
├── W3.Client/
│   ├── Contracts/
│   │   ├── ProductDto.cs
│   │   ├── CreateProductRequest.cs
│   │   └── IProductService.cs
│   ├── Pages/
│   │   └── Products.razor
│   ├── Services/
│   │   └── ClientProductService.cs
│   ├── Program.cs
│   └── _Imports.razor
│
└── docker-compose.yml
```

---

# DEL A — DET SOM BEHOLDES

## 4. Filer som kopieres uendret


Disse filene beholdes akkurat som i W2 og det opprinnelige undervisningsopplegget:

- `W3/Infra/Sql.cs`
- `W3/Models/Product.cs`
- `W3/Models/User.cs`
- `W3/appsettings.json`
- `docker-compose.yml`

Det opplastede dokumentet beskriver at filene skal kopieres uendret, men inneholder ikke hele kildekoden deres. Derfor blir de ikke diktet opp her. Kopier de faktiske filene fra W2.


Serverprosjektet beholder pakkene:


```
<ItemGroup>
  <PackageReference Include="Dapper" Version="2.1.79" />
  <PackageReference Include="MySqlConnector" Version="2.6.1" />
</ItemGroup>
```

## 5. `W3/Components/Pages/Home.razor`

Denne siden er fortsatt en vanlig statisk serverside.


```
@page "/"

<h1>Velkommen</h1>

<p>
    <a href="/login">Logg inn</a> eller
    <a href="/register">registrer deg</a>
</p>
```

## 6. `W3/Components/RedirectToLogin.razor`

Denne beholdes enkel. `forceLoad: true` sikrer at login-siden får en vanlig HTTP-forespørsel.


```
@inject NavigationManager Nav

@code {
    protected override void OnInitialized()
        => Nav.NavigateTo("/login", forceLoad: true);
}
```

## 7. `W3/Components/Pages/Login.razor`

Login skal fortsatt være **statisk SSR**. Det er derfor ingen `@rendermode` her.

`HttpContext.SignInAsync` må skrive autentiseringscookien til en ekte HTTP-respons. Det kan ikke gjøres fra en komponent som allerede kjører interaktivt over SignalR eller WebAssembly.


```
@page "/login"
@using System.Security.Claims
@using Microsoft.AspNetCore.Authentication
@using Microsoft.AspNetCore.Authentication.Cookies
@using Microsoft.AspNetCore.Identity
@inject Sql Sql
@inject NavigationManager Nav

<h3>Login</h3>

@if (error is not null)
{
    <p style="color:red">@error</p>
}

<EditForm Model="Model" OnValidSubmit="LoginUser" FormName="login">
    <label>
        Brukernavn:
        <InputText @bind-Value="Model.Username" />
    </label>

    <label>
        Passord:
        <InputText type="password" @bind-Value="Model.Password" />
    </label>

    <button type="submit">Login</button>
</EditForm>

<p>Ny her? <a href="/register">Registrer deg</a></p>

@code {
    [CascadingParameter]
    public HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromForm]
    public LoginModel Model { get; set; } = new();

    private string? error;

    private readonly PasswordHasher<User> _hasher = new();

    private async Task LoginUser()
    {
        var user = await Sql.GetUserByUsername(Model.Username);

        if (user is null ||
            _hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                Model.Password)
            == PasswordVerificationResult.Failed)
        {
            error = "Wrong username and/or password";
            return;
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        Nav.NavigateTo("/products");
    }

    public class LoginModel
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
```

## 8. `W3/Components/Pages/Register.razor`

Registrering bruker databasen og blir på serveren. Den trenger ikke være WebAssembly for å lære poenget i dette prosjektet.


```
@page "/register"
@using Microsoft.AspNetCore.Identity
@inject Sql Sql
@inject NavigationManager Nav

<h3>Registrer</h3>

@if (error is not null)
{
    <p style="color:red">@error</p>
}

<EditForm Model="Model"
          OnValidSubmit="RegisterUser"
          FormName="register">

    <label>
        Brukernavn:
        <InputText @bind-Value="Model.Username" />
    </label>

    <label>
        Passord:
        <InputText type="password" @bind-Value="Model.Password" />
    </label>

    <button type="submit">Register</button>
</EditForm>

@code {
    [SupplyParameterFromForm]
    public RegisterModel Model { get; set; } = new();

    private string? error;

    private readonly PasswordHasher<User> _hasher = new();

    private async Task RegisterUser()
    {
        if (string.IsNullOrWhiteSpace(Model.Username) ||
            string.IsNullOrWhiteSpace(Model.Password))
        {
            error = "Username and/or password can not be empty";
            return;
        }

        if (await Sql.GetUserByUsername(Model.Username) is not null)
        {
            error = "This username is not available";
            return;
        }

        var passwordHash =
            _hasher.HashPassword(new User(), Model.Password);

        await Sql.CreateUser(
            Model.Username,
            passwordHash,
            Role.User);

        Nav.NavigateTo("/login");
    }

    public class RegisterModel
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
```

## 9. `W3/Components/Pages/Logout.razor`

Logout blir også på serveren fordi cookien skal fjernes fra en HTTP-respons.


```
@page "/logout"
@using Microsoft.AspNetCore.Authentication
@using Microsoft.AspNetCore.Authentication.Cookies
@inject NavigationManager Nav

@code {
    [CascadingParameter]
    public HttpContext HttpContext { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await HttpContext.SignOutAsync();

        Nav.NavigateTo("/login");
    }
}
```

# DEL B — NYTT FOR SERVER OG KLIENT

## 10. Hvorfor trenger vi DTO og interface?

Når komponenten kjører på serveren, kan den bruke `Sql` direkte gjennom en server-service.

Når komponenten kjører i nettleseren, kan den ikke koble seg til MySQL. Den må spørre serveren gjennom HTTP.

Derfor lager vi:

1. `ProductDto` — data som kan sendes som JSON
2. `CreateProductRequest` — data klienten sender ved oppretting
3. `IProductService` — den felles kontrakten komponenten bruker

Disse filene ligger i `W3.Client`, fordi de må finnes i koden som lastes ned til nettleseren. Serveren kan også bruke dem fordi serverprosjektet refererer til klientprosjektet.

---

## 11. `W3.Client/Contracts/ProductDto.cs`

DTO-en inneholder bare data som brukergrensesnittet trenger.


```
namespace W3.Client.Contracts;

public sealed record ProductDto(
    string Id,
    string Name,
    decimal Price);
```

## 12. `W3.Client/Contracts/CreateProductRequest.cs`

Denne klassen brukes av både skjemaet og API-et.


```
using System.ComponentModel.DataAnnotations;

namespace W3.Client.Contracts;

public sealed class CreateProductRequest
{
    [Required(ErrorMessage = "Navn må fylles ut.")]
    public string Name { get; set; } = "";

    [Range(
        typeof(decimal),
        "0.01",
        "999999999",
        ErrorMessage = "Prisen må være større enn null.")]
    public decimal Price { get; set; }
}
```

## 13. `W3.Client/Contracts/IProductService.cs`

Dette er nøkkelen i hele opplegget.

`Products.razor` bruker dette interfacet, ikke `Sql` og ikke `HttpClient` direkte.


```
namespace W3.Client.Contracts;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetProductsAsync();

    Task CreateProductAsync(
        CreateProductRequest request);
}
```

## 14. `W3/Services/ServerProductService.cs`

Denne implementasjonen brukes når komponenten kjører på serveren.

Den bruker den eksisterende `Sql`-klassen direkte.

Valideringen gjentas her fordi serveren alltid må kontrollere data. Man kan ikke stole på at klienten har kjørt validering.


```
using W2.Infra;
using W3.Client.Contracts;

namespace W3.Services;

public sealed class ServerProductService(Sql sql)
    : IProductService
{
    public async Task<IReadOnlyList<ProductDto>>
        GetProductsAsync()
    {
        var products = await sql.GetProducts();

        return products
            .Select(product => new ProductDto(
                Convert.ToString(product.Id),
                product.Name,
                product.Price))
            .ToArray();
    }

    public async Task CreateProductAsync(
        CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Produktnavnet kan ikke være tomt.");
        }

        if (request.Price <= 0)
        {
            throw new ArgumentException(
                "Prisen må være større enn null.");
        }

        await sql.CreateProduct(
            request.Name.Trim(),
            request.Price);
    }
}
```

## 15. `W3.Client/Services/ClientProductService.cs`

Denne implementasjonen brukes når komponenten kjører i nettleseren.

Her ser vi selve API-kallet:

- `GetFromJsonAsync` sender HTTP GET og gjør JSON om til C#-objekter.
- `PostAsJsonAsync` gjør C#-objektet om til JSON og sender HTTP POST.


```
using System.Net.Http.Json;
using W3.Client.Contracts;

namespace W3.Client.Services;

public sealed class ClientProductService(HttpClient http)
    : IProductService
{
    public async Task<IReadOnlyList<ProductDto>>
        GetProductsAsync()
    {
        return await http.GetFromJsonAsync<ProductDto[]>(
                   "api/products")
               ?? [];
    }

    public async Task CreateProductAsync(
        CreateProductRequest request)
    {
        var response = await http.PostAsJsonAsync(
            "api/products",
            request);

        response.EnsureSuccessStatusCode();
    }
}
```

## 16. `W3/Program.cs`

Dette er hele den nye serverfilen.

De viktigste nye delene er:

- `.AddInteractiveWebAssemblyComponents()`
- `.AddAuthenticationStateSerialization()`
- registrering av `ServerProductService`
- `/api/products`
- `.AddInteractiveWebAssemblyRenderMode()`
- klientens assembly legges til

API-et ligger direkte i `Program.cs` for å holde undervisningen samlet. Det kan flyttes til egne endpoint-klasser senere.


```
using Microsoft.AspNetCore.Authentication.Cookies;
using W2.Infra;
using W3.Client.Contracts;
using W3.Components;
using W3.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddScoped<Sql>(_ =>
    new Sql(
        builder.Configuration
            .GetConnectionString("default")!));

builder.Services.AddScoped<
    IProductService,
    ServerProductService>();

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/login";
        options.LoginPath = "/login";
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var sql = scope.ServiceProvider
        .GetRequiredService<Sql>();

    await sql.CreateUserTable();
    await sql.CreateProductTable();
}

var productsApi = app
    .MapGroup("/api/products")
    .RequireAuthorization();

productsApi.MapGet(
    "",
    async (IProductService productService) =>
    {
        var products =
            await productService.GetProductsAsync();

        return Results.Ok(products);
    });

productsApi.MapPost(
    "",
    async (
        CreateProductRequest request,
        IProductService productService) =>
    {
        try
        {
            await productService.CreateProductAsync(request);
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(W3.Client.Pages.Products).Assembly);

app.Run();
```

### Hva gjør JSON-delen?

Når nettleseren henter produkter, returnerer API-et omtrent dette:


```
[
  {
    "id": "1",
    "name": "Kaffe",
    "price": 39.90
  },
  {
    "id": "2",
    "name": "Te",
    "price": 29.50
  }
]
```

Ved oppretting sender nettleseren omtrent dette:


```
{
  "name": "Mineralvann",
  "price": 32.00
}
```

Du skriver ikke denne JSON-en manuelt. `System.Net.Http.Json` gjør konverteringen mellom C# og JSON.

---

## 17. `W3.Client/Program.cs`

Klientprosjektet trenger:

- en `HttpClient`
- autentiseringsstatusen som serveren har serialisert
- klientimplementasjonen av `IProductService`


```
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using W3.Client.Contracts;
using W3.Client.Services;

var builder =
    WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddScoped(_ =>
    new HttpClient
    {
        BaseAddress =
            new Uri(builder.HostEnvironment.BaseAddress)
    });

builder.Services.AddScoped<
    IProductService,
    ClientProductService>();

await builder.Build().RunAsync();
```

## 18. `W3/Components/Routes.razor`

Routeren må lete etter sider i både serverprosjektet og klientprosjektet.

`Products.razor` ligger i klientassemblyen.


``` Samme som før 
@using Microsoft.AspNetCore.Components.Authorization
<Router AppAssembly="typeof(Program).Assembly" AdditionalAssemblies="new[] { typeof(Client._Imports).Assembly }" NotFoundPage="typeof(Pages.NotFound)">
    <Found Context="routeData">
        <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" >
            <NotAuthorized>
                <RedirectToLogin/>
            </NotAuthorized>
        </AuthorizeRouteView>
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>

```

## 19. `W3/Components/_Imports.razor`

Dette er en full, enkel variant for serverprosjektet.


```
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.JSInterop
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using W2.Infra
@using W2.Models
@using W3
@using W3.Components
@using W3.Client.Contracts
```

## 20. `W3.Client/_Imports.razor`

Dette er en full, enkel variant for klientprosjektet.


```
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.JSInterop
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using W3.Client
@using W3.Client.Contracts
@using W3.Client.Services
```

## 21. `W3.Client/Pages/Products.razor`

Dette er den samme logiske produktsiden som før, men den injiserer nå `IProductService` i stedet for `Sql`.

Dette er den viktigste pedagogiske endringen:


```
@page "/products"
@rendermode InteractiveAuto
@attribute [Authorize]
@inject IProductService ProductService

<h3>Produkter</h3>

@if (error is not null)
{
    <p style="color:red">@error</p>
}
else if (products is null)
{
    <p>Laster...</p>
}
else
{
    <table border="1" cellpadding="8">
        <thead>
            <tr>
                <th>Id</th>
                <th>Navn</th>
                <th>Pris</th>
            </tr>
        </thead>

        <tbody>
            @foreach (var product in products)
            {
                <tr>
                    <td>@product.Id</td>
                    <td>@product.Name</td>
                    <td>@product.Price.ToString("N2")</td>
                </tr>
            }
        </tbody>
    </table>
}

<EditForm
    Model="NewProduct"
    OnValidSubmit="Create">

    <DataAnnotationsValidator />
    <ValidationSummary />

    <label>
        Navn:
        <InputText
            @bind-Value="NewProduct.Name" />
    </label>

    <label>
        Pris:
        <InputNumber
            @bind-Value="NewProduct.Price"
            step="0.01" />
    </label>

    <button type="submit">
        Legg til
    </button>
</EditForm>

<p>
    <a href="/logout"
       data-enhance-nav="false">
        Logg ut
    </a>
</p>

@code {
    private IReadOnlyList<ProductDto>? products;
    private string? error;

    private CreateProductRequest NewProduct { get; set; }
        = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadProducts();
    }

    private async Task LoadProducts()
    {
        try
        {
            products =
                await ProductService.GetProductsAsync();

            error = null;
        }
        catch
        {
            error = "Kunne ikke hente produkter";
        }
    }

    private async Task Create()
    {
        try
        {
            await ProductService.CreateProductAsync(
                NewProduct);

            NewProduct = new CreateProductRequest();

            await LoadProducts();
        }
        catch
        {
            error = "Kunne ikke opprette produktet";
        }
    }
}
```

# DEL C — HVA SOM FAKTISK ER LÆRINGSMÅLET

## 22. Samme interface, forskjellig implementasjon

På serveren registreres:


```
builder.Services.AddScoped<
    IProductService,
    ServerProductService>();
```

I klienten registreres:


```
builder.Services.AddScoped<
    IProductService,
    ClientProductService>();
```

Komponenten injiserer bare:


```
@inject IProductService ProductService
```

Dette kalles ofte **Dependency Inversion** eller programmering mot en abstraksjon:

```text
Products.razor
    kjenner IProductService
    kjenner ikke Sql
    kjenner ikke HttpClient
```

## 23. Når brukes API-et?

API-et brukes bare når `Products.razor` faktisk kjører som WebAssembly i nettleseren.

### Serverkjøring

```text
Products.razor
    → ServerProductService
    → Sql
```

Ingen JSON og ingen HTTP er nødvendig mellom komponenten og databasen.

### Klientkjøring

```text
Products.razor
    → ClientProductService
    → HTTP
    → JSON
    → API på serveren
```

Klientkoden kan ikke bruke:

- `Sql`
- MySQL-driveren
- connection string
- serverens filsystem
- hemmeligheter

## 24. Autentisering

Login og logout ligger fortsatt på serveren.

Serveren serialiserer et begrenset bilde av autentiseringsstatusen til klienten:


```
.AddAuthenticationStateSerialization()
```

Klienten leser denne statusen:


```
.AddAuthenticationStateDeserialization()
```

Det er fortsatt serveren som bestemmer om API-kallet er lovlig:


```
var productsApi = app
    .MapGroup("/api/products")
    .RequireAuthorization();
```

`[Authorize]` på `Products.razor` beskytter brukergrensesnittet. `.RequireAuthorization()` beskytter selve dataene og operasjonene.

---

## 25. Ting som med vilje er fjernet

Følgende ble utelatt fordi det skjulte hovedpoenget:

- eget `W3.Contracts`-prosjekt
- egen `ProductEndpoints`-klasse
- egen spesialklasse for servicefeil
- avansert parsing av HTTP-feilmeldinger
- omfattende logging
- retur-URL etter innlogging
- jobbkø, bakgrunnsarbeid og produksjonsoppsett

Dette kan legges til senere. Først bør studenten forstå denne kjeden:

```text
Razor-komponent
    → interface
    → server- eller klientimplementasjon
    → eventuelt API og JSON
```

---

## 26. Kjørerekkefølge

1. Start MySQL med eksisterende `docker-compose.yml`.
2. Start serverprosjektet `W3`.
3. Registrer en bruker.
4. Logg inn.
5. Åpne `/products`.
6. Opprett et produkt.
7. Last siden på nytt noen ganger.
8. Bruk nettleserens utviklerverktøy og se etter kall til `/api/products`.

På første kjøring kan produktsiden bruke serverimplementasjonen. Når WebAssembly-pakken er tilgjengelig og Auto velger klientkjøring ved et senere besøk, vil nettleserens Network-fane vise API-kallene.

---

## 27. Kort oppsummering

Den opprinnelige Blazor Server-versjonen gjorde dette:

```text
Products.razor → Sql
```

Den nye versjonen gjør dette:

```text
Products.razor → IProductService
```

Deretter avgjør Dependency Injection hvilken implementasjon som brukes:

```text
Server:
IProductService → ServerProductService → Sql

Klient:
IProductService → ClientProductService → HTTP/JSON → API
```

Det er hele poenget. Resten er støtte rundt denne grensen.
