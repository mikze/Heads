import { FormEvent, useEffect, useState } from 'react';
import {
    Alert, Box, Button, Chip, Divider, List, ListItem, ListItemText, MenuItem, Paper, Stack, TextField, Typography,
} from '@mui/material';
import { createRoom, joinRoom, roomFromUrl } from '../net/connection';
import { useAppSelector } from '../store/hooks';
import { RoomInfo } from '../store/store';
import { allGames, gameById } from '../games';
import ConnectionBadge from './ConnectionBadge';

const NICK_KEY = 'glowki.nick';

function roomBlocked(room: RoomInfo): string | null {
    if (!gameById(room.game)) return 'Gra niedostępna';
    if (room.inMatch) return 'Trwa mecz';
    if (room.players >= room.maxPlayers) return 'Pełny';
    return null;
}

export default function Menu({ title, subtitle }: { title: string; subtitle?: string }) {
    const serverGames = useAppSelector((s) => s.rooms.games);
    const notice = useAppSelector((s) => s.session.notice);
    const status = useAppSelector((s) => s.session.status);
    const rooms = useAppSelector((s) => s.rooms.list);
    const [nick, setNick] = useState('');
    const [roomName, setRoomName] = useState('');
    const [gameId, setGameId] = useState('');
    const [invite, setInvite] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [busy, setBusy] = useState(false);

    // Zapamiętany nick to tylko wygoda — bez localStorage też działa.
    useEffect(() => {
        try { setNick(localStorage.getItem(NICK_KEY) ?? ''); } catch { /* brak dostępu */ }
        setInvite(roomFromUrl());
    }, []);

    const connected = status === 'connected';
    const hasNick = nick.trim().length > 0;
    const invited = invite ? rooms.find((r) => r.id === invite) : undefined;
    // Gry, które ma serwer i które ta strona umie wyświetlić.
    const games = allGames().filter((g) => serverGames.some((sg) => sg.id === g.id));
    const chosenGame = games.find((g) => g.id === gameId) ?? games[0];
    const manyGames = games.length > 1;

    async function run(action: () => Promise<string | null>) {
        setBusy(true);
        setError(null);
        try { localStorage.setItem(NICK_KEY, nick.trim()); } catch { /* brak dostępu */ }
        const err = await action();
        setBusy(false);
        if (err) setError(err);
    }

    function onCreate(e: FormEvent) {
        e.preventDefault();
        if (chosenGame) void run(() => createRoom(nick, roomName, chosenGame.id));
    }

    return (
        <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center', p: 2 }}>
            <Paper sx={{ p: { xs: 3, sm: 4 }, width: '100%', maxWidth: 520 }}>
                <Stack spacing={3}>
                    <Box>
                        <Typography variant="h3" component="h1" sx={{ fontWeight: 800 }}>{title}</Typography>
                        {subtitle && <Typography color="text.secondary">{subtitle}</Typography>}
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

                    {invite && connected && (
                        invited ? (
                            <Alert
                                severity="success"
                                action={(
                                    <Button
                                        color="inherit"
                                        disabled={busy || !hasNick || roomBlocked(invited) !== null}
                                        onClick={() => void run(() => joinRoom(nick, invited.id))}
                                    >
                                        Dołącz
                                    </Button>
                                )}
                            >
                                Zaproszenie do pokoju „{invited.name}”
                                {roomBlocked(invited) && ` (${roomBlocked(invited)!.toLowerCase()})`}
                            </Alert>
                        ) : (
                            <Alert severity="warning">Pokój z zaproszenia już nie istnieje. Utwórz nowy albo wybierz inny.</Alert>
                        )
                    )}

                    <Stack component="form" onSubmit={onCreate} direction={{ xs: 'column', sm: 'row' }} spacing={1}>
                        {manyGames && (
                            <TextField
                                select
                                label="Gra"
                                size="small"
                                value={chosenGame?.id ?? ''}
                                onChange={(e) => setGameId(e.target.value)}
                                sx={{ minWidth: 140 }}
                            >
                                {games.map((g) => <MenuItem key={g.id} value={g.id}>{g.name}</MenuItem>)}
                            </TextField>
                        )}
                        <TextField
                            label="Nazwa pokoju (opcjonalnie)"
                            placeholder={hasNick ? `Pokój ${nick.trim()}` : undefined}
                            value={roomName}
                            onChange={(e) => setRoomName(e.target.value)}
                            size="small"
                            sx={{ flex: 1 }}
                            slotProps={{ htmlInput: { maxLength: 24 } }}
                        />
                        <Button type="submit" variant="contained" disabled={busy || !hasNick || !connected || !chosenGame}>
                            Utwórz pokój
                        </Button>
                    </Stack>

                    <Divider />

                    <Box>
                        <Typography variant="h6" component="h2" sx={{ mb: 1 }}>Otwarte pokoje</Typography>
                        {rooms.length === 0 ? (
                            <Typography color="text.secondary">
                                {connected ? 'Nie ma jeszcze żadnego pokoju — utwórz pierwszy.' : 'Łączenie z serwerem…'}
                            </Typography>
                        ) : (
                            <List disablePadding>
                                {rooms.map((room) => {
                                    const blocked = roomBlocked(room);
                                    return (
                                        <ListItem
                                            key={room.id}
                                            disableGutters
                                            secondaryAction={(
                                                <Button
                                                    size="small"
                                                    variant="outlined"
                                                    disabled={busy || !hasNick || blocked !== null}
                                                    onClick={() => void run(() => joinRoom(nick, room.id))}
                                                >
                                                    Dołącz
                                                </Button>
                                            )}
                                            sx={{ pr: 12 }}
                                        >
                                            <ListItemText
                                                primary={room.name}
                                                secondary={(
                                                    <Stack component="span" direction="row" spacing={1} sx={{ alignItems: 'center', mt: 0.5 }}>
                                                        {manyGames && <span>{gameById(room.game)?.name ?? room.game} ·</span>}
                                                        <span>{room.players}/{room.maxPlayers}</span>
                                                        {room.host && <span>· gospodarz {room.host}</span>}
                                                        {blocked && <Chip component="span" size="small" label={blocked} />}
                                                    </Stack>
                                                )}
                                                slotProps={{ primary: { sx: { overflowWrap: 'anywhere' } } }}
                                            />
                                        </ListItem>
                                    );
                                })}
                            </List>
                        )}
                    </Box>

                    <ConnectionBadge />
                </Stack>
            </Paper>
        </Box>
    );
}
