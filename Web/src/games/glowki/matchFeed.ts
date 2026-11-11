// Dane meczu Główek. Pozycje przychodzą ~100 razy na sekundę, więc nie przechodzą
// przez Reduxa — scena Phasera czyta je stąd, a React subskrybuje wynik (subscribe/info).

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

export interface MatchPlayer {
    nick: string;
    skin: number;
}

export interface Score {
    left: number;
    right: number;
}

/** Opis meczu z serwera (Match.Describe). */
export interface MatchDescription {
    entities: EntityInfo[];
    right: MatchPlayer;
    left: MatchPlayer;
    score: Score;
}

/** Część stanu, którą pokazuje React — nowy obiekt przy każdej zmianie. */
export interface MatchInfo {
    right: MatchPlayer | null;
    left: MatchPlayer | null;
    score: Score;
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
    info: MatchInfo = { right: null, left: null, score: { left: 0, right: 0 } };
    private listener: MatchListener | null = null;
    private subscribers = new Set<() => void>();

    reset(match: MatchDescription) {
        this.entities = match.entities;
        this.bonuses.clear();
        this.lastFrame = null;
        this.setInfo({ right: match.right, left: match.left, score: match.score });
    }

    score(score: Score) {
        this.setInfo({ ...this.info, score });
    }

    /** Dla useSyncExternalStore. */
    subscribe = (fn: () => void) => {
        this.subscribers.add(fn);
        return () => { this.subscribers.delete(fn); };
    };

    getInfo = () => this.info;

    private setInfo(info: MatchInfo) {
        this.info = info;
        this.subscribers.forEach((fn) => fn());
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
