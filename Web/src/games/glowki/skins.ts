// Grafiki Główek. Wyglądy głów są w kolejności numerów (skin) używanych przez serwer.
export const ASSETS = '/games/glowki';
export const SKIN_TEXTURES = ['MiniHead1', 'MiniHead2', 'MiniHead3'];
export const SKIN_URLS = SKIN_TEXTURES.map((t) => `${ASSETS}/${t}.png`);
