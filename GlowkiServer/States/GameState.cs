using GlowkiServer.UDP;
using System;
using System.Collections.Generic;
using System.Linq;
using Box2D.NetStandard.Dynamics.World;
using System.Threading.Tasks;
using GlowkiServer.B2DWorld;
using GlowkiServer.Game;
using System.Threading;
using System.Numerics;
using GlowkiServer.Services;

namespace GlowkiServer.States
{
    internal class GameState : State
    {
        UDPServer<Frame> UDPServer;
        World world;
        private bool isLoaded;
        EntityWrap player, enemyPlayer;

        public GameState(World world)
        {
            Console.WriteLine("GameState.");
            this.world = world;
        }

        public override void HandleState()
        {
            Thread.Sleep(10);
            if (isLoaded)
            {               
                const float TimeStep = 1.0f / 60;
                const int VelocityIterations = 6;
                const int PositionIterations = 2;
                world.Step(TimeStep, VelocityIterations, PositionIterations);

                foreach (var ew in Game.Game.Entities.Where(x => x.dynamic))
                {
                    UDPServer.BroadCast(new Frame
                    {
                        id = ew.entity.Id,
                        X = ew.body.Position.X,
                        Y = ew.body.Position.Y,
                        R = ew.body.Transform.GetAngle()
                    });
                }
            }
        }

        public override void LoadContent()
        {
            EntityFactory entityFactory = new EntityFactory(new NormalBodyFactory(world));
            player = entityFactory.CreateDynamicPlayer(300, 350, new Vector2(40, 40), "mikze");
            enemyPlayer = entityFactory.CreateDynamicPlayer(200, 350, new Vector2(40, 40), "mikze2");
            Game.Game.Entities = new List<EntityWrap>() {
            entityFactory.CreateDynamicCircle(200, 50, 30f, "circle"),
            entityFactory.CreateStaticBox(300, 50, new Vector2(1000, 30), ""),
            entityFactory.CreateStaticBox(300, 450, new Vector2(1000, 30), "floor"),
            entityFactory.CreateStaticBox(10, 350, new Vector2(10, 600), ""),
            entityFactory.CreateStaticBox(790, 350, new Vector2(10, 600), ""),
            entityFactory.CreateStaticBox(45, 325, new Vector2(100, 10), ""),
            entityFactory.CreateStaticBox(745, 325, new Vector2(100, 10), ""),
            entityFactory.CreateStaticBoxSensor(730, 330, new Vector2(30, 160), "EnemyGoal"),
            entityFactory.CreateStaticBoxSensor(55, 330, new Vector2(30, 160), "PlayerGoal"),
            player,
            enemyPlayer,
            };

            var inputHandler = new InputHandler();
            inputHandler.ReciveHandler = handleInput;

           

            if (UDPServer is null)
                UDPServer = new UDPServer<Frame>(1337);

            MyContactListener.playerScore += PlayerScore;
            MyContactListener.enemyScore += EnemyScore;
            isLoaded = true;
        }

        private void EnemyScore()
        {
            _ = ChatService._chatroomService.BroadcastMessageAsync(new Message() { NickName = "Server", Msg = "EnemyScore" });
        }

        private void PlayerScore()
        {
            _ = ChatService._chatroomService.BroadcastMessageAsync(new Message() { NickName = "Server", Msg = "PlayerScore" });
        }

        private void handleInput(int input, int clientId)
        {
            var inputs = (Input)input;
            if (inputs.HasFlag(Input.right))
                if (inputs.HasFlag(Input.enemy)) player.body.ApplyForceToCenter(new Vector2(5, 0)); else enemyPlayer.body.ApplyForceToCenter(new Vector2(5, 0));
            if (inputs.HasFlag(Input.left))
                if (inputs.HasFlag(Input.enemy)) player.body.ApplyForceToCenter(new Vector2(-5, 0)); else enemyPlayer.body.ApplyForceToCenter(new Vector2(-5, 0));
            if (inputs.HasFlag(Input.up) && MyContactListener.CANJUMP > 0)
                if (inputs.HasFlag(Input.enemy)) player.body.ApplyLinearImpulseToCenter(new Vector2(0, -0.4f)); else enemyPlayer.body.ApplyLinearImpulseToCenter(new Vector2(0, -0.4f));
        }

        public override void SetToGameState()
        {
            _stateHandler.state = States.GameState;        
        }

        public override void SetToLobbyState()
        {
            _stateHandler.state = States.LobbyState;
            _stateHandler.ChangeScene(new LobbyState());
        }
    }
}
