using GlowkiServer;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GlowkiServer.GlowkiService;

namespace Glowki
{
    public static class ProtoHelper
    {
        static GlowkiServiceClient client;
        static AsyncDuplexStreamingCall<Message, Message> chat;
        public static string nickname;
        public static string _IP { get; set; }
        public static void Initialize(string IP = "127.0.0.1")
        {
            _IP = IP;
        }

        static async Task<IEnumerable<GlowkiServer.Entity>> AsyncLoadEntities()
        {
            var input = new EntitiesRequest();
            var channel = GrpcChannel.ForAddress($"http://{_IP}:5000");
            var client = new GlowkiServiceClient(channel);
            var t = client.GetEntities(input);

            List<GlowkiServer.Entity> l = new List<GlowkiServer.Entity>();
            await foreach (var e in t.ResponseStream.ReadAllAsync())
                l.Add(e);

            return l;
        }

        public static IEnumerable<GlowkiServer.Entity> LoadEntities()
        {
            var en = AsyncLoadEntities();
            en.Wait();
            return en.Result;
        }

        public static void SetGameState()
        {
            var input = new EntitiesRequest();
            var channel = GrpcChannel.ForAddress($"http://{_IP}:5000");
            var client = new GlowkiServiceClient(channel);
            client.SetState(new State() { State_ = 1 });
        }
        public async static Task JoinToChatAsync()
        {
            AppContext.SetSwitch(
    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress($"http://{_IP}:5000", new GrpcChannelOptions() { Credentials = ChannelCredentials.Insecure });
            var Client = new ChatRoom.ChatRoomClient(channel);

            chat = Client.join();         
                await chat.RequestStream.WriteAsync(new Message { NickName = nickname, Msg = $"Gowno has joined the room" });
                while (await chat.ResponseStream.MoveNext(cancellationToken: CancellationToken.None))
                {
                    var response = chat.ResponseStream.Current;
                    OnChatRecieve.Invoke(response.NickName, response.Msg);
                }
        }
        public async static void WriteToChat(string msg)
        {
            await chat.RequestStream.WriteAsync(new Message { NickName = nickname, Msg = msg });
        }

        public static Action<string,string> OnChatRecieve = (nichName, message) => { };
    }
}
