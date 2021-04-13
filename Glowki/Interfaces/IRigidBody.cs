using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Glowki.Interfaces
{
    public interface IRigidBody
    {
        public bool IsStatic { get; set; }
        public int id { get; set; }
        public Vector2 Position { get; set; }
        public float Angle { get; set; }
        public Vector2 GetPosition();
        public void ApplyForceToCenter(Vector2 v);
        public void ApplyLinearImpulseToCenter(Vector2 v);
    }
}
