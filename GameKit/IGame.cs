namespace GameKit
{
    /// <summary>
    /// Opis gry rejestrowanej na serwerze. GameKit zajmuje się pokojami, lobby i czatem,
    /// a gra dostarcza tylko zasady samego meczu.
    /// </summary>
    public interface IGameDefinition
    {
        /// <summary>Krótki identyfikator, np. "glowki". Ten sam musi znać front.</summary>
        string Id { get; }
        string Name { get; }
        int MinPlayers { get; }
        int MaxPlayers { get; }
        /// <summary>Ile wyglądów postaci można wybrać w lobby (0, jeśli gra ich nie ma).</summary>
        int SkinCount { get; }

        /// <summary>Tworzy mecz. Gracze są w kolejności miejsc: pierwszy to gospodarz pokoju.</summary>
        IGameSession CreateSession(IReadOnlyList<GamePlayer> players, IMatchContext context);
    }

    /// <summary>Jeden trwający mecz.</summary>
    public interface IGameSession : IDisposable
    {
        /// <summary>Stan początkowy wysyłany klientom w zdarzeniu "MatchStarted".</summary>
        object Describe();

        void Start();

        /// <summary>Sterowanie od gracza z danego miejsca. Może przyjść z dowolnego wątku.</summary>
        void SetInput(int seat, int input);
    }

    /// <param name="Seat">Miejsce w pokoju, od 0 (gospodarz).</param>
    public record GamePlayer(int Seat, string Nick, int Skin);

    /// <summary>To, co mecz może zrobić na zewnątrz: wysłać coś graczom albo sam się zakończyć.</summary>
    public interface IMatchContext
    {
        /// <summary>Wysyła zdarzenie do wszystkich graczy w pokoju (bez czekania).</summary>
        void Send(string eventName, object payload);

        /// <summary>Kończy mecz, np. po zdobyciu limitu punktów. Gracze wracają do lobby.</summary>
        void End(string reason);
    }
}
