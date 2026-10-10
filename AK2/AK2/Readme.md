
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



## Generell KI bruk
Har bedt KI om å utdype oppgavebeskrivelse uten å skrive kode. For oppgave 1 viste det seg at jeg haddde laget fem parametre i GameCard istedenfor 
å bare gi et BoardGame objekt. Programmet krasjet når jeg klikket på spill linkene etter at jeg implementerte endringen uten at jeg skjønte hvorfor.
Spurte KI og fikk til svar at feilen lå hos GamesDetails som fortsatt brukte de gamle parameteren fra GameCard.


# Andre kilder
Har fått god hjelp av et Blazor kurs på nettside Dometrain. Søkefunksjonen etter spill er bygd med kode presentert i dette kurset. 
Fin video om xUnit testing: https://www.youtube.com/watch?v=VXSqNCso3fA
https://www.tutorialspoint.com/article/how-to-select-a-random-element-from-a-chash-list