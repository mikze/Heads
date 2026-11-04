import { useEffect } from 'react';
import { Provider } from 'react-redux';
import { createTheme, CssBaseline, ThemeProvider } from '@mui/material';
import { store } from '@/store/store';
import { useAppSelector } from '@/store/hooks';
import { connect } from '@/net/connection';
import Menu from '@/components/Menu';
import Lobby from '@/components/Lobby';
import MatchView from '@/components/MatchView';

const theme = createTheme({
    palette: {
        mode: 'dark',
        primary: { main: '#4caf50' },
        background: { default: '#0f1a12', paper: '#17251a' },
    },
    shape: { borderRadius: 10 },
});

function Screens() {
    const view = useAppSelector((s) => s.session.view);

    useEffect(() => {
        connect().catch(() => { /* ponowna próba odbywa się w tle */ });
    }, []);

    if (view === 'lobby') return <Lobby />;
    if (view === 'match') return <MatchView />;
    return <Menu />;
}

export default function App() {
    return (
        <Provider store={store}>
            <ThemeProvider theme={theme}>
                <CssBaseline />
                <Screens />
            </ThemeProvider>
        </Provider>
    );
}
