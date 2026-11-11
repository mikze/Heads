import { Alert, Box, Button, Chip, IconButton, Paper, Stack, Typography } from '@mui/material';
import { useState } from 'react';
import { inviteLink, leaveRoom, setReady, setSkin, startMatch } from '../net/connection';
import { LobbyPlayer } from '../store/store';
import { useAppSelector } from '../store/hooks';
import { gameById, GameClient } from '../games';
import Chat from './Chat';

function PlayerCard({ player, isMe, label, game }: {
    player?: LobbyPlayer; isMe: boolean; label?: string; game?: GameClient;
}) {
    if (!player) {
        return (
            <Paper variant="outlined" sx={{ p: 3, flex: 1, minHeight: 220, display: 'grid', placeItems: 'center' }}>
                <Typography color="text.secondary">Czekamy na gracza…</Typography>
            </Paper>
        );
    }

    const skins = game?.skins ?? [];
    return (
        <Paper variant="outlined" sx={{ p: 3, flex: 1, borderColor: isMe ? 'primary.main' : undefined }}>
            <Stack spacing={2} sx={{ alignItems: 'center' }}>
                <Stack direction="row" spacing={1} sx={{ alignItems: 'center', flexWrap: 'wrap', justifyContent: 'center' }}>
                    <Typography variant="h6" sx={{ overflowWrap: 'anywhere' }}>{player.nick}</Typography>
                    {isMe && <Chip size="small" label="Ty" color="primary" />}
                    {player.isHost && <Chip size="small" label="Gospodarz" variant="outlined" />}
                </Stack>
                {label && <Typography variant="body2" color="text.secondary">{label}</Typography>}
                {skins.length > 0 && (
                    <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                        {isMe && skins.length > 1 && (
                            <IconButton aria-label="Poprzedni wygląd" onClick={() => void setSkin((player.skin + skins.length - 1) % skins.length)}>◀</IconButton>
                        )}
                        <Box
                            component="img"
                            src={skins[player.skin] ?? skins[0]}
                            alt={`Wygląd ${player.skin + 1}`}
                            sx={{ width: 96, height: 96, objectFit: 'contain' }}
                        />
                        {isMe && skins.length > 1 && (
                            <IconButton aria-label="Następny wygląd" onClick={() => void setSkin((player.skin + 1) % skins.length)}>▶</IconButton>
                        )}
                    </Stack>
                )}
                {isMe ? (
                    <Button
                        variant={player.ready ? 'contained' : 'outlined'}
                        color={player.ready ? 'success' : 'primary'}
                        onClick={() => void setReady(!player.ready)}
                    >
                        {player.ready ? 'Gotowy ✓' : 'Zgłoś gotowość'}
                    </Button>
                ) : (
                    <Chip label={player.ready ? 'Gotowy' : 'Niegotowy'} color={player.ready ? 'success' : 'default'} />
                )}
            </Stack>
        </Paper>
    );
}

export default function Lobby() {
    const { id: roomId, name: roomName, game: gameId, players, inMatch, minPlayers, maxPlayers } = useAppSelector((s) => s.lobby);
    const [copied, setCopied] = useState(false);
    const myId = useAppSelector((s) => s.session.myId);
    const notice = useAppSelector((s) => s.session.notice);
    const game = gameById(gameId);

    const amHost = players.find((p) => p.isHost)?.id === myId;
    const allReady = players.length >= minPlayers && players.every((p) => p.ready);
    const canStart = amHost && allReady && !inMatch;
    const seats = game?.seatOrder ?? Array.from({ length: maxPlayers }, (_, i) => i);

    async function copyInvite() {
        try {
            await navigator.clipboard.writeText(inviteLink(roomId));
            setCopied(true);
            setTimeout(() => setCopied(false), 2000);
        } catch {
            // Schowek bywa niedostępny (np. strona bez HTTPS) — wtedy pokazujemy link do ręcznego skopiowania.
            window.prompt('Skopiuj link zaproszenia:', inviteLink(roomId));
        }
    }

    return (
        <Box sx={{ maxWidth: 1000, mx: 'auto', p: { xs: 2, md: 4 } }}>
            <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between', mb: 3, gap: 2, flexWrap: 'wrap' }}>
                <Box sx={{ minWidth: 0 }}>
                    <Typography variant="h4" component="h1" sx={{ fontWeight: 800, overflowWrap: 'anywhere' }}>{roomName}</Typography>
                    <Typography variant="body2" color="text.secondary">
                        {game?.name ?? gameId} · pokój {roomId}
                    </Typography>
                </Box>
                <Stack direction="row" spacing={1}>
                    <Button variant="outlined" onClick={() => void copyInvite()}>
                        {copied ? 'Skopiowano ✓' : 'Kopiuj link zaproszenia'}
                    </Button>
                    <Button color="inherit" onClick={() => void leaveRoom()}>Wyjdź do menu</Button>
                </Stack>
            </Stack>

            {notice && <Alert severity="info" sx={{ mb: 2 }}>{notice}</Alert>}
            {!game && <Alert severity="warning" sx={{ mb: 2 }}>Ta strona nie obsługuje gry „{gameId}”.</Alert>}

            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} sx={{ mb: 2 }}>
                {seats.map((seat) => {
                    const player = players.find((p) => p.seat === seat);
                    return (
                        <PlayerCard
                            key={seat}
                            player={player}
                            isMe={player?.id === myId}
                            label={game?.seatLabels?.[seat]}
                            game={game}
                        />
                    );
                })}
            </Stack>

            <Paper sx={{ p: 2, mb: 2, display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 2, flexWrap: 'wrap' }}>
                <Typography color="text.secondary">
                    {amHost
                        ? canStart ? 'Wszyscy są gotowi — możesz zaczynać.' : 'Mecz wystartuje, gdy wszyscy gracze będą gotowi.'
                        : 'Mecz rozpoczyna gospodarz, gdy wszyscy są gotowi.'}
                </Typography>
                {amHost && (
                    <Button variant="contained" size="large" disabled={!canStart} onClick={() => void startMatch()}>
                        Rozpocznij mecz
                    </Button>
                )}
            </Paper>

            <Chat />
        </Box>
    );
}
