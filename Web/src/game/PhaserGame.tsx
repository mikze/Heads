import { useEffect, useRef } from 'react';
import * as Phaser from 'phaser';
import { MatchScene } from './scenes/MatchScene';

/** Canvas Phasera z meczem. Gra powstaje przy montowaniu i jest niszczona przy wyjściu z meczu. */
export default function PhaserGame() {
    const container = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const game = new Phaser.Game({
            type: Phaser.AUTO,
            width: 1024,
            height: 800,
            parent: container.current!,
            backgroundColor: '#000000',
            audio: { noAudio: true },
            scale: {
                mode: Phaser.Scale.FIT,
                autoCenter: Phaser.Scale.CENTER_BOTH,
            },
            scene: [MatchScene],
        });
        return () => game.destroy(true);
    }, []);

    return <div ref={container} style={{ width: '100%', height: '100%' }} />;
}
