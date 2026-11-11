using GlowkiServer.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GlowkiServer.Lobby
{
    /// <summary>
    /// Wszystkie pokoje na serwerze. Pamięta, w którym pokoju jest każde połączenie,
    /// tworzy pokoje na żądanie i usuwa je, gdy wyjdzie ostatni gracz.
    /// </summary>
    public class RoomManager(IHubContext<GameHub> hub)
    {
        /// <summary>Grupa SignalR graczy w menu — dostają listę pokoi na bieżąco.</summary>
        public const string MenuGroup = "menu";
        private const int MaxRooms = 50;
        private const int MaxNameLength = 24;
        private const string IdAlphabet = "abcdefghjkmnpqrstuvwxyz23456789";

        private readonly object gate = new();
        private readonly Dictionary<string, Room> rooms = new();
        private readonly Dictionary<string, Room> byConnection = new();

        public Room For(string connectionId)
        {
            lock (gate)
                return byConnection.GetValueOrDefault(connectionId);
        }

        public JoinResult Create(string connectionId, string nick, string name)
        {
            name = name?.Trim() ?? "";
            if (name.Length == 0) name = $"Pokój {nick?.Trim()}";
            if (name.Length > MaxNameLength) name = name[..MaxNameLength];

            lock (gate)
            {
                if (byConnection.ContainsKey(connectionId))
                    return new JoinResult(false, "Już jesteś w pokoju");
                if (rooms.Count >= MaxRooms)
                    return new JoinResult(false, "Za dużo pokoi na serwerze, spróbuj później");

                var room = new Room(NewId(), name, hub, BroadcastRooms);
                var result = room.Join(connectionId, nick);
                if (!result.Ok)
                    return result;

                rooms[room.Id] = room;
                byConnection[connectionId] = room;
                return result;
            }
        }

        public JoinResult Join(string connectionId, string roomId, string nick)
        {
            lock (gate)
            {
                if (byConnection.ContainsKey(connectionId))
                    return new JoinResult(false, "Już jesteś w pokoju");
                if (roomId == null || !rooms.TryGetValue(roomId.Trim().ToLowerInvariant(), out var room))
                    return new JoinResult(false, "Taki pokój nie istnieje");

                var result = room.Join(connectionId, nick);
                if (result.Ok)
                    byConnection[connectionId] = room;
                return result;
            }
        }

        /// <summary>Wyjście z pokoju (powrót do menu albo rozłączenie). Zwraca pokój, z którego gracz wyszedł.</summary>
        public async Task<Room> Leave(string connectionId)
        {
            Room room;
            lock (gate)
            {
                if (!byConnection.Remove(connectionId, out room))
                    return null;
            }

            await room.Leave(connectionId);

            bool removed = false;
            lock (gate)
            {
                // Ktoś mógł wejść w międzyczasie, więc sprawdzamy jeszcze raz pod blokadą.
                if (room.PlayerCount == 0 && rooms.Remove(room.Id))
                {
                    room.Dispose();
                    removed = true;
                }
            }
            if (removed)
            {
                Console.WriteLine($"Pokój \"{room.Name}\" ({room.Id}) zamknięty.");
                await BroadcastRooms();
            }
            return room;
        }

        public object[] List()
        {
            Room[] all;
            lock (gate)
                all = rooms.Values.ToArray();
            return all.Select(r => r.Info()).ToArray();
        }

        public Task BroadcastRooms() => hub.Clients.Group(MenuGroup).SendAsync("Rooms", List());

        // Wywoływać pod lock(gate).
        private string NewId()
        {
            string id;
            do
                id = string.Concat(Enumerable.Range(0, 5).Select(_ => IdAlphabet[Random.Shared.Next(IdAlphabet.Length)]));
            while (rooms.ContainsKey(id));
            return id;
        }
    }
}
