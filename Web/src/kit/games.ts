import type { ComponentType } from 'react';

/**
 * Część gry działająca w przeglądarce. Kit (menu, lobby, czat, połączenie) nie importuje gier —
 * dostaje je przez registerGames() i woła tylko to, co tu opisane.
 */
export interface GameClient {
    /** Ten sam identyfikator co IGameDefinition.Id na serwerze. */
    id: string;
    name: string;
    /** Jedno zdanie pod nazwą gry w menu. */
    tagline?: string;
    /** Adresy grafik wyglądów w kolejności numerów na serwerze. Pusta lista — brak wyboru wyglądu. */
    skins: string[];
    /** Podpis miejsca w lobby, np. „gra po prawej”. Indeks to miejsce (0 = gospodarz). */
    seatLabels?: string[];
    /** Kolejność kart graczy w lobby od lewej, np. [1, 0]. Domyślnie po kolei. */
    seatOrder?: number[];
    /** Zdarzenia meczu z serwera (nazwa → obsługa). Wywoływane tylko, gdy gracz jest w pokoju tej gry. */
    events: Record<string, (payload: any) => void>;
    /** Serwer wystartował mecz; description to wynik IGameSession.Describe(). */
    matchStarted(description: any): void;
    /** Ekran meczu. Zakończenie meczu obsługuje kit (powrót do lobby). */
    MatchView: ComponentType;
}

let registered: GameClient[] = [];

export function registerGames(games: GameClient[]) {
    registered = games;
}

export const allGames = () => registered;

export const gameById = (id: string | null | undefined) => registered.find((g) => g.id === id);
