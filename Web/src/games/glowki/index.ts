import type { GameClient } from '@/kit/games';
import { EntityInfo, MatchDescription, matchFeed, Score } from './matchFeed';
import { SKIN_URLS } from './skins';
import MatchView from './MatchView';

const glowki: GameClient = {
    id: 'glowki',
    name: 'Główki',
    tagline: 'Piłka nożna głowami dla dwóch graczy',
    skins: SKIN_URLS,
    // Gospodarz (miejsce 0) gra po prawej, więc w lobby jego karta też jest po prawej.
    seatLabels: ['Gra po prawej', 'Gra po lewej'],
    seatOrder: [1, 0],
    events: {
        Frame: (frame: number[]) => matchFeed.frame(frame),
        Score: (score: Score) => matchFeed.score(score),
        BonusSpawned: (bonus: EntityInfo) => matchFeed.bonusSpawned(bonus),
        BonusRemoved: (id: number) => matchFeed.bonusRemoved(id),
    },
    matchStarted: (description: MatchDescription) => matchFeed.reset(description),
    MatchView,
};

export default glowki;
