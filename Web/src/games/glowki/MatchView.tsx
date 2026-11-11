import dynamic from 'next/dynamic';
import { useSyncExternalStore } from 'react';
import { Box, Button, Stack, Typography } from '@mui/material';
import { leaveMatch } from '@/kit/net/connection';
import { matchFeed } from './matchFeed';

// Phaser korzysta z window, więc ładujemy go tylko w przeglądarce.
const PhaserGame = dynamic(() => import('./PhaserGame'), { ssr: false });

export default function MatchView() {
    const { left, right, score } = useSyncExternalStore(matchFeed.subscribe, matchFeed.getInfo, matchFeed.getInfo);

    return (
        <Box sx={{ height: '100vh', display: 'flex', flexDirection: 'column', bgcolor: '#000' }}>
            <Stack
                direction="row"
                sx={{ px: 2, py: 1, alignItems: 'center', justifyContent: 'space-between', gap: 2, flexWrap: 'wrap', bgcolor: 'background.paper' }}
            >
                <Typography sx={{ fontWeight: 700 }}>
                    {left?.nick} {score.left} : {score.right} {right?.nick}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                    ← → ruch · ↑ skok · spacja kopnięcie · Esc wyjście
                </Typography>
                <Button size="small" color="inherit" onClick={() => void leaveMatch()}>Zakończ mecz</Button>
            </Stack>
            <Box sx={{ flex: 1, minHeight: 0 }}>
                <PhaserGame />
            </Box>
        </Box>
    );
}
