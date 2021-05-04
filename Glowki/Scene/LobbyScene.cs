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
using System.Threading;
using Systems;

namespace Scenes
{
    public enum skin
    {
        MiniHead1,
        MiniHead2,
        MiniHead3
    }

    internal class LobbyScene : Scene
    {
        World world;
        GuiSystem _guiSystem;
        TextBox chatText;
        skin playerSkin;
        Entity readyText, enemyReadyText,text, enemyText, playerReadyButtom, enemyReadyButtom;
        EntityFactory entityFactory;
        MouseSystem mouseSystem;
        private SpriteBatch spriteBatch;

        private Sprite playerSprite, enemySprite, upArrowSprite, downArrowSprite;
        private Transform2 plyerTextureTransform, enemyTextureTransform, upArrowTransform, downArrowTransForm;

        bool ready = false;
        private bool? live = null;
        public LobbyScene()
        {                   
            plyerTextureTransform = new Transform2(new Vector2(640, 102));
            upArrowTransform = new Transform2(new Vector2(690, 80));
            downArrowTransForm = new Transform2(new Vector2(690, 120));
            enemyTextureTransform = new Transform2(new Vector2(640, 210));    
        }
        public override void LoadContent()
        {          
            ProtoHelper.OnChatRecieve += OnChatRecive;
            _ = ProtoHelper.JoinToChatAsync();
            var camera = new OrthographicCamera(_sceneHandler._graphicsDevice);
            mouseSystem = new MouseSystem();

            MouseSystem.OnMouseLeftClick += mouseClick;
            spriteBatch = new SpriteBatch(_sceneHandler._graphicsDevice);
            world = new WorldBuilder()
                .AddSystem(new RenderSystem(spriteBatch, camera))
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

            while (!ProtoHelper.IsChatLive)
                Thread.Sleep(10);

            Thread.Sleep(500);
            ProtoHelper.WriteToChat("!live");


            while (live == null)
                Thread.Sleep(10);

            if ((bool)live == true)
                ChangeStateToGame();

            var player = _sceneHandler._content.Load<Texture2D>("MiniHead1");
            var upArrow = _sceneHandler._content.Load<Texture2D>("MiniHead1");
            playerSprite = new Sprite(player);
            enemySprite = new Sprite(player);
            upArrowSprite = new Sprite(_sceneHandler._content.Load<Texture2D>("upArrow"));
            downArrowSprite = new Sprite(_sceneHandler._content.Load<Texture2D>("downArrow"));
        }

        private void mouseClick()
        {
            if (mouseSystem.Clicked(new Vector2(500, 110), new Vector2(16, 16)))
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
            if (mouseSystem.Clicked(new Vector2(690, 80), new Vector2(16, 16)))
            {
                var s = playerSkin + 1;
                playerSkin = s <= (skin)Enum.GetValues(typeof(skin)).Length-1 ? s : 0;
                playerSprite = new Sprite(_sceneHandler._content.Load<Texture2D>(playerSkin.ToString()));
                ProtoHelper.WriteToChat($"!SkinChange,{(int)playerSkin}");
            }
            if (mouseSystem.Clicked(new Vector2(690, 120), new Vector2(16, 16)))
            {
                var s = playerSkin - 1;
                playerSkin = s < 0 ? (skin)Enum.GetValues(typeof(skin)).Length-1 : s;
                playerSprite = new Sprite(_sceneHandler._content.Load<Texture2D>(playerSkin.ToString()));
                ProtoHelper.WriteToChat($"!SkinChange,{(int)playerSkin}");
            }
        }

        void OnChatRecive(string nickNmae, string message)
        {
            if (nickNmae.ToLower() == "server")
            {
                if (message.ToLower() == "!live")
                    live = true;
                if (message.ToLower() == "!notlive")
                    live = false;
                if (message.ToLower().Contains("!skin"))
                {
                    var skin = (skin)int.Parse(message.Split(",")[1]);
                    enemySprite = new Sprite(_sceneHandler._content.Load<Texture2D>(skin.ToString()));
                }

            }

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
                OnlineGameScene.Enemy = true;
            }
            else
            {
                var t = enemyText.Get<Text>();
                t.text = $"{nickNmae}: {message}";
            }
        }
        internal override void DrawScene(GameTime gameTime)
        {
            if (spriteBatch != null && playerSprite != null && plyerTextureTransform != null)
            {
                spriteBatch?.Begin();
                spriteBatch?.Draw(playerSprite, plyerTextureTransform);
                spriteBatch?.Draw(enemySprite, enemyTextureTransform);
                spriteBatch?.Draw(upArrowSprite, upArrowTransform);
                spriteBatch?.Draw(downArrowSprite, downArrowTransForm);
                spriteBatch?.End();
            }
            
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
            _sceneHandler.ChangeScene(new OnlineGameScene(ProtoHelper._IP));
        }
    }
}
