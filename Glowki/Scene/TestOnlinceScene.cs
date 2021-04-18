using Blaster.Systems;
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
    internal class TestOnlinceScene : Scene
    {
        public static bool Enemy = false;
        World world;
        EntityFactory entityFactory;
        Client<Frame> client;
        Client<byte> inputClient;
        IPEndPoint inputEndPoint;
        string _ip;
        Entity points, enemyPoints;
        B2DWorld b2DWorld;
        int score = 0;
        int enemyScore = 0;
        public TestOnlinceScene(string ip)
        {
            _ip = ip;
            inputEndPoint = new IPEndPoint(IPAddress.Parse(_ip), 1338);
        }

        public override void LoadContent()
        {
            b2DWorld = new B2DWorld(new System.Numerics.Vector2(0.0f, 1.500000000000000e+01f));
            client = new Client<Frame>(0);
            client.Connect(new IPEndPoint(IPAddress.Parse(_ip), 1337));
            inputClient = new Client<byte>(0);
            inputClient.Connect(inputEndPoint);

            var xx = ProtoHelper.LoadEntities();
            var camera = new OrthographicCamera(_sceneHandler._graphicsDevice);

            ProtoHelper.OnChatRecieve += OnChatRecive;

            world = new WorldBuilder()
                    .AddSystem(new RenderSystem(new SpriteBatch(_sceneHandler._graphicsDevice), camera, _sceneHandler._content))
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
                if (!x.Params.Contains("Goal")) {
                    if (x.Kind == 2)
                        entityFactory.CreateDynamicCircle(new Vector2(x.PositionX, x.PositionY), 30f).Get<IRigidBody>().id = x.Id;
                    if (x.Kind == 3)
                    {
                        var player = entityFactory.CreatePlayer(x.Params, new Vector2(x.PositionX, x.PositionY));
                        player.Get<IRigidBody>().id = x.Id;
                    }
                    else
                        entityFactory.CreateStaticBox(new Vector2(x.PositionX, x.PositionY), new Vector2(x.SizeX, x.SizeY)).Get<IRigidBody>().id = x.Id;
                }
            }

            points = entityFactory.CreateText(new Vector2(50,50), "0", 0);
            enemyPoints = entityFactory.CreateText(new Vector2(720, 50), "0", 0);

            new TaskFactory().StartNew(() => 
            {
                while (true)
                {
                    var msg = client.Listen();
                    var RigitBody = BodyFactory.RigidBodies.FirstOrDefault(x => x.id == msg.id);
                    if (RigitBody != null)
                    {
                        RigitBody.Position = new System.Numerics.Vector2(msg.X, msg.Y);
                        RigitBody.Angle = msg.R;
                    }
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
            }
        }

        internal override void DrawScene(GameTime gameTime)
        {
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
                if(Enemy)
                {
                    input |= Input.enemy;
                }

                inputClient.Send((byte)input, inputEndPoint);
            }
        }
    }
}
