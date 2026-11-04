import { FormEvent, useEffect, useState } from 'react';
import { Alert, Box, Button, Paper, Stack, TextField, Typography } from '@mui/material';
import { joinRoom } from '@/net/connection';
import { useAppSelector } from '@/store/hooks';
import ConnectionBadge from './ConnectionBadge';

const NICK_KEY = 'glowki.nick';

export default function Menu() {
    const notice = useAppSelector((s) => s.session.notice);
    const status = useAppSelector((s) => s.session.status);
    const [nick, setNick] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [busy, setBusy] = useState(false);

    // Zapamiętany nick to tylko wygoda — bez localStorage też działa.
    useEffect(() => {
        try { setNick(localStorage.getItem(NICK_KEY) ?? ''); } catch { /* brak dostępu */ }
    }, []);

    async function onSubmit(e: FormEvent) {
        e.preventDefault();
        setBusy(true);
        setError(null);
        try { localStorage.setItem(NICK_KEY, nick.trim()); } catch { /* brak dostępu */ }
        const err = await joinRoom(nick);
        setBusy(false);
        if (err) setError(err);
    }

    return (
        <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center', p: 2 }}>
            <Paper component="form" onSubmit={onSubmit} sx={{ p: 4, width: '100%', maxWidth: 400 }}>
                <Stack spacing={3}>
                    <Box>
                        <Typography variant="h3" component="h1" sx={{ fontWeight: 800 }}>Główki</Typography>
                        <Typography color="text.secondary">Piłka nożna głowami dla dwóch graczy</Typography>
                    </Box>
                    {notice && <Alert severity="info">{notice}</Alert>}
                    {error && <Alert severity="error">{error}</Alert>}
                    <TextField
                        label="Nick"
                        value={nick}
                        onChange={(e) => setNick(e.target.value)}
                        autoFocus
                        slotProps={{ htmlInput: { maxLength: 16 } }}
                    />
                    <Button
                        type="submit"
                        variant="contained"
                        size="large"
                        disabled={busy || nick.trim().length === 0 || status !== 'connected'}
                    >
                        Dołącz do gry
                    </Button>
                    <ConnectionBadge />
                </Stack>
            </Paper>
        </Box>
    );
}
