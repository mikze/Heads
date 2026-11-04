// Dane meczu dla sceny Phasera. Pozycje przychodzą ~100 razy na sekundę,
// więc nie przechodzą przez Reduxa — scena czyta je stąd bezpośrednio.

export enum EntityKind {
    Box = 1,
    Ball = 2,
    Player = 3,
    Bonus = 4,
}

export interface EntityInfo {
    id: number;
    kind: EntityKind;
    positionX: number;
    positionY: number;
    sizeX: number;
    sizeY: number;
    params: string;
}

export interface MatchListener {
    onFrame(frame: number[]): void;
    onBonusSpawned(bonus: EntityInfo): void;
    onBonusRemoved(id: number): void;
}

class MatchFeed {
    entities: EntityInfo[] = [];
    bonuses = new Map<number, EntityInfo>();
    lastFrame: number[] | null = null;
    private listener: MatchListener | null = null;

    reset(entities: EntityInfo[]) {
        this.entities = entities;
        this.bonuses.clear();
        this.lastFrame = null;
    }

    attach(listener: MatchListener) { this.listener = listener; }
    detach(listener: MatchListener) { if (this.listener === listener) this.listener = null; }

    frame(frame: number[]) {
        this.lastFrame = frame;
        this.listener?.onFrame(frame);
    }

    bonusSpawned(bonus: EntityInfo) {
        this.bonuses.set(bonus.id, bonus);
        this.listener?.onBonusSpawned(bonus);
    }

    bonusRemoved(id: number) {
        this.bonuses.delete(id);
        this.listener?.onBonusRemoved(id);
    }
}

export const matchFeed = new MatchFeed();
