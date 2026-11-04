# Heads (Główki)

Sieciowa gra dla dwóch graczy w stylu „piłki główkami”: dwie postaci z dużymi głowami kopią piłkę i próbują strzelić gola przeciwnikowi. Gra działa w przeglądarce.

## Jak to działa

- **`GlowkiServer/`**: serwer w ASP.NET (.NET 10). Liczy fizykę meczu w Box2D i rozmawia z przeglądarkami przez SignalR (`/game`). Po zbudowaniu frontu serwuje też jego pliki, więc wszystko jest dostępne pod jednym adresem.
- **`Web/`**: front w Next.js z Phaserem (scena meczu), Reduxem (stan aplikacji) i MUI (menu, lobby, czat).

Przeglądarka wysyła serwerowi tylko wciśnięte klawisze. Serwer liczy fizykę i około 100 razy na sekundę odsyła pozycje piłki, graczy i stóp.

Jest jeden pokój na dwóch graczy. Pierwszy gracz, który wejdzie, jest gospodarzem: gra po prawej i rozpoczyna mecz, gdy obaj gracze zgłoszą gotowość.

## Uruchomienie

Potrzebne są .NET SDK 10 i Node.js (z npm).

### Wersja do grania (jeden proces)

```bash
cd Web && npm install && npm run build   # buduje front do Web/out
cd ../GlowkiServer && dotnet run
```

Gra jest dostępna pod `http://<adres-serwera>:5000`.

### Tryb deweloperski (front z automatycznym przeładowaniem)

W dwóch terminalach:

```bash
cd GlowkiServer && dotnet run    # serwer gry na porcie 5000
cd Web && npm run dev            # front na http://localhost:8080
```

Front łączy się z serwerem na porcie 5000 tego samego hosta, z którego wczytano stronę. Inny adres można ustawić zmienną `NEXT_PUBLIC_SERVER_URL` (np. `NEXT_PUBLIC_SERVER_URL=http://192.168.1.10:5000 npm run build`).

## Sterowanie

| Klawisz | Akcja |
| --- | --- |
| ← → | ruch |
| ↑ | skok |
| spacja | kopnięcie |
| Esc | zakończenie meczu (obaj gracze wracają do lobby) |
