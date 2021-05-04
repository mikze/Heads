using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Entities.Systems;
using System;

namespace Glowki.Systems
{
    public class MouseSystem : UpdateSystem
    {
        public static Action OnMouseRightClick;
        public static Action OnMouseLeftClick;
        public static Vector2 mousePosition;
        public static Vector2 bodyCentre;

        bool LeftButtonmouseClick = false;
        bool RightButtommouseClick = false;
        public override void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            mousePosition = mouseState.Position.ToVector2();

            if (mouseState.LeftButton == ButtonState.Pressed && !LeftButtonmouseClick)
            {
                LeftButtonmouseClick = true;
                OnMouseLeftClick?.Invoke();
            }

            if (mouseState.LeftButton == ButtonState.Released)
                LeftButtonmouseClick = false;

            if (mouseState.RightButton == ButtonState.Pressed && !RightButtommouseClick)
            {
                RightButtommouseClick = true;
                OnMouseLeftClick?.Invoke();
            }

            if (mouseState.RightButton == ButtonState.Released)
                RightButtommouseClick = false;
        }

        public bool Clicked(Vector2 position, Vector2 size)
        {
            if(mousePosition.X > position.X - size.X/2 && mousePosition.X < position.X + size.X/2)
                if (mousePosition.Y > position.Y - size.X / 2 && mousePosition.Y < position.Y + size.Y/2)
                    return true;

            return false;
        }
    }
}
