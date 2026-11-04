using Grpc.Core;
using System.Threading.Tasks;
using static GlowkiServer.ChatRoom;

namespace GlowkiServer.Services
{
    public class ChatService : ChatRoomBase
    {
        public static readonly Chat.ChatRoom _chatroomService = new Chat.ChatRoom();

        public ChatService()
        {

        }
        public override async Task join(IAsyncStreamReader<Message> requestStream, IServerStreamWriter<Message> responseStream, ServerCallContext context)
        {
            if (!await requestStream.MoveNext()) return;

            var nickName = requestStream.Current.NickName;
            if (!_chatroomService.Join(nickName, responseStream))
            {
                await responseStream.WriteAsync(new Message() { NickName = "Server", Msg = "!NameTaken" });
                return;
            }

            try
            {
                do
                {
                    if (requestStream.Current.Msg.StartsWith("!"))
                        await _chatroomService.HandleMessageAsCommand(requestStream.Current);
                    else
                        await _chatroomService.BroadcastMessageAsync(requestStream.Current);
                } while (await requestStream.MoveNext());
            }
            finally
            {
                _chatroomService.Remove(nickName, responseStream);
            }
        }
    }
}
