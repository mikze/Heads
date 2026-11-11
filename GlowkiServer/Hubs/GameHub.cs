using GlowkiServer.Lobby;
using Microsoft.AspNetCore.SignalR;

namespace GlowkiServer.Hubs
{
    /// <summary>Punkt kontaktu z przeglądarką (SignalR, ścieżka /game).</summary>
    public class GameHub(RoomManager rooms) : Hub
    {
        private string Me => Context.ConnectionId;
        private Room MyRoom => rooms.For(Me);

        // Nowe połączenie zaczyna w menu, więc od razu dostaje zmiany listy pokoi.
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Me, RoomManager.MenuGroup);
            await base.OnConnectedAsync();
        }

        public object[] ListRooms() => rooms.List();

        public Task<object> CreateRoom(string nick, string name) => Enter(rooms.Create(Me, nick, name));

        public Task<object> Join(string nick, string roomId) => Enter(rooms.Join(Me, roomId, nick));

        private async Task<object> Enter(JoinResult result)
        {
            if (!result.Ok)
                return new { ok = false, error = result.Error };

            var room = result.Room;
            await Groups.RemoveFromGroupAsync(Me, RoomManager.MenuGroup);
            await Groups.AddToGroupAsync(Me, room.Group);
            var nick = room.NickOf(Me);
            await room.SystemChat($"{nick} dołączył do pokoju");
            await room.BroadcastLobby();
            return new { ok = true, id = Me, lobby = room.Snapshot() };
        }

        public async Task LeaveRoom()
        {
            var room = await rooms.Leave(Me);
            if (room != null)
                await Groups.RemoveFromGroupAsync(Me, room.Group);
            await Groups.AddToGroupAsync(Me, RoomManager.MenuGroup);
        }

        public Task SetSkin(int skin) => MyRoom?.SetSkin(Me, skin) ?? Task.CompletedTask;

        public Task SetReady(bool ready) => MyRoom?.SetReady(Me, ready) ?? Task.CompletedTask;

        public Task SendChat(string text) => MyRoom?.Chat(Me, text) ?? Task.CompletedTask;

        public Task<string> StartMatch() => MyRoom?.StartMatch(Me) ?? Task.FromResult("Nie jesteś w pokoju");

        public void SendInput(int input) => MyRoom?.Input(Me, input);

        public Task LeaveMatch() => MyRoom?.LeaveMatch(Me) ?? Task.CompletedTask;

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await rooms.Leave(Me);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
