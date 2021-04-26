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
using Box2D.NetStandard.Dynamics.Joints.Revolute;

namespace GlowkiServer.States
{
    internal class GameState : State
    {
        UDPServer<Frame> UDPServer;
        World world;
        private bool isLoaded;
        EntityWrap player, enemyPlayer, foot;
        RevoluteJoint legJoint;

        public GameState(World world)
        {
            Console.WriteLine("GameState.");
            this.world = world;
        }

        public override void HandleState()
        {
            try
            {
                //Console.WriteLine($"{player.body.Position} {Game.Game.Entities.First(x => x.entity.Params == "Ball").body.Position} {enemyPlayer.body.Position}");
                Thread.Sleep(10);
                if (isLoaded)
                {
                    const float TimeStep = 1.0f / 60;
                    const int VelocityIterations = 6;
                    const int PositionIterations = 2;
                    world.Step(TimeStep, VelocityIterations, PositionIterations);

                    foreach (var ew in Game.Game.Entities.Where(x => x.clientRefresh))
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
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public override void LoadContent()
        {
            EntityFactory entityFactory = new EntityFactory(new NormalBodyFactory(world));

            var entitySet = EntitySets.GameStateSet(entityFactory);       
            player = entityFactory.CreateDynamicPlayer(693, 405, 30, "mikze", true);
            enemyPlayer = entityFactory.CreateDynamicPlayer(105, 405, 30, "mikze2", true);
            foot = entityFactory.CreateDynamicBox(250, 350, new Vector2(60, 10), "foot", true);
            legJoint = entityFactory.CreateRevoluteJointJoint(player, foot);

            entityFactory.CreateDistanceJointJoint(player, foot);
            entitySet.Add(enemyPlayer);
            entitySet.Add(player);
            entitySet.Add(foot);
            Game.Game.Entities = entitySet;

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
            if (inputs.HasFlag(Input.kick))
            {
                if (inputs.HasFlag(Input.enemy))
                {
                    Console.WriteLine("Turn on/off");
                    if (!legJoint.IsMotorEnabled)
                        legJoint.EnableMotor(true);
                    else
                        legJoint.EnableMotor(false);
                }
                else
                {
                    legJoint.MotorSpeed = -legJoint.MotorSpeed;
                }
            }
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
