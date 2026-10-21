
# Oppgave 1

## Info 
Siden games har ansvar for å vise alle spill og bruker GameCard som er en gjenbrukbar komponent.
GamesDetail viser detaljene for et spill. På games siden er det lagt til lenker som man kan klikke på for å komme til detaljene for et spill.
Grunnen til at jeg valgte å gjøre det slik var siden jeg har forsøkt å tenke på brukeren og at brukergrensesnittet skal være minimalistisk og ryddig. 
På samme grunnlag er derfor filteret for antall spillere bakt inn sammen med navn filteret. 
Videre har man tenkt en del rundt hvordan man kan lage en løsning som skalerer. Det å bare ha en GamesDetails side istedenfor en side per spill
har jeg ansett som gunstig. 

## KI bruk
* Noe debugging av egenskrevet Linq spørring.
* En prompt: Beste måte for å generere stigende Id enkelt med konstruktør i C# klasse.
* Brukte IEnumerable<BoardGame> games = gamesQuery.GetAllGames() direkte. Fikk beskjed om at man må bruke OnInitialized metode.
* Fikk en lang feilmelding om at noe var galt første gang jeg kjørte programmet. Det viste seg at jeg hadde registrert en service
som var avhengig av en annen Service som ikke var registrert. Løsningen ble naturligvis å registrere denne servicen også.
*Fikk en ny feilmelding der det viste seg at jeg hadde glemt å bruke new og instansiere listen til brettspillene.

prompt: Hvordan kan jeg bygge filteret mitt slik at jeg kan søke på antall spillere også i samme søkefelt som for navnet. 
Skal finne treff for mellom min og maks spillere.

Svar: Du kan sjekke om teksten i søkefeltet er et tall. Er den det, finner du spill der tallet ligger mellom min og maks spillere. Er den ikke et tall, søker du på navn som før.

```C#
IEnumerable<BoardGame> FilteredItems
{
    get
    {
        if (string.IsNullOrWhiteSpace(nameFilter))
            return games;

        var filter = nameFilter.Trim();

        if (int.TryParse(filter, out int players))
        {
            return games.Where(g =>
                g.MinPlayers <= players && players <= g.MaxPlayers);
        }

        return games.Where(g =>
            g.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase));
    }
}
```

Et spørsmål rundt om man burde bruke = new() for spill man søker etter i Games.razor.
Fikk til svar at det skjuler feil og at det tryggeste er å la det være nullmed ? og heller gi brukeren beskjed  med eks: <p>Ingen spill funnet.</p>

Prompt: Skal ha en knapp der hvor jeg får tilfeldig spill skrevet ut når bruker trykker på knapp. Da blir det kanskje litt feil å ha GetRandomGame metoden min i OnInitialized() metoden?
Ja, det stemmer. OnInitialized() kjøres bare én gang når siden lastes, så der ville du fått ett tilfeldig spill som aldri endrer seg. Kallet skal ligge i en metode som knappen kjører ved klikk:

```C#
<button @onclick="ShowRandomGame">Tilfeldig spill</button>

@if (randomGame != null)
{
    <p>@randomGame.Name</p>
}

@code {
    private BoardGame? randomGame;

    private void ShowRandomGame()
    {
        randomGame = gamesQuery.GetRandomGame();
    }
}
```

Prøvde med: boardGames.Single(g => g.Id == index) for GetRandomGame metoden min men programmet krasjet om jeg klikket hurtig.
Fikk dette til svar:

Den beste løsningen er å trekke et tilfeldig element i stedet for en tilfeldig Id:
```
public BoardGame? GetRandomGame()
{
    if (games.Count == 0)
        return null;

    int index = Random.Shared.Next(games.Count);
    return games[index];
}
```
Random.Shared.Next(games.Count) gir et tall fra 0 til og med Count - 1, som alltid er en gyldig plass i listen. Da spiller det ingen rolle hva Id-ene er.

Prompt: Tenker at bruker skal klikke på Id en i oversikten over spillene og deretter bli navigert til riktig side som viser detaljer. Hvordan bør det gjøres?
Svar: Det enkleste er å gjøre Id-en (eller navnet) til en vanlig lenke til detaljsiden. Siden detaljsiden har ruten /games/{Id:int}, trenger lenken bare å peke dit med riktig Id.
```C#
<td><a href="/games/@g.Id">@g.Id</a></td>
```

Spurte litt rundt om det går ann å sentralisere mer av koden og feltene:
Svar: Kort sagt: hver komponent har sine egne felter, så de må deklareres der de brukes.

Men fikk et tips som jeg var enig i: Detaljsiden henter nå hele listen bare for å plukke ut ett spill. Det er ryddigere å flytte det oppslaget inn i tjenesten:

```C#
public BoardGame? GetGameById(int id) =>
    GetAllGames().FirstOrDefault(g => g.Id == id);
```

Opplevde problemer med at category ikke var synlig på GamesDetail siden min. Spurte KI og fikk til svar at Category ikke var lagret i konstruktør.

Prompt: Har flyttet filtreringen ut i en egen metode i StorageAndQuery klassen. Hva er beste praksis for å aktivere filteret i Games klassen?

Svar (utdrag): 

Filtrer når teksten endres (anbefalt)

Lagre resultatet i et felt, og oppdater det bare når brukeren skriver:
```html
<input type="search" @bind="nameFilter" @bind:event="oninput" @bind:after="ApplyFilter" />
```

```C#
private IEnumerable<BoardGame> filteredGames = [];

protected override void OnInitialized() => ApplyFilter();

private void ApplyFilter()
{
    filteredGames = gamesQuery.FilterGames(nameFilter).ToList();
}
```

Prompt: Hvordan kan man bruke foreach for visning av komponenter

Svar: Du skriver komponenten inne i @foreach på samme måte som vanlig HTML. Den lages da én gang per element i listen:

```C#
@foreach (var game in filteredGames)
{
    <GameCard Game="game" @key="game.Id" />
}
```
@key er valgfritt, men lurt i lister som endrer seg. Blazor bruker det til å holde styr på hvilket kort som hører til hvilket spill når listen filtreres. 
Uten @key kan Blazor gjenbruke feil kort når elementer forsvinner, og da kan tilstand inne i kortet havne på feil spill. Bruk noe unikt, som Id.

Det seg at jeg haddde laget fem parametre i GameCard istedenfor å bare gi et BoardGame objekt. Programmet krasjet når jeg klikket på spill linkene etter at jeg implementerte endringen uten at jeg skjønte hvorfor.
Spurte KI og fikk til svar at feilen lå hos GamesDetails som fortsatt brukte de gamle parameteren fra GameCard.


Noe hjelp med tester. Hadde en tanke om å ta en liste for filteret og en liste for alle spill og sammenligne innhold.
Etter litt chatting fikk jeg beskjed om at det var et blindspor. Valgte heller å bruke assert.Empty();

## Oppgave 2
Prompt: Hvordan henger EditForm sammen med Value og ValueChanged?

Svar: De er to forskjellige ting, men de bruker det samme mønsteret.

EditForm er rammen rundt et skjema. Den holder på et modellobjekt (f.eks. et BoardGame), validerer det og kaller OnValidSubmit når alt er gyldig. Selv har den ingen Value og ValueChanged.

Feltene inne i skjemaet har derimot Value og ValueChanged. Blazors egne felt, som InputText og InputNumber, er bygget på akkurat det samme mønsteret som DiceRating.
Derfor skriver du @bind-Value med stor V på dem:

```C#
<EditForm Model="game" OnValidSubmit="Save">
    <InputText @bind-Value="game.Name" />
    <InputNumber @bind-Value="game.MinPlayers" />
    <DiceRating @bind-Value="game.Rating" />
</EditForm>
````


Prompt: Tenkte å ha en metode som jeg kaller HandleAdd på Games siden  som jeg kobler til en EventCallback metode som ligge i GameCard.  
Foreløpig tenker jeg at den kun skal skrive ut en ekstra beskjed "spillet ble lagt til som skal være synlig i tre sekunder før den forsvinner. Hva er beste praksis?

Svar (Utrdag) EventCallback er naturligvis ikke en metode men et parameter.

Kjørte nye prompter der man ba om kodeeksempler.
Fikk til svar at det fantes problemstillinger som at en bruker klikker hurtig på knapppen forlater siden og at man 
kunne bruke cansellationToken. 

Ønsket ingen voldsom løsning på tidspunktet. 

Fikk til slutt denne koden som var det jeg ønsket meg:
```
@if (message is not null)
{
    <p>@message</p>
}

@code {
    private string? message;

    private async Task HandleAdd(BoardGame game)
    {
        message = "Spillet ble lagt til";
        await Task.Delay(3000);
        message = null;
    }
}
```

Hovedproblemet mitt var egentlig at jeg slet med hvordan man henter ut meldingen. Den første prompten burde adressert det bedre :)

Prompt: Har et problem. Min onclick skal gjøre to ting på en gang. 
Den skal kobles til metoden som sender signal til forelderen om at brukeren klikket og den skal samtidig oppdatere Count via Add metode som ligger i en GameNightState klasse. Svar veldig kort.

Svar: Gjør begge deler i forelderens HandleAdd, ikke i kortet. Kortet sender bare signalet, og forelderen oppdaterer Count:
```C#
@inject GameNightState GameNight

private async Task HandleAdd(BoardGame game)
{
    GameNight.Add();
    message = "Spillet ble lagt til";
    await Task.Delay(3000);
    message = null;
}
```

Prompt: Hvordan lager men en side som blir den som brukeren kommer til om en Id i URLen ikke finnes?

Svar: Lag selve siden
En helt vanlig side, for eksempel Pages/IkkeFunnet.razor:
```C#
@page "/ikke-funnet"

<h2>Fant ikke det du lette etter</h2>
<p>Spillet finnes ikke, eller adressen er feil.</p>
<a href="/spillhylla">← Tilbake til spillhylla</a>
```

Fortell Routeren hvilken side som er “ikke funnet”-siden, i Routes.razor (eller App.razor):
```C#
<Router AppAssembly="typeof(Program).Assembly" NotFoundPage="typeof(Pages.IkkeFunnet)">
    ...
</Router>
```

I spillsiden, når spillet ikke finnes:
```C#
@inject NavigationManager Nav

@code {
    protected override async Task OnParametersSetAsync()
    {
        game = await Games.GetByIdAsync(Id);
        if (game is null)
        {
            Nav.NotFound();
        }
    }
}
```
Opplevde at Til oversikt knappen min ikke funket på NotFound siden. Spurte KI og fikk til svar at jeg enten kunne gjøre siden interaktiv,
men at det beste var om man la det som en lenke og bruker @onclick når knappen skal gjøre noe, som å legge til spillkveld.

```C#<a href="/games" class="btn btn-primary">Til oversikt</a>
```

Skjønte ikke helt hva som menes med aktiv lenke og spurte KI om hva det var.
Videre fikk man kodeeksempel for hvordan man kan lage NavLinker til meny og tilhørende css. Ønsket venstrejustering av menyen. Legger ved denne KI genererte koden:

NavMenu.razor
```C#
<nav class="nav-menu">
    <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
        Forside
    </NavLink>

    <NavLink class="nav-link" href="Games">
        Spillhylla
    </NavLink>

    <NavLink class="nav-link" href="game-night">
        Min spillekveld 
    </NavLink>
</nav>
```

NavMenu.razor.css
```C#
.nav-menu {
    display: flex;
    flex-direction: column;
    justify-content: flex-start;
    gap: 0.5rem;
    padding: 1rem;
}

.nav-menu ::deep a {
    color: #333;
    padding: 0.5rem 0.75rem;
    border-radius: 4px;
    text-decoration: none;
}

.nav-menu ::deep a:hover {
    background-color: #e9ecef;
}

.nav-menu ::deep a.active {
    background-color: #0d6efd;
    color: white;
    font-weight: bold;
}


```

Ny prompt der jeg spurte om hvordan jeg burde legge til NavMenuen for alle sider der den er aktuell.

Fikk til svar at den kunne legges i MainLayout.razor og at alle sider vil få den som standard: 

MainLayout.razor
```C#
@inherits LayoutComponentBase

<div class="page">
    <aside class="sidebar">
        <NavMenu />
    </aside>

    <main class="content">
        @Body
    </main>
</div>
```

MainLayout.razor.css
```C#
.page {
    display: flex;          
}

.sidebar {
    width: 220px;
    background-color: #f8f9fa;
    border-right: 1px solid #dee2e6;
}

.content {
    flex: 1;               
    padding: 1.5rem;
}
```

Videre ble jeg forklart at om en side ikke skal ha menyen kan man gi den en egen layout med @layout øverst på siden.

Prompt: Har satt at bruker skriver inn i skjemaet et tall. Hvordan validere at det er mellom 1 og 6 og vise feilmelding om det er ugylidg verdi? 
Tenkte å bruke OnValidSubmit og koble den til en Save metode.

Svar: Ja, OnValidSubmit er riktig valg. Den kalles bare når alt er gyldig, så Save trenger ikke sjekke noe selv. Du trenger tre ting:

1. Regelen på modellen;
```C#
using System.ComponentModel.DataAnnotations;

public class BoardGame
{
    // ...
    [Range(1, 6, ErrorMessage = "Terningkastet må være fra 1 til 6")]
    public int Rating { get; set; }
}
```

2. Skjemaet
```C#
<EditForm Model="game" OnValidSubmit="Save" FormName="game-form">
    <DataAnnotationsValidator />

    <label>
        Terningkast
        <InputNumber @bind-Value="game.Rating" />
    </label>
    <ValidationMessage For="() => game.Rating" />

    <button type="submit">Lagre</button>
</EditForm>

@code {
    private BoardGame game = new();

    private async Task Save()
    {
        // Kommer du hit, er alle reglene oppfylt
        await GameApi.CreateAsync(game);
    }
}
```

Stusset litt på hvorfor ikke ValidationSummary ble brukt.
Fikk til svar: 
<ValidationMessage For="..." /> viser feilene for ett felt, typisk rett under det feltet.
<ValidationSummary /> viser alle feilene samlet i én liste.


## Oppgave 3
La til en bool verdi IsSelected for å holde styr på om et spill er valgt eller ikke.

Prompt: Sliter med at melding blir vist for alle spill og ikke kun spillet det gjelder. Hva er beste praksis?

Svar: Problemet er at du har én message-variabel for hele siden
Beste praksis på sikt: la GameCard eie meldingen selv:

GameCard.razor
```C#
@if (message is not null)
    {
        <p>@message</p>
    }

@code {

    private async Task Add()
    {
        message = BoardGame.IsSelected
            ? "Spillet er allerede lagt til"
            : "Spillet ble lagt til";

        await OnAdd.InvokeAsync(BoardGame);

        await Task.Delay(3000);
        message = null;
    }
```

Da blir HandleAdd på foreldresiden bare selve logikken:

```C#
private void HandleAdd(BoardGame game)
{
    if (game.IsSelected) return;
    game.IsSelected = true;
    GameNight.Add(game);
}
```

Det ante meg at dette ikke var i tråd med oppgavetekst. 
Ny prompt: Oppfyller det kravet i oppgavetekst knyttet til foreldre barn rolle.

Svar: Oppgave 2 sier at GameCard skal si fra via en EventCallback, at kortet ikke selv avgjør hva som skjer, og at det er forelderen som viser bekreftelsen. 
I variant 2 leser kortet IsSelected og bestemmer selv om meldingen er «lagt til» eller «allerede lagt til». 
Da tar barnet en beslutning som tilhører forelderen, og det bryter med «data ned, hendelser opp».
Dette svaret var som forventet :)

Videre fikk jeg forlart:
Koden din lagrer valget som game.IsSelected på selve BoardGame-objektet. Det strider mot oppgave 3 på to måter:

Spillekvelden skal ha ett hjem: GameNightState. Når «valgt» også ligger på spillet, har du to sannheter som kan komme ut av synk, for eksempel når du fjerner et spill fra spillekvelden.
I oppgave 4 hentes spillene fra API-et. Da blir IsSelected enten nullstilt ved hver henting eller delt mellom alle brukere hvis API-et lagrer det.

Spør heller tilstandsklassen, og send svaret ned til kortet som parameter:
Langt svar. Jeg hentet mye kode fra svaret. Se vedleggmappen og PDF dokument GameNight-foreldre-barn.pdf

Prompt: Tenker jeg skal lage en rød runding med svart tykk skrift som viser antall valgte spill til høyre i toppen av NavMenuen min. Hvordan gjør jeg det med html og css? Har laget en egen GameBadge.

```CSS
.gameNight-badge {
    display: inline-flex;          /* sentrerer tallet */
    align-items: center;
    justify-content: center;

    min-width: 1.75rem;            /* rund ved ett siffer */
    height: 1.75rem;
    padding: 0 0.4rem;             /* blir en «pille» ved to siffer */
    border-radius: 999px;          /* helt rund */

    background-color: #ff3b30;     /* rød */
    color: #000;                   /* svart */
    font-weight: 800;              /* tykk skrift */
    font-size: 0.9rem;
    line-height: 1;
}
```

```html
<div class="top-row ps-3 navbar navbar-dark">
    <div class="container-fluid">
        <a class="navbar-brand" href="">AK2</a>
        <GameBadge />
    </div>
</div>
```

Prompt: GameBadge blir oppdatert med nye tallverdier når den ligger på Games siden men ikke i NavMenyen.

Svar: Løsning: gjør hele appen interaktiv (anbefalt)
I Components/App.razor legger du til render-modus på Routes og HeadOutlet:
```C#
<HeadOutlet @rendermode="InteractiveServer" />
...
<Routes @rendermode="InteractiveServer" />
```

Hva er beste praksis for Blazor. Jeg skal ha legge til og fjern knapper på spilldetaljsiden også. 
Bare kjøre duplisert kode eller injecte fra Games siden?

Svar: Ingen av delene. Du kan ikke injecte en side i Blazor, bare tjenester. Logikken du ellers ville duplisert, ligger allerede i GameNightState. 
Spilldetaljsiden skal derfor injecte tilstandsklassen, akkurat slik Games.razor gjør.

Delt logikk: GameNightState
Alt som handler om hva som er valgt, ligger på ett sted: Contains, Add og Remove. 
Begge sidene kaller de samme metodene, så det blir ingen duplisert logikk. Denne gjenbruken er nettopp grunnen til at du har en tilstandsklasse.

Delt UI: en liten knapp-komponent
Selve knappene blir like på begge sidene. Det er derfor ryddig å trekke dem ut i en egen, «dum» komponent som bare viser og melder fra, i tråd med «data ned, hendelser opp»:

```C#
@* GameNightButton.razor *@
@if (IsSelected)
{
    <button class="btn btn-outline-danger" @onclick="() => OnRemove.InvokeAsync(Game)">Fjern fra spillekveld</button>
}
else
{
    <button class="btn btn-primary" @onclick="() => OnAdd.InvokeAsync(Game)">Legg til i spillekveld</button>
}

@code {
    [Parameter, EditorRequired] public BoardGame Game { get; set; } = default!;
    [Parameter] public bool IsSelected { get; set; }
    [Parameter] public EventCallback<BoardGame> OnAdd { get; set; }
    [Parameter] public EventCallback<BoardGame> OnRemove { get; set; }
}
```

Bruk den i GameCard i stedet for de to knappene du har der nå. Videresend parameterne fra kortet:
```C#
<GameNightButton Game="BoardGame" IsSelected="IsSelected" OnAdd="OnAdd" OnRemove="OnRemove" />
```

Spilldetaljsiden blir forelderen
```
@page "/games/{Id:int}"
@inject GameNightState GameNight

@if (game is not null)
{
    <h1>@game.Name</h1>
    @* ...detaljer... *@

    <GameNightButton Game="game"
                     IsSelected="GameNight.Contains(game.Id)"
                     OnAdd="HandleAdd"
                     OnRemove="HandleRemove" />

    @if (message is not null)
    {
        <p>@message</p>
    }
}

@code {
    [Parameter] public int Id { get; set; }
    private BoardGame? game;
    private string? message;

    protected override async Task OnParametersSetAsync()
    {
        game = /* hent spillet, f.eks. fra tjenesten/API-et */;
    }

    private async Task HandleAdd(BoardGame g)
    {
        message = GameNight.Add(g) ? "Spillet ble lagt til" : "Spillet er allerede lagt til";
        await Task.Delay(3000);
        message = null;
    }

    private void HandleRemove(BoardGame g) => GameNight.Remove(g.Id);
}
```

Videre fikk man et bra tips om å droppe:
```C#
public int Count { get; private set; }.
```
Og heller bruke:
```C#
public int Count => games.Count;
```
Da kan Count++ og Count -- fjernes. Mer stabilt


Slet med den justeringen etter om skjermen er liten og bred. Fikk beskjed av KI om at html koden i Games måtte pakkes i en div og gis klasse:
```C#
<div class="game-grid">
    @foreach (var game in filteredGames)
    {
        <div class="game-cell" @key="game.Id">
            <GameCard BoardGame="game"
                      IsSelected="GameNight.Contains(game.Id)"
                      OnAdd="HandleAdd"
                      OnRemove="HandleRemove" />

            @if (messages.TryGetValue(game.Id, out var msg))
            {
                <p>@msg</p>
            }

            <a href="/games/@game.Id">Detaljer</a>
        </div>
    }
</div>
```

Og fikk tilhørende css av KI:
```
.game-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 1rem;
}

.game-cell {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
}
```

Fikk også css av KI for game card:
```C#
<div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 1rem;">
    @foreach (var game in filteredGames)
    {
        <GameCard BoardGame="game" @key="game.Id"
                  IsSelected="GameNight.Contains(game.Id)"
                  OnAdd="HandleAdd" OnRemove="HandleRemove" />
    }
</div>
```

Prompt: Skjønner ikke. Vis spillhylla i ca. 375 px og minst 1200 px bredde, og legg skjermbilder i README.md. Når jeg prøver med de faste verdiene så endres det jo når jeg zoomer
Svar: 
Åpne spillhylla i Chrome eller Edge og trykk F12.
Trykk Ctrl+Shift+M for å slå på device toolbar.
Velg Responsive øverst og skriv inn bredden 375. Høyden kan for eksempel være 800.
Ta skjermbilde: klikk på de tre prikkene i device toolbar og velg Capture screenshot. Vil du ha hele siden, velger du Capture full size screenshot.
Gjenta med bredden 1280 (eller hvilken som helst bredde fra 1200 og oppover).

Begynte med en enkel variant for start og slutt av spill. Fikk forklart en del rundt Stopwatch. Men med alle funksjonene jeg ønsket meg ble det til slutt en del kode.
Jeg ønsket en knapp for å starte spill og en for å avslutte spill. Videre ønsket jeg en teller som fortsatte å telle om bruker forlater siden.
For AI dokumentasjon av dette se: Tidtaking_spillkveld

### Teoridel tilstander
Tilstand betyr i denne sammenheng alt applikasjonen husker når den kjører. For eksempel spill som er lagt til spillkveld og hvilken fane som er åpen.
Med lokal tilstand menes det som hører til en komponent. Et eksempel på lokal tilstand i min applikasjon er filteret i Games. Om jeg bytter fane og kommer tilbake til Games siden er filteret resettet.
Et eksempel på visning av applikasjonstilstand er GameBadge. Denne overlever navigasjon mellom sidene uten at tallet blir resettet. Men det er GameNightState som har applikasjonstilstanden og dataen.  

Parametere og EventCallback funker ypperlig i barn foreldre forhold. Barnet definerer Parametre mens foreldre gir verdier ned til barnet som rapporterer tilbake med EventCallback. Foreldrene styrer hva som skjer.
Et eksempel på det i egen applikasjon er GameNightButton knappen min som har EventCallback parametre OnAdd og OnRemove. Data sendes ned som verdier fra foreldrene Games og GamesDetails som har hver sin implementasjon av hva som skal skje. Knappen melder fra oppover når den trykkes.

Kaskaderende verdier sender verdier nedover istedenfor oppover med EventCallback. Verdiene blir tilgjengelige for alle ledd under forelderen i motsetning til EventCallback som kun sender oppover et ledd.
Jeg har brukt det noe i forbindelse med tidligere prosjekt for å sende informasjon om hvem som er innlogget:
```C#
AddCascadingAuthenticationState()
```

Tilstandstjenester registreres for klasser som skal holde på en tilstand i en gitt periode. Det er gunstig å bruke slike om flere komponenter skal bruke samme tjenesten. Slike klasser kan være injected (dependency injection) for at klasser eller sider kan bruke dem.
Et eksempel fra egen applikasjon er GamesStorageAndQuery. Levetiden er satt til Singleton som vil si at tilstanden eksisterer så lenge programmet kjører. Spillene kommer til å være delt for alle brukere av appen.
Mens selve spillkvelden er unik for hver bruker. Så to brukere vil ikke dele samme spillkveld med mindre dem bruker samme maskin.

Et annet og kanskje bedre eksempel på tilstandstjeneste er GameNightState som bare skal overleve spillkvelden. Denne har fått scoped som tilstand som gjør at hver spiller får en unik spillkveld.
Det er derimot verdt å vite at tilstanden er tapt om bruker refresher siden. Syntes det ble litt for dumt og spurte derfor Claude om hjelp. Etter litt drøftinger ble man klokere på hvordan man kunne gå frem.

Prompt: Ønsker at du lager en PDF som viser ombyggingen til ProtectedLocalStorage. Videre må jeg ha en knapp som resetter spillkvelden. Kjørte en ny prompt der jeg fikk lagt til farger for endringene.
Svar: Se i vedleggsmappen etter Spillekvled protectedlocalstorage 

Ny prompt, lastet opp prosjektet mitt til Claude:
Nå funker ikke den bekreftelsen om at spillkveld er lagt til så bra. Noen ganger henger den og kommer etterhvert som man klikker på andre spill mens noen ganger kommer den ikke i det hele tatt.

Svar: Jeg fant årsaken. Det er en sideeffekt av at AddAsync ble asynkron.
Meldingen blir derfor aldri vist, med mindre noe annet får siden til å tegnes i løpet av de 3 sekundene, for eksempel at du klikker på et annet spill. Det er nøyaktig det du ser.

Løsningen
Be Blazor tegne på nytt med en gang meldingen er satt, med StateHasChanged():

Games.Razor
```C#
private async Task HandleAdd(BoardGame game)
{
    var text = await GameNight.AddAsync(game)
        ? "Spillet ble lagt til"
        : "Spillet er allerede lagt til";

    messages[game.Id] = text;
    StateHasChanged();               // ← vis meldingen nå

    await Task.Delay(3000);

    // Fjern bare hvis det fortsatt er denne meldingen
    if (messages.TryGetValue(game.Id, out var current) && current == text)
        messages.Remove(game.Id);
}
```

GamesDetails.razor
```C#
private async Task HandleAdd(BoardGame g)
{
    message = await GameNight.AddAsync(g) ? "Spillet ble lagt til" : "Spillet er allerede lagt til";
    StateHasChanged();               // ← vis meldingen nå

    await Task.Delay(3000);
    message = null;
}
```

Etter endringen med å legge til nettleser lagring kan jeg ha flere faner opp og endringene i en fane blir oppdatert i de andre når siden refreshes.



## Oppgave 5
### Manuell testlogg 
#### For oppgave 2
Testtilfelle: Åpne /games/ID
Steg: Bruker en ID som ikke finnes, for eksempel /games/9999
Forventet resultat: Man blir tatt til NotFound siden. Programmet krasjer ikke.
Faktisk resultat: Som forventet

Testtilfelle: Åpne /games/ID
Steg: Bruker en ID som er bokstaver, for eksempel /games/abc
Forventet resultat: Man blir tatt til NotFound siden. Programmet krasjer ikke.
Faktisk resultat: Som forventet

Adressen treffer ikke spillsiden siden at disse sidene tar kun ID med tall: 
```C#
@page "/games/{Id:int}"
```

Testtilfelle: Klikk terningkast 1 og 6 på spillsiden <br>
Steg: Kontroller at forelderen får riktig verdi <br>
Forventet resultat: Forelderen får riktig verdig <br>
Faktisk resultat: Som forventet

Testtilfelle: Kontroller DiceRating <br>
Steg: ReadOnly skal ikke reagerer på klikk <br>
Forventet resultat: Ingen melding vises <br>
Faktisk resultat: Som forventet 

Testtilfelle: Kontroll av DiceRating verdi <br> 
Steg: Send en verdi utenfor 1–6 <br>
Forventet resultat: Feilmelding vises <br>
Faktisk resultat: Som forventet

Testtilfelle: Kontroll meny <br>
Steg: Markeringen i menyen følger side <br> 
Forventet resultat: Stemmer med siden man er på <br>
Faktisk resultat: Som forventet

#### For oppgave 3
Testtilfelle: Legg til et spill fra spillhylla <br>
Steg: kontroller at tallet i menyen øker <br>
Forventet resultat: GameBadge øker <br>
Faktisk resultat: Som forventet

Testtilfelle: Gå til en spillside og tilbake <br>
Steg:  Kontroller at spillkvelden er uendret <br>
Forventet resultat: Spillkvelden har ikke mistet sin tilstand <br>
Faktisk resultat: Som forventet

Testtilfelle: Legg til spill <br>
Steg: Forsøk å legge til samme spill to ganger <br> 
Forventet resultat: Man har kun tilgang til å fjerne (ikke legge til) spill som allerede er lagt til <br>
Faktisk resultat: Som forventet

Testtilfelle: Fjern det siste spille <br>
Steg: Kontroller at siden viser en tom-tilstand <br>
Forventet resultat: Samlet spilletid blir 0 <br>
Faktisk resultat: Som forventet

Testtilfelle: Samlet spilletid <br>
Steg: Kontroller samlet spilletid med minst tre spill <br>
Forventet resultat: Alle spill teller oppover. Total spilletid teller også live <br>
Faktisk resultat: Som forventet

Testtilfelle: Dokumenter om spillekvelden deles mellom faner <br>
Steg: Åpne appen i to faner <br>
Forventet resultat: Scoped GameNightState gir hver fane sin egen kopi av spillekvelden. Spillekvelden lagres i localStorage, som deles mellom faner, slik at alle faner viser samme spillekveld. Fikk hjelp av AI for formulering. <br>
Faktisk resultat: Som forventet. Endringer i en fane må refreshes 


Testtilfelle: 
Steg: 
Forventet resultat: 
Faktisk resultat 



## Generell KI bruk
Har bedt KI om å utdype oppgavebeskrivelse uten å skrive kode. 
Har brukt KI en del ved å lime inn feilmeldinger og få forklart hva som er feil. 
Har fått KI til å lage noen emojier.
Har sett og hentet inspirasjon for et oppsett jeg fikk fra Claude i sommer. Se vedleggsmappen etter fil med navn: fra-blazor-server-til-interactive-auto-ren.md.
Brukt enkelte ganger for å forenkle egenskrevet kode. 
Litt CSS spørsmål.
Litt spørsmål rundt teoridelen av oppgaven for å sikre at man har forstått konseptene skikkelig og får formulert ting faglig riktig.

## KI refleksjoner
Videreutvikling av forslag man har fått fra KI har vært viktig. Det er jeg som har styrt designet av løsningen. Noen eksempler på ting som ble endret etter KI svar:
* Ba om venstrejustert meny. Fikk en løsning der menyen var plassert i toppen. Videre var teksten grå og lite synlig. Ny prompt løste det.
 
# Andre kilder
Har fått god hjelp av et Blazor kurs på nettside Dometrain. Søkefunksjonen etter spill er bygd med kode presentert i dette kurset.
Ellers har undervsningen til Ali vært uvurdeelig. Kjempeopplegg. Har lært mye. Mye kode følger akkurat samme mønster som er vist i undervisnigen.
Det har vært veldig fint å kunne gå tilbake for å se på opptak fra tidligere samlinger med gode kodeeksempler.
Fin video om xUnit testing: https://www.youtube.com/watch?v=VXSqNCso3fA
https://www.tutorialspoint.com/article/how-to-select-a-random-element-from-a-chash-list

## Bilder
Testtilfelle: Spillhylle i forskjellige breder.
Steg: Tok skjermbilder av forskjellige skjermbredde
Forventet resultat: Spillhylla kan brukes på både smal skjerm (mobil) og bred skjerm.

### Faktisk resultat 375 px: 
![Spillhylla på 375 px](Bilder/375px.png)

### Faktisk resultat 1200 px:
![Spillhylla på 1200 px](Bilder/1200px.png)