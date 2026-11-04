import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import {
    addMessage, backToMenu, clearChat, joined, LobbyState, matchEnded, matchStarted,
    MatchPlayer, Score, setLobby, setMatch, setNotice, setScore, setStatus, store,
} from '@/store/store';
import { EntityInfo, matchFeed } from './matchFeed';

// Domyślnie serwer gry działa na porcie 5000 tego samego hosta, z którego wczytano stronę.
// Inny adres można podać przy budowaniu: NEXT_PUBLIC_SERVER_URL=http://host:port
function serverUrl() {
    const configured = process.env.NEXT_PUBLIC_SERVER_URL;
    const base = configured && configured.length > 0
        ? configured
        : `${window.location.protocol}//${window.location.hostname}:5000`;
    return `${base.replace(/\/$/, '')}/game`;
}

interface JoinResponse {
    ok: boolean;
    error?: string;
    id?: string;
    lobby?: LobbyState;
}

interface MatchDescription {
    entities: EntityInfo[];
    right: MatchPlayer;
    left: MatchPlayer;
    score: Score;
}

let connection: HubConnection | null = null;
let starting: Promise<void> | null = null;

function inRoom() {
    return store.getState().session.view !== 'menu';
}

// Handlery nie mogą zwracać wartości — SignalR uznałby ją za odpowiedź dla serwera.
function setUpHandlers(conn: HubConnection) {
    conn.on('Lobby', (lobby: LobbyState) => { store.dispatch(setLobby(lobby)); });
    conn.on('Chat', (msg: { nick: string; text: string; system: boolean }) => { store.dispatch(addMessage(msg)); });

    conn.on('MatchStarted', (match: MatchDescription) => {
        matchFeed.reset(match.entities);
        store.dispatch(setMatch({ right: match.right, left: match.left, score: match.score }));
        store.dispatch(matchStarted());
    });
    conn.on('MatchEnded', (reason: string) => { store.dispatch(matchEnded(reason)); });
    conn.on('Score', (score: Score) => { store.dispatch(setScore(score)); });
    conn.on('Frame', (frame: number[]) => { matchFeed.frame(frame); });
    conn.on('BonusSpawned', (bonus: EntityInfo) => { matchFeed.bonusSpawned(bonus); });
    conn.on('BonusRemoved', (id: number) => { matchFeed.bonusRemoved(id); });

    // Po zerwaniu połączenia serwer usuwa gracza z pokoju, więc wracamy do menu.
    conn.onreconnecting(() => {
        store.dispatch(setStatus('connecting'));
        if (inRoom()) store.dispatch(backToMenu('Połączenie z serwerem zostało zerwane'));
    });
    conn.onreconnected(() => store.dispatch(setStatus('connected')));
    conn.onclose(() => {
        store.dispatch(setStatus('disconnected'));
        if (inRoom()) store.dispatch(backToMenu('Połączenie z serwerem zostało zerwane'));
        connection = null;
        // Próbujemy połączyć się ponownie w tle.
        setTimeout(() => connect().catch(() => {}), 3000);
    });
}

export function connect(): Promise<void> {
    if (connection?.state === HubConnectionState.Connected) return Promise.resolve();
    if (starting) return starting;

    const conn = connection ?? new HubConnectionBuilder()
        .withUrl(serverUrl())
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Warning)
        .build();

    if (!connection) {
        connection = conn;
        setUpHandlers(conn);
    }

    store.dispatch(setStatus('connecting'));
    starting = conn.start()
        .then(() => { store.dispatch(setStatus('connected')); })
        .catch((e) => {
            console.warn('Nie udało się połączyć z serwerem', e);
            store.dispatch(setStatus('disconnected'));
            connection = null;
            setTimeout(() => connect().catch(() => {}), 3000);
            throw e;
        })
        .finally(() => { starting = null; });
    return starting;
}

function send(method: string, ...args: unknown[]) {
    if (connection?.state !== HubConnectionState.Connected) return Promise.resolve();
    return connection.send(method, ...args).catch((e) => console.warn(`Nie udało się wysłać ${method}`, e));
}

export async function joinRoom(nick: string): Promise<string | null> {
    try {
        await connect();
    } catch {
        return 'Brak połączenia z serwerem';
    }
    // Czyścimy czat przed dołączeniem, bo serwer od razu wysyła komunikat o wejściu do pokoju.
    store.dispatch(clearChat());
    const result = await connection!.invoke<JoinResponse>('Join', nick);
    if (!result.ok) return result.error ?? 'Nie udało się dołączyć';

    if (result.lobby) store.dispatch(setLobby(result.lobby));
    store.dispatch(joined(result.id!));
    return null;
}

export async function leaveRoom() {
    await send('LeaveRoom');
    store.dispatch(backToMenu(null));
}

export const setSkin = (skin: number) => send('SetSkin', skin);
export const setReady = (ready: boolean) => send('SetReady', ready);
export const sendChat = (text: string) => send('SendChat', text);
export const sendInput = (input: number) => send('SendInput', input);
export const leaveMatch = () => send('LeaveMatch');

export async function startMatch() {
    if (connection?.state !== HubConnectionState.Connected) return;
    const error = await connection.invoke<string | null>('StartMatch');
    store.dispatch(setNotice(error));
}
