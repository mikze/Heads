﻿using Blaster.Systems;
using Box2D.NetStandard.Dynamics.Bodies;
using Glowki;
using Glowki.Components;
using Glowki.Interfaces;
using Glowki.Systems;
using Glowki.UDP;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Input;
using MonoGame.Extended.Sprites;
using SimpleUDP.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Systems;
using B2DWorld = Box2D.NetStandard.Dynamics.World.World;

namespace Scenes
{
    internal class OnlineGameScene : Scene
    {
        public static bool Enemy = false;
        World world;
        EntityFactory entityFactory;
        Client<Frame> client;
        Client<byte> inputClient;
        IPEndPoint inputEndPoint;
        string _ip;
        Entity points, enemyPoints;
        private Sprite wallSprite;
        private Transform2 transform;
        B2DWorld b2DWorld;
        int score = 0;
        int enemyScore = 0;
        private SpriteBatch spriteBatch;
        Dictionary<int, Entity> bonuses = new Dictionary<int, Entity>();

        public OnlineGameScene(string ip)
        {
            _ip = ip;
            inputEndPoint = new IPEndPoint(IPAddress.Parse(_ip), 1338);
        }

        public override void LoadContent()
        {
            var wallTexture = _sceneHandler._content.Load<Texture2D>("background2");
            wallSprite = new Sprite(wallTexture);
            transform = new Transform2(new Vector2(512, 400));

            b2DWorld = new B2DWorld(new System.Numerics.Vector2(0.0f, 1.500000000000000e+01f));
            client = new Client<Frame>(0);
            client.Connect(new IPEndPoint(IPAddress.Parse(_ip), 1337));
            inputClient = new Client<byte>(0);
            inputClient.Connect(inputEndPoint);

            var xx = ProtoHelper.LoadEntities();
            var camera = new OrthographicCamera(_sceneHandler._graphicsDevice);

            ProtoHelper.OnChatRecieve += OnChatRecive;
            spriteBatch = new SpriteBatch(_sceneHandler._graphicsDevice);
            world = new WorldBuilder()
                    .AddSystem(new RenderSystem(spriteBatch, camera))
                    .AddSystem(new PhysicsSystem())
                    .AddSystem(new MouseSystem())
                    .AddSystem(new MovementSystem())
                    .Build();

            _sceneHandler._gameComponents.Add(world);

            entityFactory = new EntityFactory(
                b2DWorld,
                world,
                _sceneHandler._content,
                _sceneHandler._graphicsDevice);

            foreach (var x in xx)
            {
                    if (x.Kind == 2)
                        entityFactory.CreateDynamicCircle(new Vector2(x.PositionX, x.PositionY), x.SizeX).Get<IRigidBody>().id = x.Id;
                    if (x.Kind == 3)
                    {
                        var pars = x.Params.Split(",");
                        var skin = (skin)int.Parse(pars[1]);
                        var player = entityFactory.CreatePlayer(x.Params, new Vector2(x.PositionX, x.PositionY), skin.ToString());
                        player.Get<IRigidBody>().id = x.Id;
                    }
                    if (x.Params == "foot")
                        entityFactory.CreateFoot(new Vector2(x.PositionX, x.PositionY), new Vector2(x.SizeX, x.SizeY)).Get<IRigidBody>().id = x.Id;
                    if (x.Params == "rfoot")
                    entityFactory.CreateReverseFoot(new Vector2(x.PositionX, x.PositionY), new Vector2(x.SizeX, x.SizeY)).Get<IRigidBody>().id = x.Id;
            }

            points = entityFactory.CreateText(new Vector2(50,50), "0", 0);
            enemyPoints = entityFactory.CreateText(new Vector2(720, 50), "0", 0);

            new TaskFactory().StartNew(() => 
            {
                while (true)
                {
                    try
                    {
                        var msg = client.Listen();
                        var RigitBody = BodyFactory.RigidBodies.FirstOrDefault(x => x.id == msg.id);
                        if (RigitBody != null)
                        {
                            RigitBody.Position = new System.Numerics.Vector2(msg.X, msg.Y);
                            RigitBody.Angle = msg.R;
                        }
                    }
                    catch { }
                }
            }
            );

            IsLoaded = true;
        }
        private void OnChatRecive(string nickNmae, string message)
        {
            if (nickNmae == "Server")
            {
                if (message == "PlayerScore")
                {
                    var _points = points.Get<Text>();
                    score++;
                    _points.text = score.ToString();
                }
                if (message == "EnemyScore")
                {
                    var _points = enemyPoints.Get<Text>();
                    enemyScore++;
                    _points.text = enemyScore.ToString();
                }
                if(message.Contains("bonus"))
                {
                    var msg = message.Split(",");
                    var X = int.Parse(msg[2]);
                    var Y = int.Parse(msg[3]);
                    var id = int.Parse(msg[4]);
                    if (msg[1].Contains("speed"))
                    {
                        var entity = entityFactory.BonusSpeed(new Vector2(X, Y));
                        entity.Get<IRigidBody>().id = id;
                        bonuses.Add(id, entity);
                    }
                }
                if (message.Contains("destroy"))
                {
                    Console.WriteLine("Remove bonus");
                    var msg = message.Split(",");
                    var id = int.Parse(msg[1]);
                    world.DestroyEntity(bonuses[id]);
                    bonuses.Remove(id);               
                }
            }
        }

        internal override void DrawScene(GameTime gameTime)
        {
            if (spriteBatch != null && wallSprite != null && transform != null)
            { 
                spriteBatch?.Begin();
                spriteBatch?.Draw(wallSprite, transform);
                spriteBatch?.End();
            }
        }

        internal override void UpdateScene(GameTime gameTime)
        {
            if (IsLoaded)
            {
                if (b2DWorld != null)
                {
                    const float TimeStep = 1.0f / 60;
                    const int VelocityIterations = 6;
                    const int PositionIterations = 2;
                    b2DWorld.Step(TimeStep, VelocityIterations, PositionIterations);
                }

                var keyboardState = KeyboardExtended.GetState();

                var input = Input.none;

                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    input |= Input.right;
                }
                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    input |= Input.left;
                }
                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    input |= Input.up;
                }
                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    input |= Input.kick;
                }
                if (keyboardState.IsKeyUp(Keys.Space))
                {
                    input |= Input.kickUp;
                }
                if (keyboardState.IsKeyDown(Keys.Escape))
                {
                    world.Dispose();
                    ProtoHelper.WriteToChat("!disconnect");
                    _sceneHandler.ChangeScene(new MenuScene());
                }
                if (Enemy)
                {
                    input |= Input.enemy;
                }

                inputClient.Send((byte)input, inputEndPoint);
            }
        }
    }
}
