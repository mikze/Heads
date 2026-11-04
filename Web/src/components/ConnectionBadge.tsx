import { Box, Typography } from '@mui/material';
import { useAppSelector } from '@/store/hooks';

const LABELS = {
    connected: { text: 'Połączono z serwerem', color: 'success.main' },
    connecting: { text: 'Łączenie z serwerem…', color: 'warning.main' },
    disconnected: { text: 'Brak połączenia z serwerem', color: 'error.main' },
} as const;

export default function ConnectionBadge() {
    const status = useAppSelector((s) => s.session.status);
    const { text, color } = LABELS[status];
    return (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Box sx={{ width: 8, height: 8, borderRadius: '50%', bgcolor: color }} />
            <Typography variant="body2" color="text.secondary">{text}</Typography>
        </Box>
    );
}
