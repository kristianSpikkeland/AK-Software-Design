# For oppgave 1
Noen av disse testene er også implemetert som automatiske tester.

Testtilfelle: Søk etter et spill som ﬁnnes <br>
Steg: Bruk små og store bokstaver <br>
Forventet resultat: Det funker med begge deler <br>
Faktisk resultat: Som forventet 

Testtilfelle: Søk etter noe som ikke ﬁnnes <br>
Steg: Sjekk resultat <br>
Forventet resultat: Spiller gis beskjed om at det var ingen treff <br>
Faktisk resultat: Som forventet 

Testtilfelle: Test grenseverdiene i spillerﬁlteret <br>
Steg: Sjekk med flere verdier om filteret stemmer <br>
Forventet resultat: Filteret filtrer riktig <br>
Faktisk resultat: Som forventet 

Testtilfelle: Kombiner søk og ﬁlte <br>
Steg: Kontroller at antall treﬀ stemmer <br>
Forventet resultat: Antall treff stemmer <br>
Faktisk resultat: Som forventet 

Testtilfelle: Tøm søkefelt <br>
Steg: Kontroller at hele samling vises <br>
Forventet resultat: Hele samling vises <br>
Faktisk resultat: Som forventet 

# For oppgave 2
Testtilfelle: Åpne /games/ID <br>
Steg: Bruker en ID som ikke finnes, for eksempel /games/9999 <br>
Forventet resultat: Man blir tatt til NotFound siden. Programmet krasjer ikke <br>
Faktisk resultat: Som forventet <br>

Testtilfelle: Åpne /games/ID <br>
Steg: Bruker en ID som er bokstaver, for eksempel /games/abc <br>
Forventet resultat: Man blir tatt til NotFound siden. Programmet krasjer ikke <br>
Faktisk resultat: Som forventet <br>

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

# For oppgave 3
Testtilfelle: Legg til et spill fra spillhylla <br>
Steg: Kontroller at tallet i menyen øker <br>
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

Se slutten av dokumentet for skjermbilder av spillhylla i forskjellige bredder.

# For oppgave 4

Testtilfelle: Stopp API-et <br>
Steg: Åpne spillhylle <br>
Forventet resultat: Appen viser feilmelding og krasjer ikke <br>
Faktisk resultat: Viser først Laster...  For deretter å bytte til meldingen: Kunne ikke hente spillene. Prøv igjen senere 

Testtilfelle: Opprettelse spill <br>
Steg: Opprett spill med tomt navn <br>
Forventet resultat: Feilmelding vises <br>
Faktisk resultat: Som foreventet. "Navn må fylles ut" vises med rød skrift 

Testtilfelle: Opprettelse spill <br>
Steg: Opprett spill der MaxPlayers lavere enn MinPlayers <br>
Forventet resultat: Feilmelding vises <br>
Faktisk resultat. Som forventet. "Maksimum spillere kan ikke være lavere enn minimum" vises med rød skrift.

Testtilfelle: Send ugyldige data rett fra API <br>
Steg: Kontroller svaret <br>
Forventet resultat: 400 <br>
Faktisk resultat: Som forventet for alle 3 testtilfeller

Testtilfelle: Endre et spil <br>
Steg: Kontroller spillhylla og spillsiden <br>
Forventet resultat: Begge viser endringen <br>
Faktisk resultat: Som forventet

Testtilfelle: Neste spill <br>
Steg: Bruker klikker på neste spill knapp <br>
Forventet resultat: Id parameter for adressen øker med en og neste spill vises om det finnes en høyere id (og spillet ikke er slettet). Om det ikke finnes høyere Id vises NotFound siden. <br>
Faktisk resultat: Som forventet

Testtilfelle: Slett et spill som ligger i Min spillekveld <br>
Steg: Deﬁner og dokumenter hva som skal skje <br>
Forventet resultat: Spillet vises ikke lengre i spillkvelden <br>
Faktisk resultat: Ikke som jeg forventet. Spillet ligger der fortsatt. Men synes egentlig ikke det gjør noe.
Alle spillene for spillkvelden forsvinner i det jeg velger nullstill spillkveld. 
Om admin driver å sletter spill mens noen stakkars spillere spiller dette spillet så er det bare bra om det ikke blir fjernet.
I ettertid så gir oppførselen også mening da spillkvelden holder sin egen tilstand og har sin egen liste. 

Testtilfelle: Feil url spillside <br>
Steg: Åpne spillsiden for en ID som API-et svarer 404 på <br>
Forventet resultat: NotFound siden vises med melding "Spillet finnes ikke, eller adressen er feil". <br> 
Faktisk resultat: Som forventet. 