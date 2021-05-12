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
        EntityWrap player, enemyPlayer, foot, enemyFoot, ball;
        public static List<EntityWrap> bonuses = new List<EntityWrap>();
        private Vector2 ballPosition;
        private Vector2 playerPosition;
        private Vector2 enemyPlayerPosition;
        RevoluteJoint legJoint, enemyLegJoint;
        float playerFootSpeed, enemyFootSpeed;
        private bool resetPositions;
        EntityFactory entityFactory;
        public static List<Action> actionsBetweenWorldIteration = new List<Action>();
        public GameState(World world)
        {
            Console.WriteLine("GameState.");
            Chat.ChatRoom.onCommandRecived += CommandHandler;
            this.world = world;
        }

        private void CommandHandler(Message msg)
        {
            if(msg.Msg == "!disconnect")
             Console.WriteLine($"{msg.NickName} Disconnected.");
            if (msg.Msg == "!live")
                _ = ChatService._chatroomService.BroadcastMessageAsync(new Message() { NickName = "Server", Msg = "!live" });
        }

        public override void HandleState()
        {
            try
            {
                Thread.Sleep(10);
                if (isLoaded)
                {
                    const float TimeStep = 1.0f / 60;
                    const int VelocityIterations = 6;
                    const int PositionIterations = 2;
                    world.Step(TimeStep, VelocityIterations, PositionIterations);
                    if(resetPositions)
                    {
                        world.ClearForces();

                        ball.body.SetLinearVelocity(new Vector2(0, 0));
                        ball.body.SetAngularVelocity(0);
                        ball.body.SetTransform(ballPosition, 0);

                        player.body.SetLinearVelocity(new Vector2(0, 0));
                        player.body.SetAngularVelocity(0);
                        player.body.SetTransform(playerPosition, 0);

                        enemyPlayer.body.SetLinearVelocity(new Vector2(0, 0));
                        enemyPlayer.body.SetAngularVelocity(0);
                        enemyPlayer.body.SetTransform(enemyPlayerPosition, 0);

                        world.ClearForces();
                        resetPositions = false;
                    }
                    if(actionsBetweenWorldIteration.Any())
                    {
                        foreach (var action in actionsBetweenWorldIteration)
                            action.Invoke();

                        actionsBetweenWorldIteration.Clear();
                    }
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
            entityFactory = new EntityFactory(new NormalBodyFactory(world));
            var entitySet = EntitySets.GameStateSet(entityFactory);

            var User = Chat.ChatRoom.users.Where(x => x.Value.Admin).FirstOrDefault();
            var enemyUser = Chat.ChatRoom.users.Where(x => !x.Value.Admin).FirstOrDefault();

            ball = entityFactory.CreateDynamicCircle(520, 435, 15f, "Ball", true);            
            player = entityFactory.CreateDynamicPlayer(917, 752, 30, $"{User.Key},{User.Value.Skin}", true);
            enemyPlayer = entityFactory.CreateDynamicPlayer(112, 752, 30, $"{enemyUser.Key},{enemyUser.Value.Skin}", true);
            foot = entityFactory.CreateDynamicBox(250, 350, new Vector2(35, 10), "rfoot", true);
            enemyFoot = entityFactory.CreateDynamicBox(250, 350, new Vector2(60, 10), "foot", true);
            legJoint = entityFactory.CreateRevoluteJointJoint(player, foot);
            enemyLegJoint = entityFactory.CreateRevoluteJointJoint(enemyPlayer, enemyFoot, true);
            playerFootSpeed = legJoint.MotorSpeed;
            enemyFootSpeed = enemyLegJoint.MotorSpeed;

            ballPosition = ball.body.Position;
            playerPosition = player.body.Position;
            enemyPlayerPosition = enemyPlayer.body.Position;


            entitySet.Add(ball);
            entitySet.Add(enemyPlayer);
            entitySet.Add(player);
            entitySet.Add(foot);
            entitySet.Add(enemyFoot);

            Game.Game.Entities = entitySet;

            var inputHandler = new InputHandler();
            inputHandler.ReciveHandler = handleInput;

            if (UDPServer is null)
                UDPServer = new UDPServer<Frame>(1337);

            MyContactListener.playerScore += PlayerScore;
            MyContactListener.enemyScore += EnemyScore;
            Game.Game.IsLive = true;
            isLoaded = true;
        }

        private void EnemyScore()
        {
            resetPositions = true;
            _ = ChatService._chatroomService.BroadcastMessageAsync(new Message() { NickName = "Server", Msg = "EnemyScore" });
        }

        private void BonusAppers()
        {
            var posX = 300;
            var posY = 753;
            var entity = entityFactory.CreateSpeedBonus(posX, posY, new Vector2(30, 30), "speedBonus");
            bonuses.Add(entity);
            _ = ChatService._chatroomService.BroadcastMessageAsync(new Message() { NickName = "Server", Msg = $"bonus,speed,{posX},{posY},{entity.entity.Id}"});
        }

        private void PlayerScore()
        {
            resetPositions = true;
            _ = ChatService._chatroomService.BroadcastMessageAsync(new Message() { NickName = "Server", Msg = "PlayerScore" });
            actionsBetweenWorldIteration.Add(BonusAppers);
        }

        private void handleInput(int input, int clientId)
        {
            var inputs = (Input)input;

            if (inputs.HasFlag(Input.right))
                if (inputs.HasFlag(Input.enemy)) enemyPlayer.body.ApplyForceToCenter(new Vector2(5, 0)); else player.body.ApplyForceToCenter(new Vector2(5, 0));
            if (inputs.HasFlag(Input.left))
                if (inputs.HasFlag(Input.enemy)) enemyPlayer.body.ApplyForceToCenter(new Vector2(-5, 0)); else player.body.ApplyForceToCenter(new Vector2(-5, 0));
            if (inputs.HasFlag(Input.enemy))
            {
                if (inputs.HasFlag(Input.kick))
                {
                    if (!enemyLegJoint.IsMotorEnabled)
                    {
                        enemyLegJoint.EnableMotor(true);
                    }
                    else
                    {
                        enemyLegJoint.MotorSpeed = enemyFootSpeed;
                    }
                }
                if (inputs.HasFlag(Input.kickUp))
                {
                    enemyLegJoint.MotorSpeed = -enemyFootSpeed;
                }
            }
            else
            {
                if (inputs.HasFlag(Input.kick))
                {
                    if (!legJoint.IsMotorEnabled)
                    {
                        legJoint.EnableMotor(true);
                    }
                    else
                    {
                        legJoint.MotorSpeed = playerFootSpeed;
                    }
                }
                if (inputs.HasFlag(Input.kickUp))
                {
                    legJoint.MotorSpeed = -playerFootSpeed;
                }
            }
            if (inputs.HasFlag(Input.up) && MyContactListener.CANJUMP > 0)
                if (inputs.HasFlag(Input.enemy)) enemyPlayer.body.ApplyLinearImpulseToCenter(new Vector2(0, -0.4f)); else player.body.ApplyLinearImpulseToCenter(new Vector2(0, -0.4f));
        }

        public override void SetToGameState()
        {
            _stateHandler.state = States.GameState;        
        }

        public override void SetToLobbyState()
        {
            Game.Game.IsLive = false;
            _stateHandler.state = States.LobbyState;
            _stateHandler.ChangeScene(new LobbyState());
        }
    }
}
