# KI bruk
* Noe debugging av egenskrevet Linq spørring.
* En prompt: Beste måte for å generere stigende Id enkelt med konstruktør i C# klasse.
* Brukte IEnumerable<BoardGame> games = gamesQuery.GetAllGames() direkte. Fikk beskjed om at man må bruke OnInitialized metode.
* Fikk en lang feilmelding om at noe var galt første gang jeg kjørte programmet. Det viste seg at jeg hadde registrert en service
som var avhengig av en annen Service som ikke var registrert. Løsningen ble naturligvis å registrere denne servicen også.
*Fikk en ny feilmelding der det viste seg at jeg hadde glemt å bruke new og instansiere listen til brettspillene.