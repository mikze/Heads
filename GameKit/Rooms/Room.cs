using GameKit.Hosting;
using Microsoft.AspNetCore.SignalR;

namespace GameKit.Rooms
{
    public class LobbyPlayer
    {
        public string ConnectionId { get; init; }
        public string Nick { get; init; }
        public int Skin { get; set; }
        public bool Ready { get; set; }
    }

    public record JoinResult(bool Ok, string Error, Room Room = null);

    /// <summary>
    /// Pokój jednej gry. Pierwszy, kto wejdzie, jest gospodarzem i to on startuje mecz.
    /// Kolejność graczy wyznacza ich miejsca w meczu (0 = gospodarz).
    /// </summary>
    public sealed class Room : IDisposable
    {
        private const int MaxNickLength = 16;
        private const int MaxChatLength = 200;

        private readonly object gate = new();
        private readonly List<LobbyPlayer> players = new();
        private readonly IHubContext<GameHub> hub;
        private readonly Func<Task> changed;
        private IGameSession match;

        public string Id { get; }
        public string Name { get; }
        public IGameDefinition Game { get; }

        /// <summary>Grupa SignalR tego pokoju — czat, lobby i zdarzenia meczu idą tylko do jego graczy.</summary>
        public string Group => $"room:{Id}";

        /// <param name="changed">Wywoływane po każdej zmianie widocznej na liście pokoi.</param>
        public Room(string id, string name, IGameDefinition game, IHubContext<GameHub> hub, Func<Task> changed)
        {
            Id = id;
            Name = name;
            Game = game;
            this.hub = hub;
            this.changed = changed;
        }

        public int PlayerCount
        {
            get { lock (gate) return players.Count; }
        }

        private IClientProxy Everyone => hub.Clients.Group(Group);

        public JoinResult Join(string connectionId, string nick)
        {
            nick = nick?.Trim() ?? "";
            if (nick.Length == 0)
                return new JoinResult(false, "Podaj nick");
            if (nick.Length > MaxNickLength)
                return new JoinResult(false, $"Nick może mieć najwyżej {MaxNickLength} znaków");

            lock (gate)
            {
                if (players.Any(p => p.ConnectionId == connectionId))
                    return new JoinResult(false, "Już jesteś w pokoju");
                if (players.Any(p => string.Equals(p.Nick, nick, StringComparison.OrdinalIgnoreCase)))
                    return new JoinResult(false, $"Nick \"{nick}\" jest zajęty");
                if (players.Count >= Game.MaxPlayers)
                    return new JoinResult(false, "Pokój jest pełny");
                if (match != null)
                    return new JoinResult(false, "Trwa mecz, spróbuj za chwilę");

                players.Add(new LobbyPlayer { ConnectionId = connectionId, Nick = nick });
            }

            Console.WriteLine($"{nick} dołączył do pokoju \"{Name}\" ({Id}, {Game.Id}).");
            return new JoinResult(true, null, this);
        }

        /// <summary>Gracz wychodzi z pokoju (rozłączenie albo powrót do menu).</summary>
        public async Task Leave(string connectionId)
        {
            LobbyPlayer player;
            bool matchEnded;
            lock (gate)
            {
                player = Find(connectionId);
                if (player == null) return;
                players.Remove(player);
                matchEnded = StopMatchLocked();
            }

            Console.WriteLine($"{player.Nick} opuścił pokój \"{Name}\" ({Id}).");
            if (matchEnded)
                await Everyone.SendAsync("MatchEnded", $"{player.Nick} opuścił mecz");
            await SystemChat($"{player.Nick} opuścił pokój");
            await BroadcastLobby();
        }

        public async Task SetSkin(string connectionId, int skin)
        {
            var count = Game.SkinCount;
            if (count <= 0) return;
            lock (gate)
            {
                var player = Find(connectionId);
                if (player == null || match != null) return;
                player.Skin = ((skin % count) + count) % count;
            }
            await BroadcastLobby();
        }

        public async Task SetReady(string connectionId, bool ready)
        {
            lock (gate)
            {
                var player = Find(connectionId);
                if (player == null || match != null) return;
                player.Ready = ready;
            }
            await BroadcastLobby();
        }

        public async Task Chat(string connectionId, string text)
        {
            text = text?.Trim() ?? "";
            if (text.Length == 0) return;
            if (text.Length > MaxChatLength) text = text[..MaxChatLength];

            string nick;
            lock (gate)
                nick = Find(connectionId)?.Nick;
            if (nick != null)
                await Everyone.SendAsync("Chat", new { nick, text, system = false });
        }

        /// <summary>Zwraca komunikat błędu albo null, gdy mecz wystartował.</summary>
        public async Task<string> StartMatch(string connectionId)
        {
            object description;
            lock (gate)
            {
                if (players.Count == 0 || players[0].ConnectionId != connectionId)
                    return "Mecz może rozpocząć tylko gospodarz";
                if (match != null)
                    return "Mecz już trwa";
                if (players.Count < Game.MinPlayers)
                    return Game.MinPlayers - players.Count == 1
                        ? "Poczekaj na kolejnego gracza"
                        : $"Potrzeba co najmniej {Game.MinPlayers} graczy";
                if (players.Any(p => !p.Ready))
                    return "Wszyscy gracze muszą być gotowi";

                var seats = players.Select((p, i) => new GamePlayer(i, p.Nick, p.Skin)).ToArray();
                var context = new MatchContext(this);
                var session = Game.CreateSession(seats, context);
                context.Session = session;
                match = session;
                description = session.Describe();
                session.Start();
            }

            Console.WriteLine($"Mecz w pokoju \"{Name}\" ({Id}) wystartował.");
            await Everyone.SendAsync("MatchStarted", description);
            await BroadcastLobby();
            return null;
        }

        public void Input(string connectionId, int input)
        {
            lock (gate)
            {
                if (match == null) return;
                var seat = players.FindIndex(p => p.ConnectionId == connectionId);
                if (seat >= 0)
                    match.SetInput(seat, input);
            }
        }

        /// <summary>Któryś gracz kończy mecz — wszyscy wracają do lobby.</summary>
        public async Task LeaveMatch(string connectionId)
        {
            string nick;
            lock (gate)
            {
                nick = Find(connectionId)?.Nick;
                if (nick == null || !StopMatchLocked()) return;
            }

            await Everyone.SendAsync("MatchEnded", $"{nick} zakończył mecz");
            await BroadcastLobby();
        }

        // Mecz kończy się sam (np. limit punktów). Wywoływane z wątku meczu,
        // więc resztę robimy w osobnym zadaniu — Dispose meczu czeka na zakończenie jego pętli.
        private void EndFromGame(IGameSession session, string reason) => _ = Task.Run(async () =>
        {
            lock (gate)
            {
                if (match != session || !StopMatchLocked()) return;
            }
            await Everyone.SendAsync("MatchEnded", reason);
            await BroadcastLobby();
        });

        public Task SystemChat(string text) =>
            Everyone.SendAsync("Chat", new { nick = "", text, system = true });

        public async Task BroadcastLobby()
        {
            await Everyone.SendAsync("Lobby", Snapshot());
            await changed();
        }

        public object Snapshot()
        {
            lock (gate)
            {
                return new
                {
                    id = Id,
                    name = Name,
                    game = Game.Id,
                    maxPlayers = Game.MaxPlayers,
                    minPlayers = Game.MinPlayers,
                    inMatch = match != null,
                    players = players.Select((p, i) => new
                    {
                        id = p.ConnectionId,
                        nick = p.Nick,
                        skin = p.Skin,
                        ready = p.Ready,
                        isHost = i == 0,
                        seat = i
                    }).ToArray()
                };
            }
        }

        /// <summary>Krótki opis do listy pokoi w menu.</summary>
        public object Info()
        {
            lock (gate)
            {
                return new
                {
                    id = Id,
                    name = Name,
                    game = Game.Id,
                    host = players.FirstOrDefault()?.Nick,
                    players = players.Count,
                    maxPlayers = Game.MaxPlayers,
                    inMatch = match != null
                };
            }
        }

        public void Dispose()
        {
            lock (gate)
                StopMatchLocked();
        }

        public string NickOf(string connectionId)
        {
            lock (gate)
                return Find(connectionId)?.Nick;
        }

        private LobbyPlayer Find(string connectionId) =>
            players.FirstOrDefault(p => p.ConnectionId == connectionId);

        // Wywoływać pod lock(gate). Po meczu wszyscy muszą ponownie zgłosić gotowość.
        private bool StopMatchLocked()
        {
            if (match == null) return false;
            match.Dispose();
            match = null;
            foreach (var p in players)
                p.Ready = false;
            Console.WriteLine($"Mecz w pokoju \"{Name}\" ({Id}) zakończony.");
            return true;
        }

        private sealed class MatchContext(Room room) : IMatchContext
        {
            /// <summary>Mecz, do którego należy ten kontekst — ustawiany zaraz po jego utworzeniu.</summary>
            public IGameSession Session;

            public void Send(string eventName, object payload) =>
                _ = room.Everyone.SendAsync(eventName, payload);

            public void End(string reason)
            {
                if (Session != null)
                    room.EndFromGame(Session, reason);
            }
        }
    }
}
