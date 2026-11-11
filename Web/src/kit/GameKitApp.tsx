import { useEffect } from 'react';
import { Provider } from 'react-redux';
import { createTheme, CssBaseline, Theme, ThemeProvider } from '@mui/material';
import { store } from './store/store';
import { useAppSelector } from './store/hooks';
import { connect } from './net/connection';
import { gameById, GameClient, registerGames } from './games';
import Menu from './components/Menu';
import Lobby from './components/Lobby';

const defaultTheme = createTheme({
    palette: {
        mode: 'dark',
        primary: { main: '#4caf50' },
        background: { default: '#0f1a12', paper: '#17251a' },
    },
    shape: { borderRadius: 10 },
});

interface Props {
    title: string;
    subtitle?: string;
    /** Gry obsługiwane przez tę stronę. Muszą też być zarejestrowane na serwerze. */
    games: GameClient[];
    theme?: Theme;
}

function Screens({ title, subtitle }: Pick<Props, 'title' | 'subtitle'>) {
    const view = useAppSelector((s) => s.session.view);
    const gameId = useAppSelector((s) => s.lobby.game);

    useEffect(() => {
        connect().catch(() => { /* ponowna próba odbywa się w tle */ });
    }, []);

    const MatchView = gameById(gameId)?.MatchView;
    if (view === 'match' && MatchView) return <MatchView />;
    if (view === 'lobby' || view === 'match') return <Lobby />;
    return <Menu title={title} subtitle={subtitle} />;
}

/** Cała aplikacja: menu z pokojami, lobby z czatem i ekran meczu wybranej gry. */
export default function GameKitApp({ title, subtitle, games, theme = defaultTheme }: Props) {
    // Gry muszą być znane przed pierwszym połączeniem — wtedy podpinamy ich zdarzenia.
    registerGames(games);
    return (
        <Provider store={store}>
            <ThemeProvider theme={theme}>
                <CssBaseline />
                <Screens title={title} subtitle={subtitle} />
            </ThemeProvider>
        </Provider>
    );
}
