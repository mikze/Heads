
using Glowki;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Gui;
using MonoGame.Extended.Gui.Controls;
using MonoGame.Extended.ViewportAdapters;
using Systems;

namespace Scenes
{
    internal class MenuScene : Scene
    {
        MonoGame.Extended.Entities.World world;
        GuiSystem _guiSystem;
        TextBox NameTextBox;
        TextBox IPTextBox;

        public override void LoadContent()
        {
            var camera = new OrthographicCamera(_sceneHandler._graphicsDevice);
            world = new WorldBuilder()
                .AddSystem(new RenderSystem(new SpriteBatch(_sceneHandler._graphicsDevice), camera))
                .Build();

            _sceneHandler._gameComponents.Add(world);

            var entityFactory = new EntityFactory(world, _sceneHandler._content, _sceneHandler._graphicsDevice);

            entityFactory.CreateText(new Vector2(100, 100), "MenuScene", 100);

            LoadGui();
        }

        internal override void DrawScene(GameTime gameTime)
        {
            _guiSystem.Draw(gameTime);
        }

        internal override void UpdateScene(GameTime gameTime)
        {
            _guiSystem.Update(gameTime);
        }
        void LoadGui()
        {
            var viewportAdapter = new DefaultViewportAdapter(_sceneHandler._graphicsDevice);
            var guiRenderer = new GuiSpriteBatchRenderer(_sceneHandler._graphicsDevice, () => Matrix.Identity);
            var font = _sceneHandler._content.Load<BitmapFont>("Sensation");
            BitmapFont.UseKernings = false;
            Skin.CreateDefault(font);
            var JoinButton = new Button { Content = "Join to server" };
            IPTextBox = new TextBox { Text = "127.0.0.1", HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom };
            NameTextBox = new TextBox { Text = "Name", HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Bottom };
            JoinButton.PressedStateChanged += ChatText_PressedStateChanged;
            var controlTest = new StackPanel
            {
                Items =
                {
                    new StackPanel
                    {
                        Items={
                            new Label("Name:") { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Bottom, },
                            NameTextBox,
                        },
                        Orientation = Orientation.Horizontal
                    },
                    new StackPanel
                    {
                        Items={
                            new Label("Ip:") { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Bottom, },
                            IPTextBox,
                        },
                        Orientation = Orientation.Horizontal
                    },
                    JoinButton
                }
                ,
                VerticalAlignment = VerticalAlignment.Centre,
                HorizontalAlignment = HorizontalAlignment.Centre,
                Spacing = 5
            };


            var demoScreen = new Screen
            {
                Content = controlTest
            };

            _guiSystem = new GuiSystem(viewportAdapter, guiRenderer) { ActiveScreen = demoScreen };
        }

        private void ChatText_PressedStateChanged(object sender, System.EventArgs e)
        {
            world.Dispose();
            ProtoHelper.nickname = NameTextBox.Text;
            ProtoHelper._IP = IPTextBox.Text;
            _sceneHandler.ChangeScene(new LobbyScene());
        }
    }
}
