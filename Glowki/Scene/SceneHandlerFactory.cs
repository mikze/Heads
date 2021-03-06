using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scenes
{
    internal static class SceneHandlerFactory
    {
        public static SceneHandler CreateSceneHandler(GameComponentCollection gameComponents, GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            var scene = new MenuScene();
            var sceneHandler = new SceneHandler(scene, gameComponents, graphicsDevice, contentManager);
            scene.SetSceneHandler(sceneHandler);
            sceneHandler.LoadScene();
            return sceneHandler;
        }
    }
}
