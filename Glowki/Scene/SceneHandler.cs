using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scenes
{
    internal class SceneHandler
    {
        private Scene _scene = null;

        public GameComponentCollection _gameComponents;
        public GraphicsDevice _graphicsDevice;
        public ContentManager _content;

        public SceneHandler(Scene scene, GameComponentCollection gameComponents, GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            _gameComponents = gameComponents;
            _graphicsDevice = graphicsDevice;
            _content = contentManager;
            _scene = scene;
        }

        public void ChangeScene(Scene scene)
        {
            _scene = scene;
            _scene.SetSceneHandler(this);
            LoadScene();
        }

        public void DrawScene(GameTime gameTime) => _scene.DrawScene(gameTime);

        public void UpdateScene(GameTime gameTime) => _scene.UpdateScene(gameTime);

        public void LoadScene() => _scene.LoadContent();
    }
}
