using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scenes
{
    internal abstract class Scene
    {
        protected SceneHandler _sceneHandler;

        internal abstract void DrawScene(GameTime gameTime);
        public void SetSceneHandler(SceneHandler sceneHandler) => _sceneHandler = sceneHandler;
        public abstract void LoadContent();
        public bool IsLoaded { get; set; }
        internal abstract void UpdateScene(GameTime gameTime);
    }
}
