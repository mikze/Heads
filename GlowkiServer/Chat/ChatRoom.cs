﻿using GlowkiServer.States;
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
        public IServerStreamWriter<Message> serverStreamWriter { get; set; }
    }

    public class ChatRoom
    {
        public static User GetAdmin() => users.Values.First(u => u.Admin);
        public static ConcurrentDictionary<string, User> users = new ConcurrentDictionary<string, User>();
        public static Action<Message> onCommandRecived = new Action<Message>(m => { });
        public void Join(string name, IServerStreamWriter<Message> response)
        {
            if (!users.Any(x => x.Key == name))
            {
                var user = new User() { serverStreamWriter = response, Admin = users.Any() ? false : true };
                if (users.Any())
                {
                    SendMessageToSubscriber(new KeyValuePair<string, User>("", user), new Message() { NickName = users.First().Key, Msg = string.Empty }).Wait();
                    SendMessageToSubscriber(new KeyValuePair<string, User>("", user), new Message() { NickName = "Server", Msg = "!Enemy" }).Wait();
                }

                users.TryAdd(name, user);
            }
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

        private async Task<KeyValuePair<string, User>?> SendMessageToSubscriber(KeyValuePair<string, User> user, Message message)
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
