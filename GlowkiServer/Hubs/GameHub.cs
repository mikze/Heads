using GlowkiServer.Lobby;
using Microsoft.AspNetCore.SignalR;

namespace GlowkiServer.Hubs
{
    /// <summary>Punkt kontaktu z przeglądarką (SignalR, ścieżka /game).</summary>
    public class GameHub(Room room) : Hub
    {
        public async Task<object> Join(string nick)
        {
            var result = room.Join(Context.ConnectionId, nick);
            if (!result.Ok)
                return new { ok = false, error = result.Error };

            await Groups.AddToGroupAsync(Context.ConnectionId, Room.Group);
            await room.SystemChat($"{nick.Trim()} dołączył do pokoju");
            await room.BroadcastLobby();
            return new { ok = true, id = Context.ConnectionId, lobby = room.Snapshot() };
        }

        public async Task LeaveRoom()
        {
            await room.Leave(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Room.Group);
        }

        public Task SetSkin(int skin) => room.SetSkin(Context.ConnectionId, skin);

        public Task SetReady(bool ready) => room.SetReady(Context.ConnectionId, ready);

        public Task SendChat(string text) => room.Chat(Context.ConnectionId, text);

        public Task<string> StartMatch() => room.StartMatch(Context.ConnectionId);

        public void SendInput(int input) => room.Input(Context.ConnectionId, input);

        public Task LeaveMatch() => room.LeaveMatch(Context.ConnectionId);

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await room.Leave(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
