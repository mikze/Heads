import { FormEvent, useEffect, useRef, useState } from 'react';
import { Box, Button, Paper, Stack, TextField, Typography } from '@mui/material';
import { sendChat } from '../net/connection';
import { useAppSelector } from '../store/hooks';

export default function Chat() {
    const messages = useAppSelector((s) => s.chat.messages);
    const [text, setText] = useState('');
    const list = useRef<HTMLDivElement>(null);

    useEffect(() => {
        list.current?.scrollTo({ top: list.current.scrollHeight });
    }, [messages]);

    function onSubmit(e: FormEvent) {
        e.preventDefault();
        if (text.trim().length === 0) return;
        void sendChat(text);
        setText('');
    }

    return (
        <Paper sx={{ p: 2, display: 'flex', flexDirection: 'column', gap: 1.5, minHeight: 320 }}>
            <Typography variant="h6">Czat</Typography>
            <Box ref={list} sx={{ flex: 1, overflowY: 'auto', maxHeight: 360, minHeight: 200 }}>
                {messages.length === 0 && (
                    <Typography variant="body2" color="text.secondary">Brak wiadomości</Typography>
                )}
                {messages.map((m) => (
                    <Typography
                        key={m.key}
                        variant="body2"
                        sx={{ py: 0.25, color: m.system ? 'text.secondary' : 'text.primary', fontStyle: m.system ? 'italic' : 'normal', overflowWrap: 'anywhere' }}
                    >
                        {m.system ? m.text : <><b>{m.nick}:</b> {m.text}</>}
                    </Typography>
                ))}
            </Box>
            <Stack component="form" direction="row" spacing={1} onSubmit={onSubmit}>
                <TextField
                    size="small"
                    fullWidth
                    placeholder="Napisz wiadomość"
                    value={text}
                    onChange={(e) => setText(e.target.value)}
                    slotProps={{ htmlInput: { maxLength: 200 } }}
                />
                <Button type="submit" variant="outlined">Wyślij</Button>
            </Stack>
        </Paper>
    );
}
