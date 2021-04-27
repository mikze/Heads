﻿using GlowkiServer.UDP;
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
        private Vector2 ballPosition;
        private Vector2 playerPosition;
        private Vector2 enemyPlayerPosition;
        RevoluteJoint legJoint, enemyLegJoint;
        float playerFootSpeed, enemyFootSpeed;
        private bool resetPositions;

        public GameState(World world)
        {
            Console.WriteLine("GameState.");
            this.world = world;
        }

        public override void HandleState()
        {
            try
            {
                Thread.Sleep(10);
                if (isLoaded)
                {
                    //Console.WriteLine($"{player.body.Position} {enemyPlayer.body.Position}");
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

            ball = entityFactory.CreateDynamicCircle(520, 435, 15f, "Ball", true);            
            player = entityFactory.CreateDynamicPlayer(917, 752, 30, "mikze", true);
            enemyPlayer = entityFactory.CreateDynamicPlayer(112, 752, 30, "mikze2", true);
            foot = entityFactory.CreateDynamicBox(250, 350, new Vector2(60, 10), "foot", true);
            enemyFoot = entityFactory.CreateDynamicBox(250, 350, new Vector2(60, 10), "foot", true);
            legJoint = entityFactory.CreateRevoluteJointJoint(player, foot);
            enemyLegJoint = entityFactory.CreateRevoluteJointJoint(enemyPlayer, enemyFoot, true);
            playerFootSpeed = legJoint.MotorSpeed;
            enemyFootSpeed = enemyLegJoint.MotorSpeed;

            ballPosition = ball.body.Position;
            playerPosition = player.body.Position;
            enemyPlayerPosition = enemyPlayer.body.Position;

            //entityFactory.CreateDistanceJointJoint(player, foot);
            //entityFactory.CreateDistanceJointJoint(enemyPlayer, enemyFoot);
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
            isLoaded = true;
        }

        private void EnemyScore()
        {
            resetPositions = true;
            _ = ChatService._chatroomService.BroadcastMessageAsync(new Message() { NickName = "Server", Msg = "EnemyScore" });
        }

        private void PlayerScore()
        {
            resetPositions = true;
            _ = ChatService._chatroomService.BroadcastMessageAsync(new Message() { NickName = "Server", Msg = "PlayerScore" });          
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
            _stateHandler.state = States.LobbyState;
            _stateHandler.ChangeScene(new LobbyState());
        }
    }
}
