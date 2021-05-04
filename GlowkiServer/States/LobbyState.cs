using Box2D.NetStandard.Dynamics.World;
using GlowkiServer.Game;
using GlowkiServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GlowkiServer.States
{
    internal class LobbyState : State
    {
        World world;
        public static bool enemyIsReady, playerIsReady;
        public LobbyState()
        {
            GlowkiServer.Chat.ChatRoom.onCommandRecived += CommandHandler;
            world = new World(new System.Numerics.Vector2(0.0f, 1.500000000000000e+01f));
            Console.WriteLine("LobbyState.");
        }

        private void CommandHandler(Message m)
        {
            if (m.Msg.ToLower() == "!live")
            {
                _ = ChatService._chatroomService.BroadcastMessageAsync(new Message() { NickName = "Server", Msg = "!NotLive" });
            }
            if (m.Msg == "!Start" && Chat.ChatRoom.users[m.NickName].Admin)
            {
                if (enemyIsReady && playerIsReady)
                {
                    Game.Game.stateHandler.SetToGameState();
                    _ = ChatService._chatroomService.BroadcastMessageAsync(new Message() { NickName = "Server", Msg = "Start" });
                }
            }

            if (m.Msg == "!Ready")
            {
                var admin = Chat.ChatRoom.users[m.NickName].Admin;
                if (admin)
                    playerIsReady = true;
                else
                    enemyIsReady = true;

                _ = ChatService._chatroomService.BroadcastMessageAsync(m);
            }
            if(m.Msg == "!NotReady")
            {
                var admin = Chat.ChatRoom.users[m.NickName].Admin;
                if (admin)
                    playerIsReady = false;
                else
                    enemyIsReady = false;

                _ = ChatService._chatroomService.BroadcastMessageAsync(m);
            }
            if (m.Msg.Contains("!SkinChange"))
            {
                var skin = m.Msg.Split(",")[1];
                Console.WriteLine(skin);
                var userToSend = Chat.ChatRoom.users.Where(x => x.Key != m.NickName).FirstOrDefault();
                userToSend.Value.Skin = int.Parse(skin);
                _ = ChatService._chatroomService.SendMessageToSubscriber(userToSend, new Message() { NickName = "Server", Msg = $"!skin,{skin}" });
            }
        }

        public override void HandleState()
        {
            Thread.Sleep(10);
        }

        public override void LoadContent()
        {
        }

        public override void SetToGameState()
        {
            if (enemyIsReady && playerIsReady)
            {
                _stateHandler.state = States.GameState;
                _stateHandler.ChangeScene(new GameState(world));
            }
        }

        public override void SetToLobbyState()
        {
            _stateHandler.state = States.LobbyState;
        }
    }
}
