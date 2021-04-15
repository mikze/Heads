using Glowki;
using Glowki.Components;
using Glowki.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Gui.Controls;
using MonoGame.Extended.Input;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.ViewportAdapters;
using System;
using Systems;

namespace Scenes
{
    internal class LobbyScene : Scene
    {
        World world;
        GuiSystem _guiSystem;
        TextBox chatText;

        Entity readyText, enemyReadyText,text, enemyText, playerReadyButtom, enemyReadyButtom;
        EntityFactory entityFactory;
        MouseSystem mouseSystem;
        bool ready = false;

        public override void LoadContent()
        {          
            ProtoHelper.OnChatRecieve += OnChatRecive;
            _ = ProtoHelper.JoinToChatAsync();
            var camera = new OrthographicCamera(_sceneHandler._graphicsDevice);
            mouseSystem = new MouseSystem();

            MouseSystem.OnMouseLeftClick += mouseClick;
            world = new WorldBuilder()
                .AddSystem(new RenderSystem(new SpriteBatch(_sceneHandler._graphicsDevice), camera, _sceneHandler._content))
                .AddSystem(mouseSystem)
                .Build();

            _sceneHandler._gameComponents.Add(world);

            entityFactory = new EntityFactory(world, _sceneHandler._content, _sceneHandler._graphicsDevice);

            text = entityFactory.CreateText(new Vector2(100, 100), ProtoHelper.nickname, 100);
            playerReadyButtom= entityFactory.CreateReadySign(new Vector2(500,110));
            readyText = entityFactory.CreateText(new Vector2(530, 102), "Not ready", 0);
            enemyText = entityFactory.CreateText(new Vector2(100, 200), "", 100);
            enemyReadyButtom = entityFactory.CreateReadySign(new Vector2(500, 210));
            enemyReadyText = entityFactory.CreateText(new Vector2(530, 202), "Not ready", 0);


            LoadGui();
        }

        private void mouseClick()
        {
            if(mouseSystem.Clicked(new Vector2(500, 110), new Vector2(16, 16)))
            {
                var _playerReadyButtom = playerReadyButtom.Get<AnimatedSprite>();
                if (!ready)
                {
                    _playerReadyButtom.Play("Ready");
                    ProtoHelper.WriteToChat("!Ready");

                    var _ReadyText = readyText.Get<Text>();
                    _ReadyText.text = "Ready";
                }
                else
                {
                    _playerReadyButtom.Play("NotReady");
                    ProtoHelper.WriteToChat("!NotReady");

                    var _ReadyText = readyText.Get<Text>();
                    _ReadyText.text = "Not ready";
                }
                ready = !ready;
            }
        }

        void OnChatRecive(string nickNmae, string message)
        {
            if (message == "!Ready")
            {
                var _enemyReadyButtom = enemyReadyButtom.Get<AnimatedSprite>();
                var _enemyReadyText = enemyReadyText.Get<Text>();
                _enemyReadyButtom.Play("Ready");
                _enemyReadyText.text = "Ready";
            }
            else if(message == "!NotReady")
            {
                var _enemyReadyButtom = enemyReadyButtom.Get<AnimatedSprite>();
                var _enemyReadyText = enemyReadyText.Get<Text>();
                _enemyReadyButtom.Play("NotReady");
                _enemyReadyText.text = "Not ready";
            }
            else if(message == "Start" && nickNmae == "Server")
            {
                ChangeStateToGame();
            }
            else if (message == "!Enemy" && nickNmae == "Server")
            {
                TestOnlinceScene.Enemy = true;
            }
            else
            {
                var t = enemyText.Get<Text>();
                t.text = $"{nickNmae}: {message}";
            }
        }
        internal override void DrawScene(GameTime gameTime)
        {        
            _guiSystem.Draw(gameTime);
        }

        internal override void UpdateScene(GameTime gameTime)
        {
            var keyboardState = KeyboardExtended.GetState();
            if (keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                ProtoHelper.WriteToChat(chatText.Text);
                var t = text.Get<Text>();
                t.text = $"{ProtoHelper.nickname}: {chatText.Text}";
            }

            _guiSystem.Update(gameTime);
        }
        void LoadGui()
        {
            var viewportAdapter = new DefaultViewportAdapter(_sceneHandler._graphicsDevice);
            var guiRenderer = new GuiSpriteBatchRenderer(_sceneHandler._graphicsDevice, () => Matrix.Identity);
            var font = _sceneHandler._content.Load<BitmapFont>("Sensation");
            var JoinButton = new Button { Content = "Start" };

            JoinButton.PressedStateChanged += OnJoinButtonClick;
            BitmapFont.UseKernings = false;
            Skin.CreateDefault(font);
            chatText = new TextBox { Text = "Send message", Position = new Point(0, 150) };
            var controlTest = new StackPanel
            {
                Items =
                {
                    chatText,
                    JoinButton
                },
                VerticalAlignment = VerticalAlignment.Bottom
            };

            var demoScreen = new Screen
            {
                Content = controlTest,       
            };
            _guiSystem = new GuiSystem(viewportAdapter, guiRenderer) { ActiveScreen = demoScreen };
        }

        private void OnJoinButtonClick(object sender, EventArgs e)
        {
            ProtoHelper.WriteToChat("!Start");
        }

        private void ChangeStateToGame()
        {
            world.Dispose();
            _sceneHandler.ChangeScene(new TestOnlinceScene(ProtoHelper._IP));
        }
    }
}
