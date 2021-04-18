using Box2D.NetStandard.Dynamics.World;
using GlowkiServer.Game;
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
        bool enemyIsReady, playerIsReady;
        public LobbyState()
        {
            GlowkiServer.Chat.ChatRoom.onCommandRecived += CommandHandler;
            world = new World(new System.Numerics.Vector2(0.0f, 1.500000000000000e+01f));
            Console.WriteLine("LobbyState.");
        }

        private void CommandHandler(Message m)
        {
            if(m.Msg == "!Ready")
            {
                var admin = Chat.ChatRoom.users[m.NickName].Admin;
                if (admin)
                    playerIsReady = true;
                else
                    enemyIsReady = true;
            }
            if(m.Msg == "!NotReady")
            {
                var admin = Chat.ChatRoom.users[m.NickName].Admin;
                if (admin)
                    playerIsReady = false;
                else
                    enemyIsReady = false;
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
