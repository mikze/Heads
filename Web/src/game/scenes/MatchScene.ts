import * as Phaser from 'phaser';
import { EntityInfo, EntityKind, matchFeed, MatchListener } from '@/net/matchFeed';
import { leaveMatch, sendInput } from '@/net/connection';
import { store } from '@/store/store';
import { SKIN_TEXTURES } from '../skins';

// Te same flagi co Input na serwerze.
const Input = {
    none: 1,
    up: 2,
    right: 4,
    left: 8,
    kick: 16,
    kickUp: 32,
};

type Keys = Record<'up' | 'left' | 'right' | 'kick' | 'exit', Phaser.Input.Keyboard.Key>;

export class MatchScene extends Phaser.Scene implements MatchListener {
    private sprites = new Map<number, Phaser.GameObjects.Image>();
    private labels = new Map<number, Phaser.GameObjects.Text>();
    private bonusSprites = new Map<number, Phaser.GameObjects.Image>();
    private keys!: Keys;
    private lastInput = -1;
    private leaving = false;
    private scoreLeft!: Phaser.GameObjects.Text;
    private scoreRight!: Phaser.GameObjects.Text;
    private unsubscribe: (() => void) | null = null;

    constructor() {
        super('Match');
    }

    preload() {
        this.load.image('background2', '/assets/background2.png');
        this.load.image('ball', '/assets/ball.png');
        this.load.image('foot', '/assets/foot.png');
        this.load.image('rfoot', '/assets/rfoot.png');
        this.load.image('BonusSpeed', '/assets/BonusSpeed.png');
        SKIN_TEXTURES.forEach((t) => this.load.image(t, `/assets/${t}.png`));
    }

    create() {
        this.add.image(512, 400, 'background2');

        matchFeed.entities.forEach((e) => this.createEntity(e));
        matchFeed.bonuses.forEach((b) => this.onBonusSpawned(b));

        // Wynik w tych samych miejscach co w wersji desktopowej.
        const style = { fontFamily: 'Arial', fontSize: '48px', color: '#ffffff' };
        this.scoreLeft = this.add.text(50, 50, '0', style);
        this.scoreRight = this.add.text(720, 50, '0', style);
        this.showScore();
        this.unsubscribe = store.subscribe(() => this.showScore());

        const K = Phaser.Input.Keyboard.KeyCodes;
        this.keys = this.input.keyboard!.addKeys({
            up: K.UP, left: K.LEFT, right: K.RIGHT, kick: K.SPACE, exit: K.ESC,
        }) as Keys;

        matchFeed.attach(this);
        if (matchFeed.lastFrame) this.onFrame(matchFeed.lastFrame);

        this.events.once(Phaser.Scenes.Events.SHUTDOWN, () => this.cleanUp());
        this.events.once(Phaser.Scenes.Events.DESTROY, () => this.cleanUp());
    }

    update() {
        if (Phaser.Input.Keyboard.JustDown(this.keys.exit) && !this.leaving) {
            this.leaving = true;
            void leaveMatch();
            return;
        }

        let input = Input.none;
        if (this.keys.right.isDown) input |= Input.right;
        if (this.keys.left.isDown) input |= Input.left;
        if (this.keys.up.isDown) input |= Input.up;
        input |= this.keys.kick.isDown ? Input.kick : Input.kickUp;

        // Serwer pamięta ostatnie sterowanie, więc wysyłamy tylko zmiany.
        if (input !== this.lastInput) {
            this.lastInput = input;
            void sendInput(input);
        }
    }

    onFrame(frame: number[]) {
        for (let i = 0; i + 3 < frame.length; i += 4) {
            const sprite = this.sprites.get(frame[i]);
            if (!sprite) continue;
            sprite.setPosition(frame[i + 1], frame[i + 2]);
            // Ten sam znak kąta co w kliencie MonoGame (grafiki stóp są pod to narysowane).
            sprite.setRotation(-frame[i + 3]);
            this.labels.get(frame[i])?.setPosition(frame[i + 1], frame[i + 2] - 48);
        }
    }

    onBonusSpawned(bonus: EntityInfo) {
        if (this.bonusSprites.has(bonus.id)) return;
        this.bonusSprites.set(bonus.id, this.add.image(bonus.positionX, bonus.positionY, 'BonusSpeed'));
    }

    onBonusRemoved(id: number) {
        this.bonusSprites.get(id)?.destroy();
        this.bonusSprites.delete(id);
    }

    private createEntity(e: EntityInfo) {
        if (e.kind === EntityKind.Ball) {
            this.sprites.set(e.id, this.circleSprite(e, 'ball'));
        } else if (e.kind === EntityKind.Player) {
            const { right, left } = store.getState().match;
            const player = e.params === 'PlayerRight' ? right : left;
            const texture = SKIN_TEXTURES[player?.skin ?? 0] ?? SKIN_TEXTURES[0];
            this.sprites.set(e.id, this.circleSprite(e, texture));
            if (player) {
                const label = this.add.text(e.positionX, e.positionY - 48, player.nick, {
                    fontFamily: 'Arial', fontSize: '16px', color: '#ffffff', stroke: '#000000', strokeThickness: 3,
                }).setOrigin(0.5, 1).setDepth(10);
                this.labels.set(e.id, label);
            }
        } else if (e.params === 'foot' || e.params === 'rfoot') {
            this.sprites.set(e.id, this.add.image(e.positionX, e.positionY, e.params));
        }
        // Ściany, podłoga i bramki nie mają grafiki — są narysowane na tle.
    }

    // Grafika skalowana tak, żeby jej wysokość odpowiadała średnicy koła z fizyki.
    private circleSprite(e: EntityInfo, texture: string) {
        const image = this.add.image(e.positionX, e.positionY, texture);
        image.setScale(e.sizeX / (image.height / 2));
        return image;
    }

    private showScore() {
        const { score } = store.getState().match;
        this.scoreLeft?.setText(String(score.left));
        this.scoreRight?.setText(String(score.right));
    }

    private cleanUp() {
        matchFeed.detach(this);
        this.unsubscribe?.();
        this.unsubscribe = null;
    }
}
