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
        public LobbyState()
        {
            world = new World(new System.Numerics.Vector2(0.0f, 1.500000000000000e+01f));
            Console.WriteLine("LobbyState.");
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
            _stateHandler.state = States.GameState;
            _stateHandler.ChangeScene(new GameState(world));
        }

        public override void SetToLobbyState()
        {
            _stateHandler.state = States.LobbyState;
        }
    }
}
