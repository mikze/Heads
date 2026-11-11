# Heads (Główki)

Sieciowa gra dla dwóch graczy w stylu „piłki główkami”: dwie postaci z dużymi głowami kopią piłkę i próbują strzelić gola przeciwnikowi. Gra działa w przeglądarce.

## Jak to działa

Kod jest podzielony na wspólną podstawę (**GameKit**) i gry, które z niej korzystają. Jeden serwer może obsługiwać wiele gier.

- **`GameKit/`**: biblioteka .NET 10. Zawiera pokoje, lobby, czat, hub SignalR (`/game`) i pętlę meczu (`GameLoop`). Nic nie wie o konkretnej grze.
- **`Games/Glowki/`**: Główki. Zasady meczu i fizyka w Box2D. Implementuje `IGameDefinition` i `IGameSession` z GameKit.
- **`GameServer/`**: aplikacja ASP.NET. Rejestruje gry (`AddGame<GlowkiGame>()`) i serwuje zbudowany front, więc wszystko jest pod jednym adresem.
- **`Web/`**: front w Next.js z MUI i Reduxem.
  - `src/kit/`: menu, lobby, czat, połączenie. Nie importuje gier.
  - `src/games/<gra>/`: ekran meczu danej gry (Główki: Phaser), opisany przez `GameClient`.
  - `public/games/<gra>/`: grafiki danej gry.

Przeglądarka wysyła serwerowi tylko wciśnięte klawisze. Serwer liczy fizykę i około 100 razy na sekundę odsyła pozycje piłki, graczy i stóp.

Na serwerze może być wiele pokoi naraz; każdy pokój ma swoją grę (Główki: dwóch graczy). W menu widać listę otwartych pokoi; można też utworzyć własny i wysłać znajomemu link z zaproszeniem (`?room=<id>`). Twórca pokoju jest gospodarzem: gra po prawej i rozpoczyna mecz, gdy obaj gracze zgłoszą gotowość. Pusty pokój znika automatycznie.

## Uruchomienie

Potrzebne są .NET SDK 10 i Node.js (z npm).

### Wersja do grania (jeden proces)

```bash
cd Web && npm install && npm run build   # buduje front do Web/out
cd ../GameServer && dotnet run
```

Gra jest dostępna pod `http://<adres-serwera>:5000`.

### Tryb deweloperski (front z automatycznym przeładowaniem)

W dwóch terminalach:

```bash
cd GameServer && dotnet run      # serwer gry na porcie 5000
cd Web && npm run dev            # front na http://localhost:8080
```

Front łączy się z serwerem na porcie 5000 tego samego hosta, z którego wczytano stronę. Inny adres można ustawić zmienną `NEXT_PUBLIC_SERVER_URL` (np. `NEXT_PUBLIC_SERVER_URL=http://192.168.1.10:5000 npm run build`).

## Dodawanie nowej gry

1. Serwer: nowy projekt w `Games/<Gra>/` z referencją do `GameKit`. Klasa z `IGameDefinition` (id, nazwa, liczba graczy, liczba wyglądów) tworzy mecz implementujący `IGameSession`. Mecz wysyła zdarzenia graczom przez `IMatchContext.Send`, a może się sam zakończyć przez `IMatchContext.End`.
2. W `GameServer`: referencja do projektu i `.AddGame<NowaGra>()` w `Program.cs`.
3. Front: folder `src/games/<gra>/` z obiektem `GameClient` (to samo `id`, obsługa zdarzeń meczu, komponent `MatchView`), dopisany do listy w `src/App.tsx`.

Gdy na serwerze jest więcej niż jedna gra, w menu pojawia się wybór gry przy tworzeniu pokoju.

## Sterowanie (Główki)

| Klawisz | Akcja |
| --- | --- |
| ← → | ruch |
| ↑ | skok |
| spacja | kopnięcie |
| Esc | zakończenie meczu (obaj gracze wracają do lobby) |
