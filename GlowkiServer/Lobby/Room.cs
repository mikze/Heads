using GlowkiServer.Game;
using GlowkiServer.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GlowkiServer.Lobby
{
    public class LobbyPlayer
    {
        public string ConnectionId { get; init; }
        public string Nick { get; init; }
        public int Skin { get; set; }
        public bool Ready { get; set; }
    }

    public record JoinResult(bool Ok, string Error);

    /// <summary>
    /// Jeden pokój na dwóch graczy. Pierwszy, kto wejdzie, jest gospodarzem:
    /// gra po prawej i to on startuje mecz.
    /// </summary>
    public class Room
    {
        public const string Group = "room";
        public const int MaxPlayers = 2;
        public const int SkinCount = 3;
        private const int MaxNickLength = 16;
        private const int MaxChatLength = 200;

        private readonly object gate = new();
        private readonly List<LobbyPlayer> players = new();
        private readonly IHubContext<GameHub> hub;
        private Match match;

        public Room(IHubContext<GameHub> hub)
        {
            this.hub = hub;
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
                if (players.Count >= MaxPlayers)
                    return new JoinResult(false, "Pokój jest pełny");
                if (match != null)
                    return new JoinResult(false, "Trwa mecz, spróbuj za chwilę");

                players.Add(new LobbyPlayer { ConnectionId = connectionId, Nick = nick });
            }

            Console.WriteLine($"{nick} dołączył do pokoju.");
            return new JoinResult(true, null);
        }

        /// <summary>Gracz wychodzi z pokoju (rozłączenie albo powrót do menu).</summary>
        public async Task Leave(string connectionId)
        {
            LobbyPlayer player;
            bool matchEnded;
            lock (gate)
            {
                player = players.FirstOrDefault(p => p.ConnectionId == connectionId);
                if (player == null) return;
                players.Remove(player);
                matchEnded = StopMatchLocked();
            }

            Console.WriteLine($"{player.Nick} opuścił pokój.");
            if (matchEnded)
                await Everyone.SendAsync("MatchEnded", $"{player.Nick} opuścił mecz");
            await SystemChat($"{player.Nick} opuścił pokój");
            await BroadcastLobby();
        }

        public async Task SetSkin(string connectionId, int skin)
        {
            lock (gate)
            {
                var player = Find(connectionId);
                if (player == null || match != null) return;
                player.Skin = ((skin % SkinCount) + SkinCount) % SkinCount;
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
            {
                nick = Find(connectionId)?.Nick;
            }
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
                if (players.Count < MaxPlayers)
                    return "Poczekaj na drugiego gracza";
                if (players.Any(p => !p.Ready))
                    return "Obaj gracze muszą być gotowi";

                var host = players[0];
                var guest = players[1];
                match = new Match(new MatchPlayer(host.Nick, host.Skin), new MatchPlayer(guest.Nick, guest.Skin), Everyone);
                description = match.Describe();
                match.Start();
            }

            Console.WriteLine("Mecz wystartował.");
            await Everyone.SendAsync("MatchStarted", description);
            await BroadcastLobby();
            return null;
        }

        public void Input(string connectionId, int input)
        {
            lock (gate)
            {
                if (match == null) return;
                var index = players.FindIndex(p => p.ConnectionId == connectionId);
                if (index < 0) return;
                match.SetInput(index == 0 ? Side.Right : Side.Left, input);
            }
        }

        /// <summary>Któryś gracz kończy mecz — obaj wracają do lobby.</summary>
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

        public Task SystemChat(string text) =>
            Everyone.SendAsync("Chat", new { nick = "", text, system = true });

        public Task BroadcastLobby() => Everyone.SendAsync("Lobby", Snapshot());

        public object Snapshot()
        {
            lock (gate)
            {
                return new
                {
                    inMatch = match != null,
                    players = players.Select((p, i) => new
                    {
                        id = p.ConnectionId,
                        nick = p.Nick,
                        skin = p.Skin,
                        ready = p.Ready,
                        isHost = i == 0
                    }).ToArray()
                };
            }
        }

        private LobbyPlayer Find(string connectionId) =>
            players.FirstOrDefault(p => p.ConnectionId == connectionId);

        // Wywoływać pod lock(gate). Po meczu obaj gracze muszą ponownie zgłosić gotowość.
        private bool StopMatchLocked()
        {
            if (match == null) return false;
            match.Dispose();
            match = null;
            foreach (var p in players)
                p.Ready = false;
            Console.WriteLine("Mecz zakończony.");
            return true;
        }
    }
}
