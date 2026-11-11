import { configureStore, createSlice, PayloadAction } from '@reduxjs/toolkit';

export type View = 'menu' | 'lobby' | 'match';
export type ConnectionStatus = 'connecting' | 'connected' | 'disconnected';

export interface LobbyPlayer {
    id: string;
    nick: string;
    skin: number;
    ready: boolean;
    isHost: boolean;
    seat: number;
}

export interface LobbyState {
    id: string;
    name: string;
    game: string;
    minPlayers: number;
    maxPlayers: number;
    inMatch: boolean;
    players: LobbyPlayer[];
}

export interface RoomInfo {
    id: string;
    name: string;
    game: string;
    host: string | null;
    players: number;
    maxPlayers: number;
    inMatch: boolean;
}

export interface ChatMessage {
    key: number;
    nick: string;
    text: string;
    system: boolean;
}

/** Gra dostępna na serwerze (ListGames). */
export interface ServerGame {
    id: string;
    name: string;
    minPlayers: number;
    maxPlayers: number;
    skinCount: number;
}

const session = createSlice({
    name: 'session',
    initialState: {
        view: 'menu' as View,
        myId: null as string | null,
        status: 'connecting' as ConnectionStatus,
        // Komunikat pokazywany w menu/lobby, np. powód powrotu z meczu.
        notice: null as string | null,
    },
    reducers: {
        setStatus: (s, a: PayloadAction<ConnectionStatus>) => { s.status = a.payload; },
        joined: (s, a: PayloadAction<string>) => { s.myId = a.payload; s.view = 'lobby'; s.notice = null; },
        backToMenu: (s, a: PayloadAction<string | null>) => { s.myId = null; s.view = 'menu'; s.notice = a.payload; },
        matchStarted: (s) => { s.view = 'match'; s.notice = null; },
        matchEnded: (s, a: PayloadAction<string>) => { if (s.view === 'match') s.view = 'lobby'; s.notice = a.payload; },
        setNotice: (s, a: PayloadAction<string | null>) => { s.notice = a.payload; },
    },
});

const lobby = createSlice({
    name: 'lobby',
    initialState: { id: '', name: '', game: '', minPlayers: 0, maxPlayers: 0, inMatch: false, players: [] } as LobbyState,
    reducers: {
        setLobby: (_s, a: PayloadAction<LobbyState>) => a.payload,
    },
});

const rooms = createSlice({
    name: 'rooms',
    initialState: { list: [] as RoomInfo[], games: [] as ServerGame[] },
    reducers: {
        setRooms: (s, a: PayloadAction<RoomInfo[]>) => { s.list = a.payload; },
        setGames: (s, a: PayloadAction<ServerGame[]>) => { s.games = a.payload; },
    },
});

let chatKey = 0;
const chat = createSlice({
    name: 'chat',
    initialState: { messages: [] as ChatMessage[] },
    reducers: {
        addMessage: (s, a: PayloadAction<Omit<ChatMessage, 'key'>>) => {
            s.messages.push({ ...a.payload, key: ++chatKey });
            if (s.messages.length > 100) s.messages.shift();
        },
        clearChat: (s) => { s.messages = []; },
    },
});

export const { setStatus, joined, backToMenu, matchStarted, matchEnded, setNotice } = session.actions;
export const { setLobby } = lobby.actions;
export const { setRooms, setGames } = rooms.actions;
export const { addMessage, clearChat } = chat.actions;

export const store = configureStore({
    reducer: {
        session: session.reducer,
        lobby: lobby.reducer,
        rooms: rooms.reducer,
        chat: chat.reducer,
    },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
