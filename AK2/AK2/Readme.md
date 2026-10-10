
# Oppgave 1

## Info 
Siden games har ansvar for å vise alle spill mens GameCard er en gjenbrukbar komponent som blir implementert i GamesDetail siden.
GamesDetail viser detaljene for et spill. På games siden er det lagt til lenker som man kan klikke på for å komme til detaljene for et spill.

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


# Andre kilder
Har fått god hjelp av et Blazor kurs på nettside Dometrain. Søkefunksjonen etter spill er bygd med kode presentert i dette kurset. 

https://www.tutorialspoint.com/article/how-to-select-a-random-element-from-a-chash-list