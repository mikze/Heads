import { Alert, Box, Button, Chip, IconButton, Paper, Stack, Typography } from '@mui/material';
import { useState } from 'react';
import { inviteLink, leaveRoom, setReady, setSkin, startMatch } from '@/net/connection';
import { LobbyPlayer } from '@/store/store';
import { useAppSelector } from '@/store/hooks';
import { SKIN_TEXTURES } from '@/game/skins';
import Chat from './Chat';

function PlayerCard({ player, isMe, side }: { player?: LobbyPlayer; isMe: boolean; side: string }) {
    if (!player) {
        return (
            <Paper variant="outlined" sx={{ p: 3, flex: 1, minHeight: 220, display: 'grid', placeItems: 'center' }}>
                <Typography color="text.secondary">Czekamy na drugiego gracza…</Typography>
            </Paper>
        );
    }

    const skins = SKIN_TEXTURES.length;
    return (
        <Paper variant="outlined" sx={{ p: 3, flex: 1, borderColor: isMe ? 'primary.main' : undefined }}>
            <Stack spacing={2} sx={{ alignItems: 'center' }}>
                <Stack direction="row" spacing={1} sx={{ alignItems: 'center', flexWrap: 'wrap', justifyContent: 'center' }}>
                    <Typography variant="h6" sx={{ overflowWrap: 'anywhere' }}>{player.nick}</Typography>
                    {isMe && <Chip size="small" label="Ty" color="primary" />}
                    {player.isHost && <Chip size="small" label="Gospodarz" variant="outlined" />}
                </Stack>
                <Typography variant="body2" color="text.secondary">Gra {side}</Typography>
                <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                    {isMe && (
                        <IconButton aria-label="Poprzedni wygląd" onClick={() => void setSkin((player.skin + skins - 1) % skins)}>◀</IconButton>
                    )}
                    <Box
                        component="img"
                        src={`/assets/${SKIN_TEXTURES[player.skin] ?? SKIN_TEXTURES[0]}.png`}
                        alt={`Wygląd ${player.skin + 1}`}
                        sx={{ width: 96, height: 96, objectFit: 'contain' }}
                    />
                    {isMe && (
                        <IconButton aria-label="Następny wygląd" onClick={() => void setSkin((player.skin + 1) % skins)}>▶</IconButton>
                    )}
                </Stack>
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
    const { id: roomId, name: roomName, players, inMatch } = useAppSelector((s) => s.lobby);
    const [copied, setCopied] = useState(false);
    const myId = useAppSelector((s) => s.session.myId);
    const notice = useAppSelector((s) => s.session.notice);

    const host = players.find((p) => p.isHost);
    const guest = players.find((p) => !p.isHost);
    const amHost = host?.id === myId;
    const canStart = amHost && players.length === 2 && players.every((p) => p.ready) && !inMatch;

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
                    <Typography variant="body2" color="text.secondary">Pokój {roomId}</Typography>
                </Box>
                <Stack direction="row" spacing={1}>
                    <Button variant="outlined" onClick={() => void copyInvite()}>
                        {copied ? 'Skopiowano ✓' : 'Kopiuj link zaproszenia'}
                    </Button>
                    <Button color="inherit" onClick={() => void leaveRoom()}>Wyjdź do menu</Button>
                </Stack>
            </Stack>

            {notice && <Alert severity="info" sx={{ mb: 2 }}>{notice}</Alert>}

            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} sx={{ mb: 2 }}>
                <PlayerCard player={guest} isMe={guest?.id === myId} side="po lewej" />
                <PlayerCard player={host} isMe={amHost} side="po prawej" />
            </Stack>

            <Paper sx={{ p: 2, mb: 2, display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 2, flexWrap: 'wrap' }}>
                <Typography color="text.secondary">
                    {amHost
                        ? canStart ? 'Obaj gracze są gotowi — możesz zaczynać.' : 'Mecz wystartuje, gdy obaj gracze będą gotowi.'
                        : 'Mecz rozpoczyna gospodarz, gdy obaj gracze są gotowi.'}
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
