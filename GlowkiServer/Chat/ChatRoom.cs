using GlowkiServer.States;
using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlowkiServer.Chat
{
    public class User
    {
        public bool Admin { get; set; }
        public bool Ready { get; set; }
        public int Skin { get; set; }
        public IServerStreamWriter<Message> serverStreamWriter { get; set; }
    }

    public class ChatRoom
    {
        public static User GetAdmin() => users.Values.First(u => u.Admin);
        public static ConcurrentDictionary<string, User> users = new ConcurrentDictionary<string, User>();
        public static Action<Message> onCommandRecived = new Action<Message>(m => { });
        // Zwraca false, gdy nick jest już zajęty przez inne połączenie.
        public bool Join(string name, IServerStreamWriter<Message> response)
        {
            if (users.TryGetValue(name, out var existing))
                return existing.serverStreamWriter == response;

            {
                var user = new User() { serverStreamWriter = response, Admin = users.Any() ? false : true };
                if (users.Any())
                {
                    SendMessageToSubscriber(new KeyValuePair<string, User>("", user), new Message() { NickName = users.First().Key, Msg = string.Empty }).Wait();
                    SendMessageToSubscriber(new KeyValuePair<string, User>("", user), new Message() { NickName = "Server", Msg = "!Enemy" }).Wait();
                }

                users.TryAdd(name, user);
            }
            return true;
        }

        // Usuwa gracza tylko wtedy, gdy to jego połączenie się zakończyło.
        public void Remove(string name, IServerStreamWriter<Message> response)
        {
            if (users.TryGetValue(name, out var user) && user.serverStreamWriter == response)
                users.TryRemove(name, out _);
        }

        internal async Task HandleMessageAsCommand(Message current)
            => await new TaskFactory().StartNew(() => onCommandRecived.Invoke(current));

        public void Remove(string name) => users.TryRemove(name, out _);

        public async Task BroadcastMessageAsync(Message message) => await BroadcastMessages(message);

        private async Task BroadcastMessages(Message message)
        {
            foreach (var user in users.Where(x => x.Key != message.NickName))
            {
                var item = await SendMessageToSubscriber(user, message);
                if (item != null)
                {
                    Remove(item?.Key);
                };
            }
        }

        internal async Task<KeyValuePair<string, User>?> SendMessageToSubscriber(KeyValuePair<string, User> user, Message message)
        {
            try
            {
                await user.Value.serverStreamWriter.WriteAsync(message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return user;
            }
        }
    }
}
