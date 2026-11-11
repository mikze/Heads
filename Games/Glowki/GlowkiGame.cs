using GameKit;

namespace Glowki
{
    /// <summary>Główki — piłka nożna głowami dla dwóch graczy.</summary>
    public sealed class GlowkiGame : IGameDefinition
    {
        public string Id => "glowki";
        public string Name => "Główki";
        public int MinPlayers => 2;
        public int MaxPlayers => 2;
        public int SkinCount => 3;

        // Miejsce 0 (gospodarz) gra po prawej, miejsce 1 po lewej.
        public IGameSession CreateSession(IReadOnlyList<GamePlayer> players, IMatchContext context) =>
            new Match(
                new MatchPlayer(players[0].Nick, players[0].Skin),
                new MatchPlayer(players[1].Nick, players[1].Skin),
                context);
    }
}
