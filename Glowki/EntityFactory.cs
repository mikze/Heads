using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Graphics;
using System.Collections.Generic;
using B2DWorld = Box2D.NetStandard.Dynamics.World.World;
using Glowki.Components;
using Glowki.Interfaces;
using Text = Glowki.Components.Text;
using MonoGame.Extended.TextureAtlases;

namespace Glowki
{
    public class EntityFactory
    {
        public World _world;
        private B2DWorld _Box2dWorld;
        private GraphicsDevice _graphicsDevice;
        private IRigitBodyFactory _BodyFactory;
        private ContentManager _contentManager;
        public EntityFactory(B2DWorld Box2dWorld, World world, ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            _Box2dWorld = Box2dWorld;
            _world = world;
            _graphicsDevice = graphicsDevice;
            _BodyFactory = BodyFactory.GetFactory();
            _contentManager = contentManager;
        }

        public EntityFactory(World world, ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            _world = world;
            _graphicsDevice = graphicsDevice;
            _BodyFactory = BodyFactory.GetFactory();
            _contentManager = contentManager;
        }

        public Entity CreateText(Vector2 position, string text, int seconds)
        {
            var font = _contentManager.Load<SpriteFont>("Score");
            var entity = _world.CreateEntity();

            entity.Attach(new Transform2(position, 0, Vector2.One * 4));
            entity.Attach(new Text(text, font));
            return entity;
        }

        public Entity CreatePlayer(string name,Vector2 position)
        {
            var entity = CreateDynamicCircle(position, 30f);
            entity.Attach(new Player(name));
            return entity;
        }
        public Entity CreateDynamicBox(Vector2 position, Vector2 size)
        {
            Texture2D rect = new Texture2D(_graphicsDevice, (int)size.X, (int)size.Y);
            Color[] data = new Color[(int)size.X * (int)size.Y];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Chocolate;
            rect.SetData(data);

            var entity = _world.CreateEntity();
            entity.Attach(new Sprite(rect));
            entity.Attach(new Transform2(position, 0, Vector2.One));
            entity.Attach(_BodyFactory.CreateDynamicBox(position, size));
            return entity;
        }

        public Entity CreateDynamicCircle(Vector2 position, float radius)
        {
            var wallTexture = _contentManager.Load<Texture2D>("ball");
            var wallSprite = new Sprite(wallTexture);
            var entity = _world.CreateEntity();

            var scale = (wallTexture .Height/2)/ radius;
            entity.Attach(wallSprite);
            var transform = new Transform2(position, 0, Vector2.One/scale);

            entity.Attach(transform);
            entity.Attach(_BodyFactory.CreateDynamicCircle(position, radius/100));
            return entity;
        }

        public Entity CreateStaticBox(Vector2 position, Vector2 size)
        {
            Texture2D rect = new Texture2D(_graphicsDevice, (int)size.X, (int)size.Y);

            Color[] data = new Color[(int)size.X * (int)size.Y];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Chocolate;
            rect.SetData(data);

            var entity = _world.CreateEntity();

            entity.Attach(new Sprite(rect));
            entity.Attach(new Transform2(position, 0, Vector2.One));
            entity.Attach(_BodyFactory.CreateStaticBox(position, size));
            return entity;
        }

        public Entity CreateStaticBoxNoColl(Vector2 position, Vector2 size)
        {
            Texture2D rect = new Texture2D(_graphicsDevice, (int)size.X, (int)size.Y);

            Color[] data = new Color[(int)size.X * (int)size.Y];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.Chocolate;
            rect.SetData(data);

            var entity = _world.CreateEntity();

            entity.Attach(new Sprite(rect));
            entity.Attach(new Transform2(position, 0, Vector2.One));
            return entity;
        }

        public Entity CreateDynamicTriangle(Vector2 position)
        {
            var wallTexture = _contentManager.Load<Texture2D>("Trojkat");
            var wallSprite = new Sprite(wallTexture);
            var entity = _world.CreateEntity();

            entity.Attach(wallSprite);
            var transform = new Transform2(position, 0, Vector2.One);

            entity.Attach(transform);
            entity.Attach(_BodyFactory.CreateStaticTriangle(position));
            return entity;
        }

        public Entity CreateReadySign(Vector2 position)
        {
            var ReadyTexture = _contentManager.Load<Texture2D>("Ready");
            var readyAtlas = TextureAtlas.Create("dudeAtlas", ReadyTexture, 16, 16);
            var spriteSheet = new SpriteSheet { TextureAtlas = readyAtlas };

            AddAnimationCycle(spriteSheet, "Ready", new[] { 0 });
            AddAnimationCycle(spriteSheet, "NotReady", new[] { 1 });

            var entity = _world.CreateEntity();

            entity.Attach(new AnimatedSprite(spriteSheet, "NotReady"));
            var transform = new Transform2(position, 0, Vector2.One);

            entity.Attach(transform);
            return entity;
        }
        private void AddAnimationCycle(SpriteSheet spriteSheet, string name, int[] frames, bool isLooping = true, float frameDuration = 0.1f)
        {
            var cycle = new SpriteSheetAnimationCycle();
            foreach (var f in frames)
            {
                cycle.Frames.Add(new SpriteSheetAnimationFrame(f, frameDuration));
            }

            cycle.IsLooping = isLooping;

            spriteSheet.Cycles.Add(name, cycle);
        }

    }
}
