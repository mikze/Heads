import GameKitApp from '@/kit/GameKitApp';
import glowki from '@/games/glowki';

// Nową grę dodaje się tutaj i w GameServer/Program.cs.
const games = [glowki];

export default function App() {
    return (
        <GameKitApp
            title="Główki"
            subtitle="Piłka nożna głowami dla dwóch graczy"
            games={games}
        />
    );
}
