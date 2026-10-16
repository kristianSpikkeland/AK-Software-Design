
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

Prompt: Hvordan legger man en komponent oppe til høyre på en side?

Svar: Det vanligste er å bruke CSS. Hvilken metode du velger avhenger av om komponenten skal følge med når man scroller.

```CSS
.gameNight-badge {
    position: fixed;
    top: 16px;
    right: 16px;
    z-index: 1000; /* legger den over annet innhold */
}
```

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

Prompt: bruker xUnit og tenkte å lage en test som sjekker navigasjon mellom sider. Kan man få til det eller må man bruke verktøy som eks. Playwright?

Fikk beskjed om at det fantes noe som heter Bunit som vistnok er greit. 

Ny prompt: Lag en veldig god Bunit manual som viser alle de mest brukte metodene for å teste nettsider. Kom med konkrete og utfyllende eksempler. 
Ikke overkompliser, det skal være enkelt for en som aldri har brukt Bunit å komme igang. Ta utgangspunkt i et generelt tema og ikke oppgaveteksten min da jeg ønsker trening i å skrive det om til det oppgaven trenger.
Gå inn å les oppgaveteksten slik at vi er sikre på at alt blir dekt og at jeg slipper å kjøre nye prompter siden jeg står fast.

Svar se vedleggsmappen.

## Oppgave 5
### Manuell testlogg 
#### Oppgave 2
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

Testtilfelle: Klikk terningkast 1 og 6 på spillsiden
Steg: Kontroller at forelderen får riktig verdi
Forventet resultat: Forelderen får riktig verdig
Faktisk resultat: Som forventet

Testtilfelle: Kontroller DiceRating
Steg: ReadOnly skal ikke reagerer på klikk
Forventet resultat: Ingen melding vises  
Faktisk resultat: Som forventet

Testtilfelle: Kontroll av DiceRating verdi 
Steg: Send en verdi utenfor 1–6
Forventet resultat: Feilmelding vises
Faktisk resultat: Som forventet

Testtilfelle: Kontroll meny
Steg: Markeringen i menyen følger side  
Forventet resultat: Stemmer med siden man er på
Faktisk resultat: Som forventet

Testtilfelle:
Steg: 
Forventet resultat:
Faktisk resultat:


## Generell KI bruk
Har bedt KI om å utdype oppgavebeskrivelse uten å skrive kode. 
Har brukt KI en del ved å lime inn feilmeldinger og få forklart hva som er feil. 
Har fått KI til å lage noen emojier.
Har sett og hentet inspirasjon for et oppsett jeg fikk fra Claude i sommer. Se vedleggsmappen etter fil med navn: fra-blazor-server-til-interactive-auto-ren.md.

## KI refleksjoner
Videreutvikling av forslag man har fått fra KI har vært viktig. Det er jeg som har styrt designet av løsningen. Noen eksempler på ting som ble endret etter KI svar:
* Ba om venstrejustert meny. Fikk en løsning der menyen var plassert i toppen. Ny prompt løste det.

# Andre kilder
Har fått god hjelp av et Blazor kurs på nettside Dometrain. Søkefunksjonen etter spill er bygd med kode presentert i dette kurset.
Ellers har undervsningen til Ali vært uvurdeelig. Kjempeopplegg. Har lært mye. Mye kode følger akkurat samme mønster som er vist i undervisnigen.
Det har vært veldig fint å kunne gå tilbake for å se på opptak fra tidligere samlinger med gode kodeeksempler.
Fin video om xUnit testing: https://www.youtube.com/watch?v=VXSqNCso3fA
https://www.tutorialspoint.com/article/how-to-select-a-random-element-from-a-chash-list