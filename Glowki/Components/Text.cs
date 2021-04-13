using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Glowki.Components
{
    public class Text
    {
        public System.Timers.Timer timer;

        public Text(string text, SpriteFont font)
        {
            this.text = text;
            this.font = font;
        }
        public string text { get; set; }
        public SpriteFont font { get; set; }
    }
}
